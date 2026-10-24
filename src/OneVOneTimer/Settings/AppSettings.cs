using System.Globalization;
using System.Text.Json.Serialization;
using OneVOneTimer.Input;
using OneVOneTimer.Overlay;

namespace OneVOneTimer.Settings;

/// <summary>What a hotkey does, in the order the settings window lists them.</summary>
public enum TimerAction { StartStopReset, StartStop, SelectTimer1, SelectTimer2 }

/// <summary>One action's two bindings: a key or mouse button, and a controller button ("" = none).</summary>
public sealed class HotkeyPair
{
    public string Key { get; set; } = "";
    public string Pad { get; set; } = "";
}

public sealed class HotkeySettings
{
    public HotkeyPair StartStopReset { get; set; } = Default(TimerAction.StartStopReset);
    public HotkeyPair StartStop { get; set; } = Default(TimerAction.StartStop);
    public HotkeyPair Timer1 { get; set; } = Default(TimerAction.SelectTimer1);
    public HotkeyPair Timer2 { get; set; } = Default(TimerAction.SelectTimer2);

    public static HotkeyPair Default(TimerAction action) => action switch
    {
        TimerAction.StartStopReset => new() { Key = "MouseBack", Pad = "A" },
        TimerAction.StartStop => new() { Key = "H", Pad = "B" },
        TimerAction.SelectTimer1 => new() { Key = "T", Pad = "DPadLeft" },
        _ => new() { Key = "Y", Pad = "DPadRight" },
    };

    public HotkeyPair For(TimerAction action) => action switch
    {
        TimerAction.StartStopReset => StartStopReset,
        TimerAction.StartStop => StartStop,
        TimerAction.SelectTimer1 => Timer1,
        _ => Timer2,
    };

    internal void Set(TimerAction action, HotkeyPair pair)
    {
        switch (action)
        {
            case TimerAction.StartStopReset: StartStopReset = pair; break;
            case TimerAction.StartStop: StartStop = pair; break;
            case TimerAction.SelectTimer1: Timer1 = pair; break;
            default: Timer2 = pair; break;
        }
    }
}

public sealed class ColorSettings
{
    public const string DefaultTimer = "#27B574", DefaultSelected = "#0099DA", DefaultLast20 = "#FF5400", DefaultBackground = "#000000";

    public string Timer { get; set; } = DefaultTimer;
    public string Selected { get; set; } = DefaultSelected;
    public string Last20 { get; set; } = DefaultLast20;
    public string Background { get; set; } = DefaultBackground;
}

/// <summary>Where the timer window is and how big, in screen pixels: its top-left corner and its height (the width follows).</summary>
public sealed class OverlayPlace
{
    /// <summary>Null until the timer is first placed: then the top-left corner of the main screen.</summary>
    public int? X { get; set; }
    public int? Y { get; set; }
    /// <summary>0 until the timer is first resized: then a height that suits the screen's scale.</summary>
    public int Height { get; set; }
}

public sealed class AppSettings
{
    /// <summary>The file's layout; an older file is brought up to date when read.</summary>
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;
    public string Language { get; set; } = "";
    public HotkeySettings Hotkeys { get; set; } = new();
    public bool StartTimerOnChange { get; set; }
    public bool TransparentBackground { get; set; }

    /// <summary>Clicks go through the timer. Never saved: always off when the timer starts.</summary>
    [JsonIgnore]
    public bool ClickThrough { get; set; }

    public ColorSettings Colors { get; set; } = new();
    public OverlayPlace Overlay { get; set; } = new();
    public bool StartWithWindows { get; set; }
    public bool CheckUpdates { get; set; } = true;

    /// <summary>Windows' display language when the timer has it (its region's variant too), else English.</summary>
    public static string DefaultLanguage(CultureInfo uiCulture)
    {
        for (var culture = uiCulture; culture.Name.Length > 0; culture = culture.Parent)
            if (App.Text.IsSupported(culture.Name))
                return culture.Name;
        var regional = App.Text.Languages.FirstOrDefault(l =>
            l.Code.Split('-') is [var language, { Length: 2 }] && language == uiCulture.TwoLetterISOLanguageName);
        return regional?.Code ?? App.Text.Fallback;
    }

    /// <summary>
    /// "Reset to defaults": hotkeys, options and colors as they started. The language, the timer's place and size, and the
    /// update check stay as the user has them.
    /// </summary>
    public void ResetToDefaults()
    {
        Hotkeys = new();
        StartTimerOnChange = false;
        TransparentBackground = false;
        ClickThrough = false;
        Colors = new();
        StartWithWindows = false;
    }

    /// <summary>
    /// Puts anything missing, broken or out of range back to its default, so a damaged or old file always gives working
    /// settings: a binding that does not read is the action's default ("" stays none), a color that is no #RRGGBB is
    /// the default color, and an input bound to two actions keeps the first.
    /// </summary>
    public void Normalize(CultureInfo? uiCulture = null)
    {
        Version = CurrentVersion;
        if (!App.Text.IsSupported(Language))
            Language = DefaultLanguage(uiCulture ?? CultureInfo.CurrentUICulture);

        Hotkeys ??= new();
        var usedKeys = new HashSet<string>();
        var usedPads = new HashSet<string>();
        foreach (var action in Enum.GetValues<TimerAction>())
        {
            var pair = Hotkeys.For(action) ?? HotkeySettings.Default(action);
            var fallback = HotkeySettings.Default(action);
            string key = Canonical(pair.Key, pad: false, fallback.Key), padName = Canonical(pair.Pad, pad: true, fallback.Pad);
            if (key.Length > 0 && !usedKeys.Add(key))
                key = "";
            if (padName.Length > 0 && !usedPads.Add(padName))
                padName = "";
            Hotkeys.Set(action, new HotkeyPair { Key = key, Pad = padName });
        }

        Colors ??= new();
        Colors.Timer = Color(Colors.Timer, ColorSettings.DefaultTimer);
        Colors.Selected = Color(Colors.Selected, ColorSettings.DefaultSelected);
        Colors.Last20 = Color(Colors.Last20, ColorSettings.DefaultLast20);
        Colors.Background = Color(Colors.Background, ColorSettings.DefaultBackground);

        Overlay ??= new();
        if (Overlay.Height != 0)
            Overlay.Height = OverlayLayout.ClampHeight(Overlay.Height);
    }

    static string Canonical(string? name, bool pad, string fallback)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name == null ? fallback : "";
        return BindingParser.TryParse(name, pad, out var binding) ? BindingParser.Format(binding) : fallback;
    }

    static string Color(string? hex, string fallback) => Ui.ColourMath.TryParse(hex, out string clean) ? clean : fallback;
}
