using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace OneVOneTimer.Stream;

/// <summary>One timer as the stream page needs it: its time when sent, and whether it runs on from there.</summary>
public sealed record OverlayTimer(long Ms, bool Running);

public sealed record OverlayColors(string Timer, string Selected, string Last20, string Background);

/// <summary>Everything the stream page shows; it runs a running timer on by itself until the next change.</summary>
public sealed record OverlayState(OverlayTimer[] Timers, int Selected, OverlayColors Colors, bool Transparent);

/// <summary>
/// The timers on stream: a page for an OBS Browser Source at http://127.0.0.1:47123/ (Game Capture shows only the game,
/// and a see-through window does not show in a Window Capture). Served from this PC to this PC only (the loopback
/// address, never the network), read only, and quiet: the page keeps time itself and hears from the timer only when
/// something changes (Server-Sent Events), so with no OBS open it costs nothing.
/// </summary>
public sealed class StreamOverlay : IDisposable
{
    public const int FirstPort = 47123, PortsTried = 10;
    static readonly TimeSpan KeepAlive = TimeSpan.FromSeconds(15), RequestTimeout = TimeSpan.FromSeconds(5);
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    const int MaxRequest = 8192;

    readonly TcpListener? _listener;
    readonly CancellationTokenSource _stop = new();
    readonly object _gate = new();
    readonly List<Channel<string>> _pages = [];
    string _state = JsonSerializer.Serialize(new OverlayState(
        [new(0, false), new(0, false)], 0,
        new(Settings.ColorSettings.DefaultTimer, Settings.ColorSettings.DefaultSelected, Settings.ColorSettings.DefaultLast20, Settings.ColorSettings.DefaultBackground), false), Json);

    public StreamOverlay(int firstPort = FirstPort, int portsTried = PortsTried)
    {
        for (int port = firstPort; port < firstPort + portsTried && _listener == null; port++)
        {
            var listener = new TcpListener(IPAddress.Loopback, port) { ExclusiveAddressUse = true };
            try
            {
                listener.Start();
                _listener = listener;
                Port = port;
            }
            catch (SocketException)
            {
                // Another app has this port: the next one.
            }
        }
        if (_listener == null)
        {
            App.Log.Warn($"stream page: ports {firstPort}-{firstPort + portsTried - 1} are all in use");
            return;
        }
        _ = Task.Run(AcceptLoop);
    }

    public int Port { get; }

    /// <summary>The link to paste in OBS, or null when no port was free.</summary>
    public string? Url => _listener == null ? null : $"http://127.0.0.1:{Port}/";

    /// <summary>The page's words: the timer's own rules for the time, the colors and the size (Stream/overlay.html).</summary>
    static readonly byte[] Page = ReadPage();

