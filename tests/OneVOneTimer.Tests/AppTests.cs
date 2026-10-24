using OneVOneTimer.App;
using OneVOneTimer.Updates;

namespace OneVOneTimer.Tests;

/// <summary>Updates, the install folder and start-up: what decides, without touching Windows.</summary>
public static class AppTests
{
    [Test]
    public static void Updates_are_looked_for_a_minute_after_the_start_then_daily()
    {
        var start = new DateTime(2026, 10, 25, 12, 0, 0, DateTimeKind.Utc);
        Assert.True(!UpdateRules.ShouldCheck(true, start, null, start.AddSeconds(30)), "not in the first minute");
        Assert.True(UpdateRules.ShouldCheck(true, start, null, start.AddSeconds(61)), "after a minute");
        Assert.True(!UpdateRules.ShouldCheck(true, start, start.AddMinutes(1), start.AddHours(5)), "not again the same day");
        Assert.True(UpdateRules.ShouldCheck(true, start, start.AddMinutes(1), start.AddHours(25)), "the next day");
        Assert.True(!UpdateRules.ShouldCheck(false, start, null, start.AddHours(25)), "never when turned off");
    }

    [Test]
    public static void Only_a_local_fixed_drive_can_be_a_test_update_folder()
    {
        Assert.True(!UpdateRules.IsLocalFolder(@"\\server\share\releases"), "a network share");
        Assert.True(!UpdateRules.IsLocalFolder(@"releases"), "a relative path");
        Assert.True(UpdateRules.IsLocalFolder(Path.GetTempPath()), "the temp folder");
    }

    [Test]
    public static void The_install_folder_is_found_from_the_program_inside_it()
    {
        string root = Path.Combine(Path.GetTempPath(), "1v1timer-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "current"));
        File.WriteAllText(Path.Combine(root, "Update.exe"), "");
        Assert.Equal(root, InstallPaths.RootFor(Path.Combine(root, "current", "1v1Timer.exe")), "installed");
        Assert.Equal((string?)null, InstallPaths.RootFor(Path.Combine(root, "1v1Timer.exe")), "the stub itself is not inside current");
        Assert.Equal((string?)null, InstallPaths.RootFor(@"D:\dev\bin\1v1Timer.exe"), "a development build");
        Assert.Equal(Path.Combine(root, "1v1 Timer.exe"), InstallPaths.StubFor(root), "the stub Windows starts: Velopack names it after the title");
    }

    [Test]
    public static void Start_with_windows_points_at_this_install_only()
    {
        string root = @"C:\Users\me\AppData\Local\1v1Timer";
        Assert.Equal("\"C:\\x\\1v1Timer.exe\" --background", Autostart.CommandFor(@"C:\x\1v1Timer.exe"), "the command");
        Assert.True(Autostart.PointsInto(Autostart.CommandFor(InstallPaths.StubFor(root)), root), "this install's (the stub's name has a space)");
        Assert.Equal($"\"{root}\\1v1 Timer.exe\" --background", Autostart.CommandFor(InstallPaths.StubFor(root)), "quoted, so the space is safe");
        Assert.True(!Autostart.PointsInto(@"""D:\other\1v1Timer.exe"" --background", root), "another copy's");
        Assert.True(!Autostart.PointsInto(null, root), "none");
    }

    [Test]
    public static void Only_this_installs_timers_are_closed_on_uninstall()
    {
        var running = new (int, string?)[] { (1, @"C:\Inst\current\1v1Timer.exe"), (2, @"D:\dev\1v1Timer.exe"), (3, null), (4, @"C:\Inst\1v1Timer.exe") };
        Assert.Equal("1,4", string.Join(",", InstallPaths.OwnProcesses(@"C:\Inst", running, self: 99)), "inside the folder");
        Assert.Equal("4", string.Join(",", InstallPaths.OwnProcesses(@"C:\Inst", running, self: 1)), "never itself");
    }

    [Test]
    public static void The_tray_menu_is_rebuilt_only_when_its_items_change()
    {
        Assert.Equal(TrayController.MenuKey(false, null, "en"), TrayController.MenuKey(false, null, "en"), "same items");
        Assert.True(TrayController.MenuKey(true, null, "en") != TrayController.MenuKey(false, null, "en"), "clickthrough's tick");
        Assert.True(TrayController.MenuKey(false, "1.1.0", "en") != TrayController.MenuKey(false, null, "en"), "an update offered");
        Assert.True(TrayController.MenuKey(false, null, "ar") != TrayController.MenuKey(false, null, "en"), "another language");
    }

    [Test]
    public static void Versions_read_as_people_say_them()
    {
        Assert.Equal("1.0.0", OneVOneTimer.AppInfo.Display("1.0.0"), "a release");
        Assert.Equal("1.1.0 beta 2", OneVOneTimer.AppInfo.Display("1.1.0-beta.2"), "a beta");
        Assert.True(!OneVOneTimer.AppInfo.Version.Contains('+'), "no build details");
    }
}
