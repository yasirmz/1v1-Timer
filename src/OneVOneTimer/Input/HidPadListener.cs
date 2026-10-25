using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OneVOneTimer.Input;

/// <summary>
/// PlayStation (DualSense, DualShock 4) and other controllers that are not XInput, read with Raw Input on a message-only
/// window of their own thread (RIDEV_INPUTSINK: from every app, the game keeps every button). A controller reports up to
/// a thousand times a second, so the reports are not taken one by one: every few milliseconds the thread reads what came
/// in one batch (GetRawInputBuffer) and looks only at each controller's newest report, and with no controller it sleeps.
/// Presses are posted to the UI thread, as Xbox buttons (<see cref="PadMapping"/>).
/// </summary>
public sealed class HidPadListener : IDisposable
{
    /// <summary>How often the reports are read while a controller is there: far shorter than any press.</summary>
    public const int PollMs = 4;
    /// <summary>With no controller (nothing came in for a while), how often the thread looks.</summary>
    const int IdleMs = 250, IdleAfterMs = 2000;
    const uint RIDEV_REMOVE = 0x00000001, RIDEV_INPUTSINK = 0x00000100, RIM_TYPEHID = 2, PM_REMOVE = 1;
    /// <summary>Joystick, gamepad and multi-axis controller (HID Generic Desktop usages 4, 5 and 8).</summary>
    static readonly ushort[] Usages = [4, 5, 8];
    static readonly PadButton[] All = Enum.GetValues<PadButton>();

    readonly SynchronizationContext _ui;
    readonly Thread _thread;
    readonly ManualResetEventSlim _stop = new();

    public event Action<InputBinding>? Pressed;

    /// <param name="ui">Where presses are posted (the UI thread).</param>
    public HidPadListener(SynchronizationContext ui)
    {
        _ui = ui;
        _thread = new Thread(Run) { IsBackground = true, Name = "1v1 Timer HID controllers" };
        _thread.Start();
    }

    unsafe void Run()
    {
        var window = new NativeWindow();
        window.CreateHandle(new CreateParams { Parent = new IntPtr(-3) }); // HWND_MESSAGE
        uint deviceSize = (uint)Marshal.SizeOf<RawInputListener.RAWINPUTDEVICE>();
        var devices = Usages.Select(u => new RawInputListener.RAWINPUTDEVICE { UsagePage = 1, Usage = u, Flags = RIDEV_INPUTSINK, Target = window.Handle }).ToArray();
        if (!RawInputListener.RegisterRawInputDevices(devices, (uint)devices.Length, deviceSize))
        {
            App.Log.Warn($"raw input (controllers): not registered ({Marshal.GetLastWin32Error()})");
            window.DestroyHandle();
            return;
        }

        var pads = new HidPads();
        const int bufferSize = 64 * 1024;
        byte* buffer = (byte*)NativeMemory.AlignedAlloc(bufferSize, 16);
        var newest = new Dictionary<IntPtr, nint>();
        long lastReport = -IdleAfterMs;
        try
        {
            while (!_stop.Wait(Environment.TickCount64 - lastReport < IdleAfterMs ? PollMs : IdleMs))
            {
                if (Drain(pads, buffer, bufferSize, newest))
                {
                    if (lastReport < 0)
                        TimerResolution.Raise(); // a controller is there: the 4 ms waits are 4 ms
                    lastReport = Environment.TickCount64;
                }
                // Nothing else is for this window; the queue is kept empty.
                while (PeekMessage(out _, IntPtr.Zero, 0, 0, PM_REMOVE))
                {
                }
            }
        }
        finally
        {
            NativeMemory.AlignedFree(buffer);
            var remove = Usages.Select(u => new RawInputListener.RAWINPUTDEVICE { UsagePage = 1, Usage = u, Flags = RIDEV_REMOVE }).ToArray();
            RawInputListener.RegisterRawInputDevices(remove, (uint)remove.Length, deviceSize);
            window.DestroyHandle();
        }
    }

    /// <summary>Reads every report that came in since the last time; true when there was one.</summary>
    unsafe bool Drain(HidPads pads, byte* buffer, int bufferSize, Dictionary<IntPtr, nint> newest)
    {
        uint header = (uint)sizeof(RawInputListener.RAWINPUTHEADER);
        bool any = false;
        while (true)
        {
            uint size = (uint)bufferSize;
            uint count = GetRawInputBuffer(buffer, ref size, header);
            if (count == 0 || count == uint.MaxValue)
                return any;
            any = true;
            // The newest report of each controller in this batch (its buttons now; a press lasts far longer than a batch).
            newest.Clear();
            byte* at = buffer;
            for (uint i = 0; i < count; i++)
            {
                var head = (RawInputListener.RAWINPUTHEADER*)at;
                if (head->Type == RIM_TYPEHID)
                    newest[head->Device] = (nint)at;
                at = (byte*)(((nint)(at + head->Size) + 7) & ~(nint)7); // NEXTRAWINPUTBLOCK: 8-byte aligned on x64
            }
            foreach (var (device, entry) in newest)
            {
                int pressed = pads.Pressed(device, (byte*)entry + header);
                if (pressed != 0)
                    foreach (var button in All)
                        if ((pressed & (int)button) != 0)
                            _ui.Post(b => Pressed?.Invoke((InputBinding)b!), InputBinding.Pad(button));
            }
        }
    }

    public void Dispose()
    {
        _stop.Set();
        _thread.Join(500);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MSG
    {
        public IntPtr Hwnd;
        public uint Message;
        public IntPtr WParam, LParam;
        public uint Time;
        public int X, Y;
        public uint Private;
    }

    [DllImport("user32.dll")]
    static extern unsafe uint GetRawInputBuffer(byte* data, ref uint size, uint headerSize);

    [DllImport("user32.dll")]
    static extern bool PeekMessage(out MSG msg, IntPtr hwnd, uint min, uint max, uint remove);
}

/// <summary>
/// Windows' timer at 1 ms for this process (from Windows 10 2004 on it changes only this process's waits), so the
/// controller threads' short waits are as short as asked: asked once a controller is there, kept until the timer exits.
/// </summary>
public static class TimerResolution
{
    static int _raised;

    public static void Raise()
    {
        if (Interlocked.Exchange(ref _raised, 1) == 0)
            timeBeginPeriod(1);
    }

    [DllImport("winmm.dll")]
    static extern uint timeBeginPeriod(uint milliseconds);
}
