using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OneVOneTimer.Input;

/// <summary>
/// The Xbox button in the same place as a button of another controller: PlayStation (DualSense, DualShock 4) and most
/// USB and Bluetooth pads number their buttons alike, so Cross is A, Circle B, Square X, Triangle Y, L1/R1 LB/RB,
/// L2/R2 LT/RT, Create (Share) Back, Options Start, L3/R3 LS/RS, and the d-pad the d-pad. One set of bindings then
/// works on every controller.
/// </summary>
public static class PadMapping
{
    /// <summary>The Xbox button for HID buttons 1 to 12 (the PlayStation button, the touchpad and the rest are not mapped).</summary>
    static readonly PadButton[] ByNumber =
    [
        PadButton.X, PadButton.A, PadButton.B, PadButton.Y, PadButton.LB, PadButton.RB, PadButton.LT, PadButton.RT,
        PadButton.Back, PadButton.Start, PadButton.LS, PadButton.RS,
    ];

    /// <summary>The d-pad for each of the hat switch's 8 directions, from up clockwise.</summary>
    static readonly PadButton[] Hat =
    [
        PadButton.DPadUp, PadButton.DPadUp | PadButton.DPadRight, PadButton.DPadRight, PadButton.DPadDown | PadButton.DPadRight,
        PadButton.DPadDown, PadButton.DPadDown | PadButton.DPadLeft, PadButton.DPadLeft, PadButton.DPadUp | PadButton.DPadLeft,
    ];

    /// <summary>The buttons held, as Xbox buttons: from the HID button numbers pressed and the hat's direction (-1 = centred).</summary>
    public static int FromHid(ReadOnlySpan<ushort> buttons, int hatDirection)
    {
        int held = 0;
        foreach (ushort n in buttons)
            if (n >= 1 && n <= ByNumber.Length)
                held |= (int)ByNumber[n - 1];
        if (hatDirection is >= 0 and < 8)
            held |= (int)Hat[hatDirection];
        return held;
    }

    /// <summary>
    /// A hat switch's value as one of 8 directions from up clockwise (0-7), or -1 when centred. Its range comes from the
    /// device (0-7 or 1-8 for 8 ways; 4-way hats give up, right, down, left); anything outside it is centred.
    /// </summary>
    public static int HatDirection(int value, int logicalMin, int logicalMax)
    {
        int ways = logicalMax - logicalMin + 1, d = value - logicalMin;
        if (d < 0 || d >= ways)
            return -1;
        return ways switch { 8 => d, 4 => d * 2, _ => -1 };
    }

    /// <summary>Microsoft's vendor id in a USB path ("VID_045E") or a Bluetooth one ("VID&02045e", "VID&0002045e").</summary>
    static readonly Regex Microsoft = new(@"VID[_&](?:[0-9A-F]{2}){0,2}045E(?![0-9A-F])", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// A controller XInput reads already, so its HID reports are left alone: an XInput device ("IG_" in its path), or any
    /// Microsoft one (an Xbox controller over Bluetooth numbers its buttons otherwise, and XInput reads it too).
    /// </summary>
    public static bool IsXInputPath(string devicePath) =>
        devicePath.Contains("IG_", StringComparison.OrdinalIgnoreCase) || Microsoft.IsMatch(devicePath);
}

/// <summary>
/// One controller press seen twice within a moment: a PlayStation controller that DS4Windows also shows as an Xbox one
/// sends each press both ways (HID and XInput). The second is dropped, so it never counts twice.
/// </summary>
public sealed class PadEcho
{
    public const long WindowMs = 100;
    readonly Dictionary<int, long> _last = [];

    public bool IsEcho(int button, long nowMs)
    {
        if (_last.TryGetValue(button, out long first) && nowMs - first < WindowMs)
            return true;
        _last[button] = nowMs;
        return false;
    }
}

/// <summary>
/// Controllers that are not XInput, read from the HID reports Raw Input hands over (<see cref="RawInputListener"/>).
/// Each device's layout is read once from Windows (its buttons and hat switch); then every report gives the buttons
/// held, as Xbox buttons (<see cref="PadMapping"/>). Buttons already held when a controller first reports are not presses.
/// </summary>
public sealed unsafe class HidPads
{
    const uint RIDI_PREPARSEDDATA = 0x20000005, RIDI_DEVICENAME = 0x20000007;
    const int HidP_Input = 0, HIDP_STATUS_SUCCESS = 0x00110000;
    const ushort UsagePageGeneric = 0x01, UsagePageButton = 0x09, UsageHatSwitch = 0x39;
    const int ValueCapsSize = 72, MaxButtons = 64;

    sealed class Device(byte[] preparsed, string name)
    {
        public readonly byte[] Preparsed = preparsed;
        public readonly string Name = name;
        public bool HasHat;
        public ushort HatCollection;
        public int HatMin, HatMax, Previous;
        public bool Seen, PressLogged;
    }

    /// <summary>Each device Raw Input named, or null for one left alone (XInput, or not readable).</summary>
    readonly Dictionary<IntPtr, Device?> _devices = [];

