using OneVOneTimer.Input;

namespace OneVOneTimer.Tests;

/// <summary>PlayStation and other HID controllers: their buttons as Xbox buttons, their d-pad, and what XInput reads instead.</summary>
public static class HidPadTests
{
    static PadButton Buttons(params ushort[] numbers) => (PadButton)PadMapping.FromHid(numbers, -1);

    [Test]
    public static void PlayStation_buttons_are_the_xbox_buttons_in_the_same_place()
    {
        Assert.Equal(PadButton.X, Buttons(1), "Square");
        Assert.Equal(PadButton.A, Buttons(2), "Cross");
        Assert.Equal(PadButton.B, Buttons(3), "Circle");
        Assert.Equal(PadButton.Y, Buttons(4), "Triangle");
        Assert.Equal(PadButton.LB | PadButton.RB, Buttons(5, 6), "L1 and R1");
        Assert.Equal(PadButton.LT | PadButton.RT, Buttons(7, 8), "L2 and R2");
        Assert.Equal(PadButton.Back, Buttons(9), "Create / Share");
        Assert.Equal(PadButton.Start, Buttons(10), "Options");
        Assert.Equal(PadButton.LS | PadButton.RS, Buttons(11, 12), "L3 and R3");
    }

    [Test]
    public static void Buttons_with_no_xbox_twin_are_left_out()
    {
        Assert.Equal((PadButton)0, Buttons(13), "PS button");
        Assert.Equal((PadButton)0, Buttons(14, 15), "touchpad and mute");
        Assert.Equal((PadButton)0, Buttons(0, 40), "out of range");
        Assert.Equal((PadButton)0, Buttons(), "none held");
    }

    [Test]
    public static void The_hat_switch_is_the_d_pad()
    {
        PadButton Hat(int value, int min = 0, int max = 7) => (PadButton)PadMapping.FromHid([], PadMapping.HatDirection(value, min, max));
        Assert.Equal(PadButton.DPadUp, Hat(0), "up");
        Assert.Equal(PadButton.DPadRight, Hat(2), "right");
        Assert.Equal(PadButton.DPadDown, Hat(4), "down");
        Assert.Equal(PadButton.DPadLeft, Hat(6), "left");
        Assert.Equal(PadButton.DPadUp | PadButton.DPadLeft, Hat(7), "up-left holds both");
        Assert.Equal((PadButton)0, Hat(8), "centred (the DualSense's null value)");
        Assert.Equal(PadButton.DPadLeft, Hat(7, 1, 8), "a 1-8 hat");
        Assert.Equal(PadButton.DPadLeft, Hat(3, 0, 3), "a 4-way hat");
        Assert.Equal((PadButton)0, Hat(-1), "below the range");
    }

    [Test]
    public static void Controllers_xinput_reads_are_not_read_twice()
    {
        Assert.True(PadMapping.IsXInputPath(@"\\?\HID#VID_045E&PID_028E&IG_00#7&1a2b3c4d&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"), "an Xbox 360 pad");
        Assert.True(PadMapping.IsXInputPath(@"\\?\HID#VID_2DC8&PID_3106&IG_00#8&abc&0&0000#{4d1e55b2}"), "a third-party XInput pad");
        Assert.True(PadMapping.IsXInputPath(@"\\?\HID#VID_045E&PID_0B13&Col01#a&b#{4d1e55b2}"), "a Microsoft pad by USB vendor id");
        Assert.True(PadMapping.IsXInputPath(@"\\?\HID#{00001812-0000-1000-8000-00805f9b34fb}_Dev_VID&02045e_PID&0b13_REV&0509#9&x#{4d1e55b2}"), "an Xbox pad over Bluetooth");
        Assert.True(PadMapping.IsXInputPath(@"\\?\HID#{00001124-0000-1000-8000-00805f9b34fb}_VID&0002045e_PID&02e0#9&y#{4d1e55b2}"), "an older Xbox pad over Bluetooth");
        Assert.True(!PadMapping.IsXInputPath(@"\\?\HID#{00001124-0000-1000-8000-00805f9b34fb}_VID&0002054c_PID&0ce6#9&z#{4d1e55b2}"), "a DualSense over Bluetooth");
        Assert.True(!PadMapping.IsXInputPath(@"\\?\HID#VID_054C&PID_0CE6&MI_03#8&2b3c&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}"), "a DualSense");
        Assert.True(!PadMapping.IsXInputPath(@"\\?\HID#VID_054C&PID_09CC&MI_03#8&1&0&0000#{4d1e55b2}"), "a DualShock 4");
    }

    [Test]
    public static void One_press_seen_twice_counts_once()
    {
        var echo = new PadEcho();
        Assert.True(!echo.IsEcho((int)PadButton.A, 1000), "the first");
        Assert.True(echo.IsEcho((int)PadButton.A, 1000 + 30), "the same press from the other driver");
        Assert.True(!echo.IsEcho((int)PadButton.B, 1000 + 30), "another button");
        Assert.True(!echo.IsEcho((int)PadButton.A, 1000 + PadEcho.WindowMs), "a new press");
    }
}
