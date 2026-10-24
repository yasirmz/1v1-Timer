using System.Windows.Forms;
using OneVOneTimer.Input;

namespace OneVOneTimer.Tests;

public static class BindingTests
{
    static string? Key(string text) => BindingParser.TryParse(text, pad: false, out var b) ? BindingParser.Format(b) : null;

    static string? Pad(string text) => BindingParser.TryParse(text, pad: true, out var b) ? BindingParser.Format(b) : null;

    [Test]
    public static void Keys_read_in_any_case_and_write_one_way()
    {
        Assert.Equal("H", Key("h"), "a letter");
        Assert.Equal("F8", Key("f8"), "a function key");
        Assert.Equal("1", Key("1"), "a digit, without Windows' D prefix");
        Assert.Equal("1", Key("D1"), "Windows' own name for it");
        Assert.Equal("NumPad5", Key("numpad5"), "the number pad");
        Assert.Equal("Space", Key(" Space "), "spaces around");
        Assert.Equal("LShiftKey", Key("LShiftKey"), "a left-hand modifier on its own");
    }

    [Test]
    public static void Every_bindable_key_round_trips()
    {
        foreach (Keys key in Enum.GetValues<Keys>().Where(BindingParser.IsBindable))
        {
            string written = BindingParser.Format(InputBinding.Key(key));
            Assert.True(BindingParser.TryParse(written, pad: false, out var back), $"{key} written as '{written}' reads back");
            Assert.Equal((int)key, back.Code, $"{written} is the same key");
        }
    }

    [Test]
    public static void Mouse_buttons_read_by_name_and_as_windows_calls_them()
    {
        Assert.Equal("MouseBack", Key("MouseBack"), "back");
        Assert.Equal("MouseBack", Key("xbutton1"), "XButton1 is back");
        Assert.Equal("MouseForward", Key("XButton2"), "XButton2 is forward");
        Assert.Equal("MouseMiddle", Key("mousemiddle"), "middle");
        Assert.True(BindingParser.TryParse("MouseBack", false, out var b) && b.Kind == InputKind.Mouse, "a mouse binding, not a key");
    }

    [Test]
    public static void Left_and_right_click_and_the_box_keys_are_refused()
    {
        foreach (string s in new[] { "MouseLeft", "MouseRight", "LButton", "RButton", "Escape", "Delete", "ShiftKey", "ControlKey", "Menu" })
            Assert.True(Key(s) == null, $"'{s}' is not a hotkey");
    }

    [Test]
    public static void Junk_is_refused()
    {
        foreach (string s in new[] { "", "   ", "Foo", "Ctrl+H", "H+J", "72", "-5", "Shift, H", "MouseSide" })
            Assert.True(Key(s) == null, $"'{s}' should not read");
        foreach (string s in new[] { "", "Foo", "A+B", "1", "MouseBack", "LeftTrigger", "A, B" })
            Assert.True(Pad(s) == null, $"pad '{s}' should not read");
    }

    [Test]
    public static void Controller_buttons_read_in_any_case()
    {
        Assert.Equal("A", Pad("a"), "A");
        Assert.Equal("DPadLeft", Pad("dpadleft"), "the d-pad");
        Assert.Equal("RT", Pad("rt"), "a trigger");
        Assert.Equal("Start", Pad("Start"), "start");
        foreach (var button in Enum.GetValues<PadButton>())
            Assert.Equal(button.ToString(), Pad(button.ToString()), $"{button} round trips");
    }

    [Test]
    public static void One_name_means_a_key_in_one_column_and_a_button_in_the_other()
    {
        Assert.True(BindingParser.TryParse("A", pad: false, out var key) && key == InputBinding.Key(Keys.A), "the A key");
        Assert.True(BindingParser.TryParse("A", pad: true, out var pad) && pad == InputBinding.Pad(PadButton.A), "the A button");
        Assert.True(BindingParser.TryParse("Back", pad: false, out var back) && back == InputBinding.Key(Keys.Back), "Backspace");
        Assert.True(BindingParser.TryParse("Back", pad: true, out var view) && view == InputBinding.Pad(PadButton.Back), "the Back button");
        Assert.True(Key("DPadLeft") == null, "no such key");
    }

    [Test]
    public static void Raw_keys_tell_left_and_right_modifiers_apart()
    {
        Assert.Equal((int?)Keys.LShiftKey, RawInputListener.KeyFrom(0x10, 0x2A, 0), "left shift");
        Assert.Equal((int?)Keys.RShiftKey, RawInputListener.KeyFrom(0x10, 0x36, 0), "right shift");
        Assert.Equal((int?)Keys.LControlKey, RawInputListener.KeyFrom(0x11, 0x1D, 0), "left ctrl");
        Assert.Equal((int?)Keys.RControlKey, RawInputListener.KeyFrom(0x11, 0x1D, 2), "right ctrl (E0)");
        Assert.Equal((int?)Keys.RMenu, RawInputListener.KeyFrom(0x12, 0x38, 2), "right alt (E0)");
        Assert.Equal((int?)Keys.H, RawInputListener.KeyFrom((int)Keys.H, 0x23, 0), "a letter as it is");
        Assert.Equal((int?)null, RawInputListener.KeyFrom(0xFF, 0x2A, 0), "Windows' fake key");
    }

    [Test]
    public static void A_held_key_counts_once()
    {
        var held = new HeldKeys();
        Assert.True(held.Press(72, 1000), "first press");
        Assert.True(!held.Press(72, 1500), "repeat after Windows' delay");
        Assert.True(!held.Press(72, 1533), "repeat");
        Assert.True(!held.Press(72, 2400), "still held, repeating");
        held.Release(72);
        Assert.True(held.Press(72, 2450), "pressed again after letting go");
        Assert.True(held.Press(84, 2450), "another key is its own");
    }

    [Test]
    public static void A_key_whose_release_was_missed_counts_again_once_quiet()
    {
        var held = new HeldKeys();
        Assert.True(held.Press(72, 0), "first press");
        // The release never came (a secure screen took it): no repeats either, then a new press.
        Assert.True(held.Press(72, HeldKeys.QuietMs + 50), "a new press after a quiet spell");
    }

    [Test]
    public static void Triggers_count_as_buttons_past_a_threshold_without_flicker()
    {
        int lt = (int)PadButton.LT, rt = (int)PadButton.RT;
        Assert.Equal(0, PadPoller.Buttons(0, 40, 0, 0) & lt, "a light touch is not a press");
        Assert.Equal(lt, PadPoller.Buttons(0, 200, 0, 0) & lt, "pulled: pressed");
        Assert.Equal(lt, PadPoller.Buttons(0, 40, 0, lt) & lt, "eased off half way: still held");
        Assert.Equal(0, PadPoller.Buttons(0, 10, 0, lt) & lt, "let go: released");
        Assert.Equal(rt, PadPoller.Buttons(0, 0, 255, 0) & rt, "the right trigger");
        Assert.Equal((int)(PadButton.A | PadButton.DPadLeft), PadPoller.Buttons((ushort)(PadButton.A | PadButton.DPadLeft), 0, 0, 0), "buttons as XInput gives them");
        Assert.Equal(0, PadPoller.Buttons(0x0C00, 0, 0, 0), "XInput's unused bits are no buttons");
    }
}
