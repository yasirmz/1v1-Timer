using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using OneVOneTimer.App;
using OneVOneTimer.Settings;
using OneVOneTimer.Updates;

namespace OneVOneTimer.Ui;

/// <summary>
/// The settings window's state, over the timer's live settings: every change applies at once (<c>changed</c>) and is
/// saved by the timer. Clickthrough is shown here too but never saved.
/// </summary>
public sealed class SettingsViewModel : INotifyPropertyChanged
{
    readonly Action _changed, _languageChanged;
    readonly UpdateService _updates;
    readonly DispatcherTimer _resetTimeout = new() { Interval = TimeSpan.FromSeconds(4) };
    bool _confirmingReset;

    public event PropertyChangedEventHandler? PropertyChanged;

    public SettingsViewModel(AppSettings settings, UpdateService updates, Action changed, Action languageChanged)
    {
        Settings = settings;
        _updates = updates;
        _changed = changed;
        _languageChanged = languageChanged;
        Rows =
        [
            new HotkeyRow(this, TimerAction.StartStopReset, Text.Get("hotkeys.startStopReset")),
            new HotkeyRow(this, TimerAction.StartStop, Text.Get("hotkeys.startStop")),
            new HotkeyRow(this, TimerAction.SelectTimer1, Text.Get("hotkeys.timer1")),
            new HotkeyRow(this, TimerAction.SelectTimer2, Text.Get("hotkeys.timer2")),
        ];
        Languages = Text.Languages.Select(l => new Option(l.Name, l.Code)).ToList();
        CheckNowCommand = new RelayCommand(_ => _ = _updates.Check(), _ => _updates.State is not (UpdateState.Checking or UpdateState.Downloading or UpdateState.Restarting));
        UpdateCommand = new RelayCommand(_ => _ = _updates.UpdateNow(), _ => _updates.State is UpdateState.Available or UpdateState.Failed);
        ResetCommand = new RelayCommand(_ => Reset());
        OpenHelpCommand = new RelayCommand(_ => OpenLink(AppInfo.HelpUrl));
        _resetTimeout.Tick += (_, _) => StopConfirming();
        _updates.Changed += OnUpdateChanged;
    }

    public AppSettings Settings { get; }

    // ---- Hotkeys ----

    public IReadOnlyList<HotkeyRow> Rows { get; }

    /// <summary>
    /// A binding chosen in a box: an input drives one action, so the same key (or button) on another action is cleared
    /// there.
    /// </summary>
    internal void SetBinding(TimerAction action, bool pad, string value)
    {
        value ??= "";
        var pair = Settings.Hotkeys.For(action);
        if ((pad ? pair.Pad : pair.Key) == value)
            return;
        if (value.Length > 0)
            foreach (var other in Enum.GetValues<TimerAction>().Where(a => a != action).Select(Settings.Hotkeys.For))
            {
                if (pad && other.Pad == value)
                    other.Pad = "";
                else if (!pad && other.Key == value)
                    other.Key = "";
            }
        if (pad)
            pair.Pad = value;
        else
            pair.Key = value;
        foreach (var row in Rows)
            row.Refresh();
        _changed();
    }

    // ---- Options ----

    public bool StartTimerOnChange
    {
        get => Settings.StartTimerOnChange;
        set => Change(() => Settings.StartTimerOnChange = value);
    }

    public bool TransparentBackground
    {
        get => Settings.TransparentBackground;
        set => Change(() => Settings.TransparentBackground = value);
    }

    public bool ClickThrough
    {
        get => Settings.ClickThrough;
        set => Change(() => Settings.ClickThrough = value);
    }

    public bool StartWithWindows
    {
        get => Settings.StartWithWindows;
        set => Change(() => Settings.StartWithWindows = value);
    }

    // ---- Colors (each change shows on the timer at once) ----

    public string TimerColor
    {
        get => Settings.Colors.Timer;
        set => ChangeColor(value, c => Settings.Colors.Timer = c);
    }

    public string SelectedColor
    {
        get => Settings.Colors.Selected;
        set => ChangeColor(value, c => Settings.Colors.Selected = c);
    }

    public string Last20Color
    {
        get => Settings.Colors.Last20;
        set => ChangeColor(value, c => Settings.Colors.Last20 = c);
    }

    public string BackgroundColor
    {
        get => Settings.Colors.Background;
        set => ChangeColor(value, c => Settings.Colors.Background = c);
    }

