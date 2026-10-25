using System.Net;
using System.Net.Sockets;
using System.Text;
using OneVOneTimer.Stream;

namespace OneVOneTimer.Tests;

/// <summary>The stream page (an OBS Browser Source): served from this PC to this PC only, with every change pushed to it.</summary>
public static class StreamOverlayTests
{
    // Ports of their own, away from a running timer's 47123.
    static int _next = 47300;

    static StreamOverlay Start() => new(Interlocked.Add(ref _next, 3) - 3, 3);

    static OverlayState State(long ms1, bool run1, long ms2 = 0, int selected = 0) =>
        new([new(ms1, run1), new(ms2, false)], selected, new("#27B574", "#0099DA", "#FF5400", "#000000"), false);

    static (string Status, string Body) Get(StreamOverlay overlay, string path, string? host = null, string method = "GET")
    {
        using var client = new TcpClient();
        client.Connect(IPAddress.Loopback, overlay.Port);
        var stream = client.GetStream();
        stream.ReadTimeout = 3000;
        byte[] request = Encoding.ASCII.GetBytes($"{method} {path} HTTP/1.1\r\nHost: {host ?? $"127.0.0.1:{overlay.Port}"}\r\nConnection: close\r\n\r\n");
        stream.Write(request);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string all = reader.ReadToEnd();
        int split = all.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        return (all[..all.IndexOf("\r\n", StringComparison.Ordinal)], split < 0 ? "" : all[(split + 4)..]);
    }

    [Test]
    public static void It_listens_on_this_pc_only()
    {
        using var overlay = Start();
        Assert.Equal($"http://127.0.0.1:{overlay.Port}/", overlay.Url, "the link");
    }

    [Test]
    public static void The_page_is_served_with_its_live_line()
    {
        using var overlay = Start();
        var (status, body) = Get(overlay, "/");
        Assert.Equal("HTTP/1.1 200 OK", status, "status");
        Assert.True(body.Contains("new EventSource(\"/events\")", StringComparison.Ordinal), "the page listens for changes");
        Assert.True(body.Contains("LAST20_MS = 20000", StringComparison.Ordinal), "with the Last 20 Seconds rule");
        Assert.True(body.Contains("<svg", StringComparison.Ordinal), "and draws to fit any size");
    }

    [Test]
    public static void The_state_is_what_the_timer_published()
    {
        using var overlay = Start();
        overlay.Publish(State(61480, true, 0, selected: 0));
        var (status, body) = Get(overlay, "/state");
        Assert.Equal("HTTP/1.1 200 OK", status, "status");
        Assert.True(body.Contains("\"ms\":61480", StringComparison.Ordinal) && body.Contains("\"running\":true", StringComparison.Ordinal), "the time and that it runs: " + body);
        Assert.True(body.Contains("\"last20\":\"#FF5400\"", StringComparison.Ordinal), "the colors");
    }

    [Test]
    public static void Every_change_reaches_an_open_page()
    {
        using var overlay = Start();
        overlay.Publish(State(1000, false));
        using var client = new TcpClient();
        client.Connect(IPAddress.Loopback, overlay.Port);
        var stream = client.GetStream();
        stream.ReadTimeout = 3000;
        stream.Write(Encoding.ASCII.GetBytes($"GET /events HTTP/1.1\r\nHost: 127.0.0.1:{overlay.Port}\r\n\r\n"));
        using var reader = new StreamReader(stream, Encoding.UTF8);
        string? Next()
        {
            for (string? line = reader.ReadLine(); line != null; line = reader.ReadLine())
                if (line.StartsWith("data: ", StringComparison.Ordinal))
                    return line[6..];
            return null;
        }
        Assert.True(Next()?.Contains("\"ms\":1000", StringComparison.Ordinal) == true, "the timers as they are when the page opens");
        overlay.Publish(State(2500, true));
        Assert.True(Next()?.Contains("\"ms\":2500", StringComparison.Ordinal) == true, "then the change");
    }

    [Test]
    public static void Requests_from_elsewhere_are_turned_away()
    {
        using var overlay = Start();
        Assert.Equal("HTTP/1.1 403 Forbidden", Get(overlay, "/state", host: "evil.example").Status, "a web site's own name pointed at this PC");
        Assert.Equal("HTTP/1.1 200 OK", Get(overlay, "/state", host: $"localhost:{overlay.Port}").Status, "localhost is this PC");
        Assert.Equal("HTTP/1.1 405 Method Not Allowed", Get(overlay, "/state", method: "POST").Status, "nothing can be changed from outside");
        Assert.Equal("HTTP/1.1 404 Not Found", Get(overlay, "/settings.json").Status, "nothing else is served");
    }

    [Test]
    public static void A_port_in_use_moves_it_to_the_next()
    {
        int first = Interlocked.Add(ref _next, 3) - 3;
        var taken = new TcpListener(IPAddress.Loopback, first);
        taken.Start();
        try
        {
            using var overlay = new StreamOverlay(first, 3);
            Assert.Equal(first + 1, overlay.Port, "the next port");
        }
        finally
        {
            taken.Stop();
        }
    }
}