    /// <summary>The Xbox buttons newly pressed in one Raw Input HID input (<paramref name="rawHid"/>: its RAWHID).</summary>
    public int Pressed(IntPtr deviceHandle, byte* rawHid)
    {
        if (!_devices.TryGetValue(deviceHandle, out var device))
            _devices[deviceHandle] = device = Open(deviceHandle);
        if (device == null)
            return 0;
        uint size = *(uint*)rawHid, count = *(uint*)(rawHid + 4);
        int pressed = 0;
        fixed (byte* preparsed = device.Preparsed)
        {
            for (uint i = 0; i < count; i++)
            {
                if (Held(device, (IntPtr)preparsed, rawHid + 8 + i * size, size) is not { } held)
                    continue; // a report without the buttons (another report id)
                if (device.Seen)
                    pressed |= held & ~device.Previous;
                else
                    App.Log.Info($"controller (HID): {device.Name} reads its buttons");
                device.Previous = held;
                device.Seen = true;
            }
        }
        if (pressed != 0 && !device.PressLogged)
        {
            device.PressLogged = true;
            App.Log.Info($"controller (HID): {device.Name} first press {(PadButton)pressed}");
        }
        return pressed;
    }

    static int? Held(Device device, IntPtr preparsed, byte* report, uint length)
    {
        ushort* usages = stackalloc ushort[MaxButtons];
        uint count = MaxButtons;
        if (HidP_GetUsages(HidP_Input, UsagePageButton, 0, usages, ref count, preparsed, report, length) != HIDP_STATUS_SUCCESS)
            return null;
        int hat = -1;
        if (device.HasHat && HidP_GetUsageValue(HidP_Input, UsagePageGeneric, device.HatCollection, UsageHatSwitch, out uint value,
                preparsed, report, length) == HIDP_STATUS_SUCCESS)
            hat = PadMapping.HatDirection((int)value, device.HatMin, device.HatMax);
        return PadMapping.FromHid(new ReadOnlySpan<ushort>(usages, (int)Math.Min(count, MaxButtons)), hat);
    }

    static Device? Open(IntPtr handle)
    {
        string? path = DeviceName(handle);
        if (path == null || PadMapping.IsXInputPath(path))
            return null;
        uint size = 0;
        GetRawInputDeviceInfo(handle, RIDI_PREPARSEDDATA, IntPtr.Zero, ref size);
        if (size == 0)
            return null;
        var id = Regex.Match(path, @"VID_([0-9A-F]{4})&PID_([0-9A-F]{4})", RegexOptions.IgnoreCase);
        var device = new Device(new byte[size], id.Success ? id.Value.ToUpperInvariant() : "a device");
        fixed (byte* preparsed = device.Preparsed)
        {
            if (GetRawInputDeviceInfo(handle, RIDI_PREPARSEDDATA, (IntPtr)preparsed, ref size) == uint.MaxValue)
                return null;
            byte* caps = stackalloc byte[64]; // HIDP_CAPS
            if (HidP_GetCaps((IntPtr)preparsed, caps) != HIDP_STATUS_SUCCESS)
                return null;
            ushort values = *(ushort*)(caps + 48); // NumberInputValueCaps
            if (values > 0)
            {
                var valueCaps = new byte[ValueCapsSize * values];
                fixed (byte* all = valueCaps)
                {
                    ushort length = values;
                    if (HidP_GetValueCaps(HidP_Input, all, ref length, (IntPtr)preparsed) == HIDP_STATUS_SUCCESS)
                        for (int i = 0; i < length; i++)
                        {
                            // HIDP_VALUE_CAPS: UsagePage at 0, LinkCollection at 6, IsRange at 12, LogicalMin/Max at 40/44, Usage(Min/Max) at 56/58.
                            byte* c = all + i * ValueCapsSize;
                            ushort first = *(ushort*)(c + 56), last = c[12] != 0 ? *(ushort*)(c + 58) : first;
                            if (*(ushort*)c != UsagePageGeneric || UsageHatSwitch < first || UsageHatSwitch > last)
                                continue;
                            device.HasHat = true;
                            device.HatCollection = *(ushort*)(c + 6);
                            device.HatMin = *(int*)(c + 40);
                            device.HatMax = *(int*)(c + 44);
                            break;
                        }
                }
            }
        }
        App.Log.Info($"controller (HID): {device.Name}, d-pad {(device.HasHat ? "yes" : "no")}");
        return device;
    }

    static string? DeviceName(IntPtr handle)
    {
        uint chars = 0;
        GetRawInputDeviceInfo(handle, RIDI_DEVICENAME, IntPtr.Zero, ref chars);
        if (chars == 0)
            return null;
        char* name = stackalloc char[(int)chars + 1];
        return GetRawInputDeviceInfo(handle, RIDI_DEVICENAME, (IntPtr)name, ref chars) == uint.MaxValue ? null : new string(name);
    }

    [DllImport("user32.dll", EntryPoint = "GetRawInputDeviceInfoW")]
    static extern uint GetRawInputDeviceInfo(IntPtr device, uint command, IntPtr data, ref uint size);

    [DllImport("hid.dll")]
    static extern int HidP_GetCaps(IntPtr preparsed, byte* caps);

    [DllImport("hid.dll")]
    static extern int HidP_GetValueCaps(int reportType, byte* valueCaps, ref ushort length, IntPtr preparsed);

    [DllImport("hid.dll")]
    static extern int HidP_GetUsages(int reportType, ushort usagePage, ushort linkCollection, ushort* usages, ref uint length,
        IntPtr preparsed, byte* report, uint reportLength);

    [DllImport("hid.dll")]
    static extern int HidP_GetUsageValue(int reportType, ushort usagePage, ushort linkCollection, ushort usage, out uint value,
        IntPtr preparsed, byte* report, uint reportLength);
}
