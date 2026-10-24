using System.Globalization;
using OneVOneTimer.App;
using OneVOneTimer.Settings;

namespace OneVOneTimer.Tests;

public static class SettingsTests
{
    static string TempFile() => Path.Combine(Path.GetTempPath(), "1v1timer-tests", Guid.NewGuid().ToString("N"), "settings.json");

    static AppSettings LoadJson(string json, string culture = "en-US")
    {
        string path = TempFile();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, json);
        return SettingsStore.Load(path, new CultureInfo(culture));
    }

    [Test]
    public static void Defaults_match_the_spec()
    {
        var s = new AppSettings();
        s.Normalize(new CultureInfo("en-US"));
        Assert.Equal("MouseBack", s.Hotkeys.StartStopReset.Key, "start/stop/reset key");
        Assert.Equal("A", s.Hotkeys.StartStopReset.Pad, "start/stop/reset button");
        Assert.Equal("H", s.Hotkeys.StartStop.Key, "start/stop key");
        Assert.Equal("B", s.Hotkeys.StartStop.Pad, "start/stop button");
        Assert.Equal("T", s.Hotkeys.Timer1.Key, "timer 1 key");
        Assert.Equal("DPadLeft", s.Hotkeys.Timer1.Pad, "timer 1 button");
        Assert.Equal("Y", s.Hotkeys.Timer2.Key, "timer 2 key");
        Assert.Equal("DPadRight", s.Hotkeys.Timer2.Pad, "timer 2 button");
        Assert.Equal("#27B574", s.Colors.Timer, "Timer");
        Assert.Equal("#0099DA", s.Colors.Selected, "Selected Timer");
        Assert.Equal("#FF5400", s.Colors.Last20, "Last 20 Seconds");
        Assert.Equal("#000000", s.Colors.Background, "Background");
        Assert.Equal(false, s.StartTimerOnChange, "start on change");
        Assert.Equal(false, s.TransparentBackground, "transparent");
        Assert.Equal(false, s.ClickThrough, "clickthrough");
        Assert.Equal(false, s.StartWithWindows, "start with Windows: off by default");
        Assert.Equal(true, s.CheckUpdates, "update check");
    }

    [Test]
    public static void A_missing_file_gives_the_defaults_in_windows_language()
    {
        Assert.Equal("ar", SettingsStore.Load(TempFile(), new CultureInfo("ar-SA")).Language, "Arabic Windows");
        Assert.Equal("de", SettingsStore.Load(TempFile(), new CultureInfo("de-AT")).Language, "German, any region");
        Assert.Equal("pt-BR", SettingsStore.Load(TempFile(), new CultureInfo("pt-BR")).Language, "Brazilian Portuguese");
        Assert.Equal("pt-BR", SettingsStore.Load(TempFile(), new CultureInfo("pt-PT")).Language, "other Portuguese: the one there is");
        Assert.Equal("zh-Hans", SettingsStore.Load(TempFile(), new CultureInfo("zh-CN")).Language, "Simplified Chinese");
        Assert.Equal("en", SettingsStore.Load(TempFile(), new CultureInfo("fr-FR")).Language, "a language the timer lacks: English");
        Assert.Equal("H", SettingsStore.Load(TempFile()).Hotkeys.StartStop.Key, "default keys");
    }

    [Test]
    public static void Save_then_load_round_trips()
    {
        string path = TempFile();
        var s = SettingsStore.Load(path, new CultureInfo("en-US"));
        s.Hotkeys.StartStop.Key = "F8";
        s.Hotkeys.Timer2.Pad = "RB";
        s.StartTimerOnChange = true;
        s.TransparentBackground = true;
        s.Colors.Last20 = "#112233";
        s.Overlay.X = -1200;
        s.Overlay.Y = 40;
        s.Overlay.Height = 96;
        s.Language = "ru";
        s.StartWithWindows = true;
        SettingsStore.Save(path, s);

        var loaded = SettingsStore.Load(path);
        Assert.Equal("F8", loaded.Hotkeys.StartStop.Key, "key");
        Assert.Equal("RB", loaded.Hotkeys.Timer2.Pad, "button");
        Assert.Equal(true, loaded.StartTimerOnChange, "option");
        Assert.Equal(true, loaded.TransparentBackground, "transparent");
        Assert.Equal("#112233", loaded.Colors.Last20, "color");
        Assert.Equal(-1200, loaded.Overlay.X, "x on a screen to the left");
        Assert.Equal(40, loaded.Overlay.Y, "y");
        Assert.Equal(96, loaded.Overlay.Height, "size");
        Assert.Equal("ru", loaded.Language, "language");
        Assert.Equal(true, loaded.StartWithWindows, "start with Windows");
    }

    [Test]
    public static void Clickthrough_is_never_saved_and_never_read()
    {
        var s = new AppSettings { ClickThrough = true };
        string json = SettingsStore.Serialize(s);
        Assert.True(!json.Contains("click", StringComparison.OrdinalIgnoreCase), "not in the file");
        Assert.Equal(false, LoadJson("""{ "clickThrough": true }""").ClickThrough, "a file that says so is not believed");
    }

    [Test]
    public static void A_damaged_file_gives_the_defaults_and_is_kept_aside()
    {
        string path = TempFile();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "{ not json");
        var loaded = SettingsStore.Load(path, new CultureInfo("en-US"));
        Assert.Equal("H", loaded.Hotkeys.StartStop.Key, "defaults");
        string backup = Path.Combine(Path.GetDirectoryName(path)!, "settings.bad.json");
        Assert.True(File.Exists(backup), "kept as settings.bad.json");
        Assert.Equal("{ not json", File.ReadAllText(backup), "as it was");
    }

    [Test]
    public static void Wrong_types_and_empty_files_give_the_defaults()
    {
        foreach (string json in new[] { "", "null", "[]", "42", """{ "hotkeys": 5 }""", """{ "overlay": { "height": "big" } }""" })
        {
            var s = LoadJson(json);
            Assert.Equal("MouseBack", s.Hotkeys.StartStopReset.Key, $"'{json}': defaults");
        }
    }

    [Test]
    public static void An_old_or_partial_file_keeps_what_it_has_and_fills_the_rest()
    {
        var s = LoadJson("""{ "startTimerOnChange": true, "hotkeys": { "timer1": { "key": "f1" } }, "somethingNew": [1, 2], }""");
        Assert.Equal(true, s.StartTimerOnChange, "kept");
        Assert.Equal("F1", s.Hotkeys.Timer1.Key, "kept, written the usual way");
        Assert.Equal("", s.Hotkeys.Timer1.Pad, "a pair written without its button has none");
        Assert.Equal("Y", s.Hotkeys.Timer2.Key, "a missing action has its defaults");
        Assert.Equal("#27B574", s.Colors.Timer, "missing colors are the defaults");
        Assert.Equal(AppSettings.CurrentVersion, s.Version, "brought up to date");
    }

    [Test]
    public static void Broken_values_go_back_to_their_defaults()
    {
        var s = LoadJson("""
            {
              "language": "xx",
              "hotkeys": {
                "startStopReset": { "key": "MouseLeft", "pad": "Turbo" },
                "startStop": { "key": "", "pad": "" },
                "timer1": null
              },
              "colors": { "timer": "green", "selected": "0099da", "last20": "#FF54", "background": null },
              "overlay": { "height": 99999 }
            }
            """, "es-MX");
        Assert.Equal("es", s.Language, "an unknown language: Windows'");
        Assert.Equal("MouseBack", s.Hotkeys.StartStopReset.Key, "left click refused: the default");
        Assert.Equal("A", s.Hotkeys.StartStopReset.Pad, "no such button: the default");
        Assert.Equal("", s.Hotkeys.StartStop.Key, "cleared stays cleared");
        Assert.Equal("", s.Hotkeys.StartStop.Pad, "cleared stays cleared");
        Assert.Equal("T", s.Hotkeys.Timer1.Key, "a null pair: the defaults");
        Assert.Equal("#27B574", s.Colors.Timer, "a color name: the default");
        Assert.Equal("#0099DA", s.Colors.Selected, "no hash, small letters: read");
        Assert.Equal("#FF5400", s.Colors.Last20, "too short: the default");
        Assert.Equal("#000000", s.Colors.Background, "null: the default");
        Assert.Equal(Overlay.OverlayLayout.MaxHeight, s.Overlay.Height, "too big: the largest");
    }

    [Test]
    public static void One_input_drives_one_action()
    {
        var s = LoadJson("""{ "hotkeys": { "startStopReset": { "key": "H", "pad": "A" }, "startStop": { "key": "h", "pad": "a" } } }""");
        Assert.Equal("H", s.Hotkeys.StartStopReset.Key, "the first keeps it");
        Assert.Equal("", s.Hotkeys.StartStop.Key, "the second loses it");
        Assert.Equal("", s.Hotkeys.StartStop.Pad, "the same for buttons");
    }

    [Test]
    public static void Reset_to_defaults_keeps_the_language_and_the_timers_place()
    {
        var s = LoadJson("""{ "language": "de", "startTimerOnChange": true, "startWithWindows": true, "colors": { "timer": "#111111" }, "overlay": { "x": 300, "y": 20, "height": 80 }, "hotkeys": { "startStop": { "key": "F9" } } }""");
        s.ResetToDefaults();
        Assert.Equal("H", s.Hotkeys.StartStop.Key, "hotkeys back");
        Assert.Equal(false, s.StartTimerOnChange, "options back");
        Assert.Equal(false, s.StartWithWindows, "start with Windows back off");
        Assert.Equal("#27B574", s.Colors.Timer, "colors back");
        Assert.Equal("de", s.Language, "language kept");
        Assert.Equal(300, s.Overlay.X, "place kept");
        Assert.Equal(80, s.Overlay.Height, "size kept");
    }

    [Test]
    public static void The_install_wizards_choices_apply_once()
    {
        var s = new AppSettings();
        s.Normalize(new CultureInfo("en-US"));
        InstallChoices.Apply("language=de\r\nstartWithWindows=true\r\nlanguage=klingon\r\nnonsense\r\n", s);
        Assert.Equal("de", s.Language, "the wizard's language");
        Assert.Equal(true, s.StartWithWindows, "start with Windows chosen");

        string dir = Path.GetDirectoryName(TempFile())!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, InstallChoices.FileName), "startWithWindows=false");
        Assert.True(InstallChoices.ConsumeInto(s, dir), "read");
        Assert.Equal(false, s.StartWithWindows, "applied");
        Assert.True(!File.Exists(Path.Combine(dir, InstallChoices.FileName)), "then deleted");
        Assert.True(!InstallChoices.ConsumeInto(s, dir), "nothing the next time");
    }
}
