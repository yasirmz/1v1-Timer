using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OneVOneTimer.Input;

public enum InputKind { Key, Mouse, Pad }

/// <summary>The mouse buttons a hotkey can be (never the left or right click).</summary>
public enum MouseButton { Middle = 3, Back = 4, Forward = 5 }

/// <summary>Xbox controller buttons as XInput reports them, plus the two triggers (pulled past a threshold).</summary>
[Flags]
public enum PadButton
{
    DPadUp = 0x0001,
    DPadDown = 0x0002,
    DPadLeft = 0x0004,
    DPadRight = 0x0008,
    Start = 0x0010,
    Back = 0x0020,
    LS = 0x0040,
    RS = 0x0080,
    LB = 0x0100,
    RB = 0x0200,
    A = 0x1000,
    B = 0x2000,
    X = 0x4000,
    Y = 0x8000,
    LT = 0x10000,
    RT = 0x20000,
}

/// <summary>One key (a Windows virtual-key code), mouse button or controller button.</summary>
public readonly record struct InputBinding(InputKind Kind, int Code)
{
    public static InputBinding Key(Keys key) => new(InputKind.Key, (int)key);
    public static InputBinding Mouse(MouseButton button) => new(InputKind.Mouse, (int)button);
    public static InputBinding Pad(PadButton button) => new(InputKind.Pad, (int)button);
}

/// <summary>
/// Bindings as the settings file writes them: a key by its name ("H", "F8", "1", "NumPad5"), a mouse button
/// ("MouseMiddle", "MouseBack", "MouseForward") and a controller button ("A", "LB", "LT", "DPadLeft"). Each action has a
/// keyboard/mouse binding and a controller binding, read separately ("A" is the A key in one and the A button in the other).
/// </summary>
public static class BindingParser
{
    static readonly Dictionary<string, MouseButton> MouseNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MouseMiddle"] = MouseButton.Middle, ["MButton"] = MouseButton.Middle,
        ["MouseBack"] = MouseButton.Back, ["XButton1"] = MouseButton.Back,
        ["MouseForward"] = MouseButton.Forward, ["XButton2"] = MouseButton.Forward,
    };

    /// <summary>
    /// Keys a box cannot take: Esc cancels and Delete clears; the generic Shift, Ctrl and Alt are always read as their
    /// left or right key; the mouse's own codes are mouse buttons; KeyCode and Modifiers are masks, not keys.
    /// </summary>
    static readonly HashSet<Keys> Unbindable =
    [
        Keys.None, Keys.Escape, Keys.Delete, Keys.ShiftKey, Keys.ControlKey, Keys.Menu, Keys.Shift, Keys.Control, Keys.Alt,
        Keys.LButton, Keys.RButton, Keys.MButton, Keys.XButton1, Keys.XButton2, Keys.Cancel, Keys.KeyCode, Keys.Modifiers,
    ];

    public static bool IsBindable(Keys key) => !Unbindable.Contains(key) && Enum.IsDefined(key);

    /// <summary>A keyboard/mouse binding (<paramref name="pad"/> false) or a controller binding from its name.</summary>
    public static bool TryParse(string? text, bool pad, out InputBinding binding)
    {
        binding = default;
        string s = (text ?? "").Trim();
        if (s.Length == 0 || s.Contains('+'))
            return false;
        if (pad)
        {
            if (char.IsAsciiDigit(s[0]) || !Enum.TryParse(s, ignoreCase: true, out PadButton button) || !Enum.IsDefined(button))
                return false;
            binding = InputBinding.Pad(button);
            return true;
        }
        if (MouseNames.TryGetValue(s, out var mouse))
        {
            binding = InputBinding.Mouse(mouse);
            return true;
        }
        if (s.Length == 1 && char.IsAsciiDigit(s[0]))
        {
            binding = InputBinding.Key(Keys.D0 + (s[0] - '0'));
            return true;
        }
        if (char.IsAsciiDigit(s[0]) || !Enum.TryParse(s, ignoreCase: true, out Keys key) || !IsBindable(key))
            return false;
        binding = InputBinding.Key(key);
        return true;
    }

    /// <summary>The name the settings file keeps (reads back with <see cref="TryParse"/>).</summary>
    public static string Format(InputBinding b) => b.Kind switch
    {
        InputKind.Mouse => "Mouse" + (MouseButton)b.Code,
        InputKind.Pad => ((PadButton)b.Code).ToString(),
        _ => (Keys)b.Code is >= Keys.D0 and <= Keys.D9 ? ((char)('0' + (b.Code - (int)Keys.D0))).ToString() : ((Keys)b.Code).ToString(),
    };

    /// <summary>The binding as a hotkey box shows it, in the user's language where it has words ("Mouse Back", "D-pad Left").</summary>
    public static string Display(InputBinding b) => b.Kind switch
    {
        InputKind.Mouse => App.Text.Get("key.mouse" + (MouseButton)b.Code),
        InputKind.Pad => (PadButton)b.Code switch
        {
            PadButton.DPadUp => App.Text.Get("pad.dpadUp"),
            PadButton.DPadDown => App.Text.Get("pad.dpadDown"),
            PadButton.DPadLeft => App.Text.Get("pad.dpadLeft"),
            PadButton.DPadRight => App.Text.Get("pad.dpadRight"),
            PadButton.Back => App.Text.Get("pad.back"),
            PadButton.Start => App.Text.Get("pad.start"),
            var p => p.ToString(),
        },
        _ => KeyName((Keys)b.Code),
    };

    static string KeyName(Keys key) => key switch
    {
        >= Keys.D0 and <= Keys.D9 => ((char)('0' + (key - Keys.D0))).ToString(),
        >= Keys.NumPad0 and <= Keys.NumPad9 => "Num " + (key - Keys.NumPad0),
        Keys.Return => "Enter",
        Keys.Back => "Backspace",
        Keys.Capital => "Caps Lock",
        Keys.Prior => "Page Up",
        Keys.Next => "Page Down",
        Keys.Snapshot => "Print Screen",
        Keys.Scroll => "Scroll Lock",
        Keys.LShiftKey => "Left Shift",
        Keys.RShiftKey => "Right Shift",
        Keys.LControlKey => "Left Ctrl",
        Keys.RControlKey => "Right Ctrl",
        Keys.LMenu => "Left Alt",
        Keys.RMenu => "Right Alt",
        Keys.LWin => "Left Win",
        Keys.RWin => "Right Win",
        Keys.Multiply => "Num *",
        Keys.Add => "Num +",
        Keys.Subtract => "Num -",
        Keys.Divide => "Num /",
        Keys.Decimal => "Num .",
        // The punctuation keys show what they type on this keyboard ("-", "[", "ö").
        _ when key.ToString().StartsWith("Oem", StringComparison.Ordinal) && Character(key) is { } c => c,
        _ => key.ToString(),
    };

    static string? Character(Keys key)
    {
        uint c = MapVirtualKey((uint)key, 2 /* MAPVK_VK_TO_CHAR */) & 0x7FFFFFFF;
        return c is > 32 and < 0xFFFF ? char.ToUpperInvariant((char)c).ToString() : null;
    }

    [DllImport("user32.dll")]
    static extern uint MapVirtualKey(uint code, uint mapType);
}
