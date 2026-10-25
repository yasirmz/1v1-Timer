using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OneVOneTimer.Input;

/// <summary>
/// Keys and mouse buttons from every app, read with Raw Input on a message-only window (RIDEV_INPUTSINK): Windows hands
/// the timer a copy of the input and the game still gets every key. Nothing is registered as a hotkey or held back, so
/// no key is ever taken from another app. Presses arrive on the thread that made the listener.
/// </summary>
public sealed class RawInputListener : NativeWindow, IDisposable
{
    const int WM_INPUT = 0x00FF;
    const uint RIDEV_REMOVE = 0x00000001, RIDEV_INPUTSINK = 0x00000100;
    const uint RID_INPUT = 0x10000003;
    const uint RIM_TYPEMOUSE = 0, RIM_TYPEKEYBOARD = 1;
    const ushort RI_KEY_BREAK = 0x1;
    const ushort RI_MOUSE_MIDDLE_DOWN = 0x0010, RI_MOUSE_BUTTON_4_DOWN = 0x0040, RI_MOUSE_BUTTON_5_DOWN = 0x0100;
    static readonly IntPtr HWND_MESSAGE = new(-3);

    readonly HeldKeys _held = new();
    byte[] _buffer = new byte[128];

    public event Action<InputBinding>? Pressed;

    /// <summary>Windows took the registration (false: no keyboard or mouse hotkeys this run).</summary>
    public bool Listening { get; }

    public RawInputListener()
    {
        CreateHandle(new CreateParams { Parent = HWND_MESSAGE });
        RAWINPUTDEVICE[] devices =
        [
            new() { UsagePage = 1, Usage = 6, Flags = RIDEV_INPUTSINK, Target = Handle }, // keyboard
            new() { UsagePage = 1, Usage = 2, Flags = RIDEV_INPUTSINK, Target = Handle }, // mouse
        ];
        Listening = RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
        if (!Listening)
            App.Log.Warn($"raw input: not registered ({Marshal.GetLastWin32Error()})");
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_INPUT)
            Read(m.LParam);
        base.WndProc(ref m); // Windows frees the input's data
    }

    unsafe void Read(IntPtr input)
    {
        uint header = (uint)sizeof(RAWINPUTHEADER), size = 0;
        if (GetRawInputData(input, RID_INPUT, IntPtr.Zero, ref size, header) != 0 || size == 0)
            return;
        if (_buffer.Length < size)
            _buffer = new byte[size];
        fixed (byte* p = _buffer)
        {
            if (GetRawInputData(input, RID_INPUT, (IntPtr)p, ref size, header) == uint.MaxValue)
                return;
            var head = (RAWINPUTHEADER*)p;
            byte* data = p + header;
            if (head->Type == RIM_TYPEKEYBOARD)
            {
                var k = (RAWKEYBOARD*)data;
                if (KeyFrom(k->VKey, k->MakeCode, k->Flags) is not { } key)
                    return;
                if ((k->Flags & RI_KEY_BREAK) != 0)
                    _held.Release(key);
                else if (_held.Press(key, Environment.TickCount64))
                    Pressed?.Invoke(InputBinding.Key((Keys)key));
            }
            else if (head->Type == RIM_TYPEMOUSE)
            {
                ushort buttons = ((RAWMOUSE*)data)->ButtonFlags;
                if ((buttons & RI_MOUSE_MIDDLE_DOWN) != 0)
                    Pressed?.Invoke(InputBinding.Mouse(MouseButton.Middle));
                if ((buttons & RI_MOUSE_BUTTON_4_DOWN) != 0)
                    Pressed?.Invoke(InputBinding.Mouse(MouseButton.Back));
                if ((buttons & RI_MOUSE_BUTTON_5_DOWN) != 0)
                    Pressed?.Invoke(InputBinding.Mouse(MouseButton.Forward));
            }
        }
    }

    /// <summary>
    /// The key a keyboard report is about: Shift, Ctrl and Alt as their left or right key; none for the fake key
    /// (255) Windows puts in some key sequences.
    /// </summary>
    internal static int? KeyFrom(int vkey, int makeCode, int flags)
    {
        const int RI_KEY_E0 = 0x2;
        bool e0 = (flags & RI_KEY_E0) != 0;
        return vkey switch
        {
            0 or 0xFF => null,
            0x10 => makeCode == 0x36 ? (int)Keys.RShiftKey : (int)Keys.LShiftKey,
            0x11 => e0 ? (int)Keys.RControlKey : (int)Keys.LControlKey,
            0x12 => e0 ? (int)Keys.RMenu : (int)Keys.LMenu,
            _ => vkey,
        };
    }

    public void Dispose()
    {
        RAWINPUTDEVICE[] devices =
        [
            new() { UsagePage = 1, Usage = 6, Flags = RIDEV_REMOVE },
            new() { UsagePage = 1, Usage = 2, Flags = RIDEV_REMOVE },
        ];
        RegisterRawInputDevices(devices, (uint)devices.Length, (uint)Marshal.SizeOf<RAWINPUTDEVICE>());
        DestroyHandle();
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTDEVICE
    {
        public ushort UsagePage, Usage;
        public uint Flags;
        public IntPtr Target;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWINPUTHEADER
    {
        public uint Type, Size;
        public IntPtr Device, WParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RAWKEYBOARD
    {
        public ushort MakeCode, Flags, Reserved, VKey;
        public uint Message, ExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RAWMOUSE
    {
        public ushort Flags;
        ushort _alignment;
        public ushort ButtonFlags, ButtonData;
        public uint RawButtons;
        public int LastX, LastY;
        public uint ExtraInformation;
    }

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] devices, uint count, uint size);

    [DllImport("user32.dll")]
    internal static extern uint GetRawInputData(IntPtr input, uint command, IntPtr data, ref uint size, uint headerSize);
}