    void ChangeColor(string value, Action<string> set)
    {
        if (ColourMath.TryParse(value, out string hex))
            Change(() => set(hex));
    }

    // ---- General ----

    public IReadOnlyList<Option> Languages { get; }

    public string Language
    {
        get => Settings.Language;
        set
        {
            if (value == Settings.Language || !Text.IsSupported(value))
                return;
            Settings.Language = value;
            Text.Language = value;
            _changed();
            _languageChanged();
        }
    }

    public bool CheckUpdates
    {
        get => Settings.CheckUpdates;
        set => Change(() => Settings.CheckUpdates = value);
    }

    /// <summary>The installed app can look for updates (a development build cannot).</summary>
    public bool CanCheckUpdates => _updates.Works;

    public ICommand CheckNowCommand { get; }

    public string CheckNowText => Text.Get(_updates.State switch
    {
        UpdateState.Checking => "update.checking",
        UpdateState.UpToDate => "update.upToDate",
        UpdateState.CheckFailed => "update.checkFailed",
        _ => "general.checkNow",
    });

    public bool CheckNowOk => _updates.State == UpdateState.UpToDate;
    public bool CheckNowFailed => _updates.State == UpdateState.CheckFailed;

    /// <summary>A newer version is offered (found, downloading, restarting or failed to download).</summary>
    public bool UpdateOffered => _updates.State is UpdateState.Available or UpdateState.Downloading or UpdateState.Restarting or UpdateState.Failed;

    public string UpdateText => _updates.State == UpdateState.Failed
        ? Text.Get("update.failed")
        : Text.Format("update.available", AppInfo.Display(_updates.NewVersion ?? ""));

    public string UpdateActionText => _updates.State switch
    {
        UpdateState.Downloading => Text.Format("update.downloading", _updates.Percent),
        UpdateState.Restarting => Text.Get("update.restarting"),
        UpdateState.Failed => Text.Get("update.retry"),
        _ => Text.Get("update.now"),
    };

    public ICommand UpdateCommand { get; }

    public ICommand ResetCommand { get; }

    /// <summary>"Reset", then "Click again to reset" for a few seconds.</summary>
    public string ResetText => Text.Get(_confirmingReset ? "general.resetConfirm" : "general.resetNow");

    public ICommand OpenHelpCommand { get; }

    public string VersionText => Text.Format("general.version", AppInfo.Display(AppInfo.Version));

    /// <summary>The window closed: it stops following the updates.</summary>
    public void Detach()
    {
        _updates.Changed -= OnUpdateChanged;
        _resetTimeout.Stop();
    }

    /// <summary>The timer changed a setting itself (Clickthrough from the tray): the window shows it.</summary>
    public void Refresh(string? property = null) => Raise(property);

    void Reset()
    {
        if (!_confirmingReset)
        {
            _confirmingReset = true;
            _resetTimeout.Start();
            Raise(nameof(ResetText));
            return;
        }
        StopConfirming();
        Settings.ResetToDefaults();
        foreach (var row in Rows)
            row.Refresh();
        _changed();
        Raise(null);
    }

    void StopConfirming()
    {
        _resetTimeout.Stop();
        _confirmingReset = false;
        Raise(nameof(ResetText));
    }

    void OnUpdateChanged()
    {
        foreach (string p in new[] { nameof(CheckNowText), nameof(CheckNowOk), nameof(CheckNowFailed), nameof(UpdateOffered), nameof(UpdateText), nameof(UpdateActionText) })
            Raise(p);
        CommandManager.InvalidateRequerySuggested();
    }

    void Change(Action set, [CallerMemberName] string? property = null)
    {
        set();
        Raise(property);
        _changed();
    }

    void Raise(string? property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    static void OpenLink(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true })?.Dispose();
        }
        catch (Exception e) when (e is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Log.Warn("open link: " + e.Message);
        }
    }
}

/// <summary>One action's row on the Hotkeys card: its name, its keyboard/mouse binding and its controller binding.</summary>
public sealed class HotkeyRow(SettingsViewModel owner, TimerAction action, string label) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Label => label;

    public string Key
    {
        get => owner.Settings.Hotkeys.For(action).Key;
        set => owner.SetBinding(action, pad: false, value);
    }

    public string Pad
    {
        get => owner.Settings.Hotkeys.For(action).Pad;
        set => owner.SetBinding(action, pad: true, value);
    }

    internal void Refresh()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Key)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pad)));
    }
}
