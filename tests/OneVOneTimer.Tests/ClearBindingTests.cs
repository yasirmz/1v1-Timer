using OneVOneTimer.Settings;
using OneVOneTimer.Ui;
using OneVOneTimer.Updates;

namespace OneVOneTimer.Tests;

/// <summary>The × on a hotkey box: any key or button can be left free (a controller has few buttons, and the game needs them).</summary>
public static class ClearBindingTests
{
    static (SettingsViewModel Vm, AppSettings Settings, Func<int> Changes) Window()
    {
        var settings = new AppSettings();
        int changes = 0;
        var updates = UpdateService.ForThisInstall(Path.GetTempPath(), new SynchronizationContext());
        var vm = new SettingsViewModel(settings, updates, () => changes++, () => { });
        return (vm, settings, () => changes);
    }

    [Test]
    public static void The_x_clears_a_controller_button_and_leaves_the_key()
    {
        var (vm, settings, changes) = Window();
        var row = vm.Rows[0]; // Start / Stop / Reset: Mouse Back and A
        Assert.True(row.HasPad && row.HasKey, "both bound to start with");
        row.ClearPadCommand.Execute(null);
        Assert.Equal("", settings.Hotkeys.StartStopReset.Pad, "the controller button is none");
        Assert.Equal("MouseBack", settings.Hotkeys.StartStopReset.Key, "the key stays");
        Assert.True(!row.HasPad, "its × hides");
        Assert.Equal(1, changes(), "applied and saved at once");
        vm.Detach();
    }

    [Test]
    public static void The_x_clears_a_key_and_leaves_the_controller_button()
    {
        var (vm, settings, _) = Window();
        var row = vm.Rows[3]; // Select Timer 2: Y and D-pad Right
        row.ClearKeyCommand.Execute(null);
        Assert.Equal("", settings.Hotkeys.Timer2.Key, "the key is none");
        Assert.Equal("DPadRight", settings.Hotkeys.Timer2.Pad, "the controller button stays");
        vm.Detach();
    }

    [Test]
    public static void Every_binding_can_be_cleared_and_stays_cleared_after_a_restart()
    {
        var (vm, settings, _) = Window();
        foreach (var row in vm.Rows)
        {
            row.ClearKeyCommand.Execute(null);
            row.ClearPadCommand.Execute(null);
        }
        var reloaded = SettingsStore.Deserialize(SettingsStore.Serialize(settings));
        reloaded.Normalize();
        foreach (var action in Enum.GetValues<TimerAction>())
        {
            Assert.Equal("", reloaded.Hotkeys.For(action).Key, $"{action} key");
            Assert.Equal("", reloaded.Hotkeys.For(action).Pad, $"{action} button");
        }
        vm.Detach();
    }
}
