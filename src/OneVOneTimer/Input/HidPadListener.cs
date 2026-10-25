using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OneVOneTimer.Input;

/// <summary>
/// PlayStation (DualSense, DualShock 4) and other controllers that are not XInput, read with Raw Input on a message-only
/// window of their own thread (RIDEV_INPUTSINK: from every app, the game keeps every button). A controller reports up to
/// a thousand times a second, so this never runs on the UI thread; presses are posted there, as Xbox buttons
/// (<see cref="PadMapping"/>).
/// </summary>
public sealed class HidPadListener : IDisposable
{
    const int WM_INPUT = 0x00FF, WM_STOP = 0x8001; // WM_APP + 1
    const uint RIDEV_REMOVE = 0x00000001, RIDEV_INPUTSINK = 0x00000100;
    const uint RID_INPUT = 0x10000003, RIM_TYPEHID = 2;
    /// <summary>Joystick, gamepad and multi-axis controller (HID Generic Desktop usages 4, 5 and 8).</summary>
    static readonly ushort[] Usages = [4, 5, 8];
    static readonly PadButton[] All = Enum.GetValues<PadButton>();

    readonly SynchronizationContext _ui;
    readonly Thread _thread;
    volatile Window? _window;

    public event Action<InputBinding>? Pressed;

    /// <param name="ui">Where presses are posted (the UI thread).</param>
    public HidPadListener(SynchronizationContext ui)
    {
        _ui = ui;
        _thread = new Thread(Run) { IsBackground = true, Name = "1v1 Timer HID controllers" };
        _thread.Start();
    }

    void Run()
    {
        var window = new Window(this);
        var devices = Usages.Select(u => new RawInputListener.RAWINPUTDEVICE { UsagePage = 1, Usage = u, Flags = RIDEV_INPUTSINK, Target = window.Handle }).ToArray();
        if (!RawInputListener.RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf<RawInputListener.RAWINPUTDEVICE>()))
        {
            App.Log.Warn($"raw input (controllers): not registered ({Marshal.GetLastWin32Error()})");
            window.DestroyHandle();
            return;
        }
        _window = window;
        while (GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
            DispatchMessage(ref msg);
        var remove = Usages.Select(u => new RawInputListener.RAWINPUTDEVICE { UsagePage = 1, Usage = u, Flags = RIDEV_REMOVE }).ToArray();
        RawInputListener.RegisterRawInputDevices(remove, (uint)remove.Length, (uint)Marshal.SizeOf<RawInputListener.RAWINPUTDEVICE>());
        window.DestroyHandle();
    }

    void Post(int pressed)
    {
        foreach (var button in All)
            if ((pressed & (int)button) != 0)
                _ui.Post(b => Pressed?.Invoke((InputBinding)b!), InputBinding.Pad(button));
    }

    public void Dispose()
    {
        if (_window is { } window)
            PostMessage(window.Handle, WM_STOP, IntPtr.Zero, IntPtr.Zero);
        _thread.Join(500);
    }

    sealed unsafe class Window : NativeWindow
    {
        static readonly IntPtr HWND_MESSAGE = new(-3);
        readonly HidPadListener _owner;
        readonly HidPads _pads = new();
        byte[] _buffer = new byte[256];

        public Window(HidPadListener owner)
        {
            _owner = owner;
            CreateHandle(new CreateParams { Parent = HWND_MESSAGE });
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_INPUT)
                Read(m.LParam);
            else if (m.Msg == WM_STOP)
                PostQuitMessage(0);
            base.WndProc(ref m);
        }

        void Read(IntPtr input)
        {
            uint header = (uint)sizeof(RawInputListener.RAWINPUTHEADER), size = 0;
            if (RawInputListener.GetRawInputData(input, RID_INPUT, IntPtr.Zero, ref size, header) != 0 || size == 0)
                return;
            if (_buffer.Length < size)
                _buffer = new byte[size];
            fixed (byte* p = _buffer)
            {
                if (RawInputListener.GetRawInputData(input, RID_INPUT, (IntPtr)p, ref size, header) == uint.MaxValue)
                    return;
                var head = (RawInputListener.RAWINPUTHEADER*)p;
                if (head->Type != RIM_TYPEHID)
                    return;
                int pressed = _pads.Pressed(head->Device, p + header);
                if (pressed != 0)
                    _owner.Post(pressed);
            }
        }
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
    static extern int GetMessage(out MSG msg, IntPtr hwnd, uint min, uint max);

    [DllImport("user32.dll")]
    static extern IntPtr DispatchMessage(ref MSG msg);

    [DllImport("user32.dll")]
    static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    static extern void PostQuitMessage(int code);
}