    static byte[] ReadPage()
    {
        using var stream = typeof(StreamOverlay).Assembly.GetManifestResourceStream("overlay.html")
            ?? throw new InvalidOperationException("The stream page is missing from the program.");
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    /// <summary>The timers now: sent to every open page (only the newest counts, a page that falls behind skips to it).</summary>
    public void Publish(OverlayState state)
    {
        string json = JsonSerializer.Serialize(state, Json);
        lock (_gate)
        {
            _state = json;
            foreach (var page in _pages)
                page.Writer.TryWrite(json);
        }
    }

    async Task AcceptLoop()
    {
        while (!_stop.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener!.AcceptTcpClientAsync(_stop.Token);
            }
            catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException or SocketException)
            {
                if (_stop.IsCancellationRequested)
                    return;
                continue;
            }
            _ = Task.Run(() => Serve(client));
        }
    }

    async Task Serve(TcpClient client)
    {
        using (client)
        {
            try
            {
                client.NoDelay = true;
                var stream = client.GetStream();
                string? head = await ReadHead(stream);
                if (head == null)
                    return;
                string[] lines = head.Split("\r\n");
                string[] request = lines[0].Split(' ');
                if (request.Length < 3)
                    return;
                string path = request[1].Split('?')[0];
                string? host = lines.Skip(1).Where(l => l.StartsWith("Host:", StringComparison.OrdinalIgnoreCase))
                    .Select(l => l[5..].Trim()).FirstOrDefault();
                if (!HostAllowed(host, Port))
                    await Respond(stream, "403 Forbidden", "text/plain", "Forbidden"u8.ToArray());
                else if (request[0] != "GET")
                    await Respond(stream, "405 Method Not Allowed", "text/plain", "GET only"u8.ToArray());
                else if (path is "/" or "/overlay")
                    await Respond(stream, "200 OK", "text/html; charset=utf-8", Page);
                else if (path == "/state")
                    await Respond(stream, "200 OK", "application/json", Encoding.UTF8.GetBytes(CurrentState()));
                else if (path == "/events")
                    await Events(stream);
                else
                    await Respond(stream, "404 Not Found", "text/plain", "Not found"u8.ToArray());
            }
            catch (Exception e) when (e is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
            {
                // The page went away (OBS closed, the source hidden or reloaded).
            }
        }
    }

    /// <summary>
    /// Only pages that asked for this PC by its own name: a web site in a browser that points a name of its own at
    /// 127.0.0.1 (DNS rebinding) gets nothing.
    /// </summary>
    internal static bool HostAllowed(string? host, int port) => host != null
        && (host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) || host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.Equals($"127.0.0.1:{port}", StringComparison.OrdinalIgnoreCase) || host.Equals($"localhost:{port}", StringComparison.OrdinalIgnoreCase));

    string CurrentState()
    {
        lock (_gate)
            return _state;
    }

    async Task<string?> ReadHead(NetworkStream stream)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_stop.Token);
        timeout.CancelAfter(RequestTimeout);
        var buffer = new byte[MaxRequest];
        int length = 0;
        while (length < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(length), timeout.Token);
            if (read == 0)
                return null;
            length += read;
            int end = buffer.AsSpan(0, length).IndexOf("\r\n\r\n"u8);
            if (end >= 0)
                return Encoding.ASCII.GetString(buffer, 0, end);
        }
        return null; // a request this long is not a browser asking for the page
    }

    static async Task Respond(NetworkStream stream, string status, string type, byte[] body)
    {
        string head = $"HTTP/1.1 {status}\r\nContent-Type: {type}\r\nContent-Length: {body.Length}\r\nCache-Control: no-store\r\nConnection: close\r\n\r\n";
        await stream.WriteAsync(Encoding.ASCII.GetBytes(head));
        await stream.WriteAsync(body);
    }

    /// <summary>The page's live line: the timers now, then every change, and a quiet line every 15 s so OBS keeps it open.</summary>
    async Task Events(NetworkStream stream)
    {
        var page = Channel.CreateBounded<string>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest });
        lock (_gate)
        {
            _pages.Add(page);
            page.Writer.TryWrite(_state);
        }
        try
        {
            await stream.WriteAsync("HTTP/1.1 200 OK\r\nContent-Type: text/event-stream\r\nCache-Control: no-store\r\nConnection: keep-alive\r\n\r\n"u8.ToArray(), _stop.Token);
            while (!_stop.IsCancellationRequested)
            {
                string? message = null;
                using (var wait = CancellationTokenSource.CreateLinkedTokenSource(_stop.Token))
                {
                    wait.CancelAfter(KeepAlive);
                    try
                    {
                        if (await page.Reader.WaitToReadAsync(wait.Token))
                            page.Reader.TryRead(out message);
                    }
                    catch (OperationCanceledException) when (!_stop.IsCancellationRequested)
                    {
                        // Nothing new for a while: the quiet line.
                    }
                }
                await stream.WriteAsync(Encoding.UTF8.GetBytes(message != null ? $"data: {message}\n\n" : ": still here\n\n"), _stop.Token);
            }
        }
        finally
        {
            lock (_gate)
                _pages.Remove(page);
        }
    }

    public void Dispose()
    {
        _stop.Cancel();
        _listener?.Stop();
    }
}
