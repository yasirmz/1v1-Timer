using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using OneVOneTimer.Input;
using OneVOneTimer.Overlay;
using OneVOneTimer.Settings;
using OneVOneTimer.Timing;
using OneVOneTimer.Ui;
using OneVOneTimer.Updates;

namespace OneVOneTimer.App;

/// <summary>
/// The running timer, on one UI thread: the overlay, the tray, the hotkey listeners, the settings window and the
/// updates. Hotkeys only listen (Raw Input and XInput), so the game keeps every key and button.
/// </summary>
public sealed class TimerApp(bool background)
{
    static readonly TimeSpan SaveDelay = TimeSpan.FromMilliseconds(400);

    readonly DuelClock _clock = new(TimeProvider.System);
    readonly DateTime _startedAt = DateTime.UtcNow;
    readonly string _settingsPath = SettingsStore.DefaultPath;
    AppSettings _settings = new();
    System.Windows.Application _app = null!;
    OverlayWindow _overlay = null!;
    TrayController _tray = null!;
    RawInputListener? _keys;
    PadPoller? _pads;
    UpdateService _updates = null!;
    SettingsWindow? _settingsWindow;
    SettingsViewModel? _vm;
    DispatcherTimer _saveTimer = null!;
    readonly Brush[] _brushes = new Brush[4];
    Brush? _outline;
    bool _rendering, _exiting;

    public int Run()
    {
        bool firstRun = !File.Exists(_settingsPath);
        _settings = SettingsStore.Load(_settingsPath);
        bool chosen = InstallChoices.ConsumeInto(_settings, SettingsStore.AppDataDir);
        Text.Language = _settings.Language;

        _app = CreateApplication();
        // WinForms parts (the tray and its menu) run on WPF's message loop.
        System.Windows.Forms.Integration.WindowsFormsHost.EnableWindowsFormsInterop();
        // Where the controller thread and the update checks hand their news to (the loop is not running yet).
        var ui = new DispatcherSynchronizationContext(_app.Dispatcher);
        _saveTimer = new DispatcherTimer { Interval = SaveDelay };
        _saveTimer.Tick += (_, _) => SaveNow();

        _updates = UpdateService.ForThisInstall(SettingsStore.AppDataDir, ui);
        _updates.Changed += () => _tray.Update(_settings.ClickThrough, OfferedUpdate());
        _updates.Restarting += Exit;

        _overlay = new OverlayWindow();
        _overlay.Placed += OnPlaced;
        _overlay.MenuWanted += () => _tray.ShowMenu();
        _overlay.Closed += (_, _) => Exit();

        _tray = new TrayController();
        _tray.SettingsClicked += OpenSettings;
        _tray.ClickThroughClicked += () => SetClickThrough(!_settings.ClickThrough);
        _tray.UpdateClicked += () => _ = _updates.UpdateNow();
        _tray.ExitClicked += Exit;

        _keys = new RawInputListener();
        _keys.Pressed += OnInput;
        _pads = new PadPoller(ui);
        _pads.Pressed += OnInput;

        ApplySettings();
        PlaceOverlay();
        _overlay.Show();
        Render();
        if (chosen || firstRun)
            SaveNow();

        StartTimers();
        ListenForOtherLaunches();
        // The first start after installing: the settings show what the hotkeys are.
        if (firstRun && !background)
            OpenSettings();

        _app.Run();

        SaveNow();
        _keys?.Dispose();
        _pads?.Dispose();
        _tray.Dispose();
        Log.Info("exit");
        return 0;
    }

    static System.Windows.Application CreateApplication()
    {
        var app = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        app.DispatcherUnhandledException += (_, e) =>
        {
            Log.Error("ui: " + e.Exception);
            e.Handled = true;
        };
#pragma warning disable WPF0001
        app.ThemeMode = ThemeMode.Dark;
#pragma warning restore WPF0001
        app.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/1v1Timer;component/Ui/Theme.xaml"),
        });
        return app;
    }

    void StartTimers()
    {
        // Back on top every few seconds: a borderless game can cover even an always-on-top window.
        var topmost = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        topmost.Tick += (_, _) =>
        {
            if (!_tray.MenuOpen)
                _overlay.KeepOnTop();
        };
        topmost.Start();

        var updates = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        updates.Tick += (_, _) =>
        {
            if (_updates.Works && UpdateRules.ShouldCheck(_settings.CheckUpdates, _startedAt, _updates.LastCheck, DateTime.UtcNow))
                _ = _updates.Check();
        };
        updates.Start();
    }

    /// <summary>A second launch asks for the settings; the uninstaller asks the timer to close.</summary>
    void ListenForOtherLaunches()
    {
        var show = new EventWaitHandle(false, EventResetMode.AutoReset, AppInfo.ShowSettingsEvent);
        var exit = new EventWaitHandle(false, EventResetMode.AutoReset, AppInfo.ExitEvent);
        new Thread(() =>
        {
            var both = new WaitHandle[] { show, exit };
            while (true)
            {
                int which = WaitHandle.WaitAny(both);
                Action act = which == 0 ? OpenSettings : Exit;
                _app.Dispatcher.InvokeAsync(act);
                if (which == 1)
                    return;
            }
        }) { IsBackground = true, Name = "1v1 Timer launches" }.Start();
    }

    // ---- Hotkeys ----

    void OnInput(InputBinding input)
    {
        // A hotkey box waiting for an input takes it, and nothing else happens.
        if (BindingBox.Listening is { } box)
        {
            box.Offer(input);
            return;
        }
        string name = BindingParser.Format(input);
        foreach (var action in Enum.GetValues<TimerAction>())
        {
            var pair = _settings.Hotkeys.For(action);
            if (input.Kind == InputKind.Pad ? pair.Pad == name : pair.Key == name)
                Perform(action);
        }
    }

    void Perform(TimerAction action)
    {
        switch (action)
        {
            case TimerAction.StartStopReset:
                _clock.StartStopReset();
                break;
            case TimerAction.StartStop:
                _clock.StartStop();
                break;
            case TimerAction.SelectTimer1:
                _clock.Select(0);
                break;
            case TimerAction.SelectTimer2:
                _clock.Select(1);
                break;
        }
        Render();
    }

    // ---- The overlay ----

    /// <summary>
    /// Draws the timers now. While one runs, WPF's frame clock calls this every frame (60 a second); with both stopped
    /// nothing is redrawn.
    /// </summary>
    void Render()
    {
        long now = _clock.Now();
        TimeSpan t1 = _clock.Elapsed(0, now), t2 = _clock.Elapsed(1, now);
        Brush Fill(int timer) => _brushes[(int)TimerColors.For(timer, _clock.Selected, t1, t2)];
        if (_overlay.Face.Show(TimeFormat.Format(t1), Fill(0), TimeFormat.Format(t2), Fill(1), _outline))
            _overlay.Reshape();

        bool running = _clock.AnyRunning;
        if (running && !_rendering)
            CompositionTarget.Rendering += OnFrame;
        else if (!running && _rendering)
            CompositionTarget.Rendering -= OnFrame;
        _rendering = running;
    }

    void OnFrame(object? sender, EventArgs e) => Render();

    /// <summary>Where the overlay was left (kept on a screen that is still there), at its saved height.</summary>
    void PlaceOverlay()
    {
        var place = _settings.Overlay;
        var primary = System.Windows.Forms.Screen.PrimaryScreen!.Bounds;
        double scale = DpiScale();
        int height = place.Height > 0 ? OverlayLayout.ClampHeight(place.Height) : (int)Math.Round(OverlayLayout.DefaultHeight * scale);
        int width = OverlayLayout.WidthFor(height, _overlay.Aspect);
        int x = place.X ?? primary.Left, y = place.Y ?? primary.Top;
        var wanted = new System.Drawing.Rectangle(x, y, width, height);
        // At least a corner of it on some screen, or back to the main screen's top-left.
        bool visible = System.Windows.Forms.Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(new System.Drawing.Rectangle(x + 8, y + 8, Math.Max(1, width - 16), Math.Max(1, height - 16))));
        if (!visible)
            wanted.Location = primary.Location;
        _overlay.Place(new PixelRect(wanted.Left, wanted.Top, wanted.Right, wanted.Bottom));
    }

    static double DpiScale()
    {
        using var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
        return g.DpiY / 96.0;
    }

    void OnPlaced()
    {
        var r = _overlay.Bounds;
        _settings.Overlay.X = r.Left;
        _settings.Overlay.Y = r.Top;
        _settings.Overlay.Height = r.Height;
        ScheduleSave();
    }

    void SetClickThrough(bool on)
    {
        _settings.ClickThrough = on;
        ApplySettings();
        _vm?.Refresh(nameof(SettingsViewModel.ClickThrough));
    }

    // ---- Settings ----

    /// <summary>The settings as they are now, on the overlay, the tray and Windows' start-up list.</summary>
    void ApplySettings()
    {
        var colors = _settings.Colors;
        _brushes[(int)TimerColor.Timer] = Frozen(colors.Timer);
        _brushes[(int)TimerColor.Selected] = Frozen(colors.Selected);
        _brushes[(int)TimerColor.Last20] = Frozen(colors.Last20);
        var back = ColourMath.ToColor(colors.Background);
        // Transparent: almost nothing behind the numbers (alpha 1, so the window can still be grabbed), and a thin outline
        // in the background color.
        var fill = new SolidColorBrush(_settings.TransparentBackground ? System.Windows.Media.Color.FromArgb(1, 0, 0, 0) : back);
        fill.Freeze();
        _overlay.Background = fill;
        _outline = _settings.TransparentBackground ? Frozen(colors.Background, alpha: 210) : null;
        _overlay.ClickThrough = _settings.ClickThrough;
        _clock.StartTimerOnChange = _settings.StartTimerOnChange;
        _tray.Update(_settings.ClickThrough, OfferedUpdate());
        if (InstallPaths.Root != null && !SettingsStore.OwnDataFolder)
        {
            try
            {
                Autostart.Sync(_settings.StartWithWindows, InstallPaths.LaunchExe);
                WizardLanguage.Remember(_settings.Language);
            }
            catch (Exception e) when (e is UnauthorizedAccessException or System.Security.SecurityException or IOException)
            {
                Log.Warn("windows settings: " + e.Message);
            }
        }
        if (_overlay.IsLoaded)
            Render();
    }

    static Brush Frozen(string hex, byte alpha = 255)
    {
        var c = ColourMath.ToColor(hex);
        var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, c.R, c.G, c.B));
        brush.Freeze();
        return brush;
    }

    string? OfferedUpdate() => _updates.State is UpdateState.Available or UpdateState.Failed && _updates.NewVersion is { } v
        ? AppInfo.Display(v)
        : null;

    void OnSettingsChanged()
    {
        ApplySettings();
        ScheduleSave();
    }

    void ScheduleSave()
    {
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    void SaveNow()
    {
        _saveTimer?.Stop();
        try
        {
            SettingsStore.Save(_settingsPath, _settings);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Log.Warn("settings save: " + e.Message);
        }
    }

    void OpenSettings()
    {
        if (_exiting)
            return;
        if (_settingsWindow != null)
        {
            if (_settingsWindow.WindowState == WindowState.Minimized)
                _settingsWindow.WindowState = WindowState.Normal;
            _settingsWindow.Activate();
            return;
        }
        ShowSettingsWindow(null);
    }

    /// <summary>A new settings window, where <paramref name="old"/> was (the language changed), the old one then closed.</summary>
    void ShowSettingsWindow(SettingsWindow? old)
    {
        _vm = new SettingsViewModel(_settings, _updates, OnSettingsChanged, OnLanguageChanged);
        var window = new SettingsWindow(_vm);
        window.Closed += (_, _) =>
        {
            if (_settingsWindow == window)
            {
                _settingsWindow = null;
                _vm = null;
            }
        };
        if (old != null)
            WindowPlacement.Carry(old, window);
        _settingsWindow = window;
        window.Show();
        window.Activate();
        old?.Close();
    }

    void OnLanguageChanged()
    {
        _tray.Update(_settings.ClickThrough, OfferedUpdate());
        // Built again in the new language (and direction), after the list that changed it has closed.
        var old = _settingsWindow;
        if (old != null)
            _app.Dispatcher.InvokeAsync(() => ShowSettingsWindow(old), DispatcherPriority.Background);
    }

    void Exit()
    {
        if (_exiting)
            return;
        _exiting = true;
        if (_rendering)
            CompositionTarget.Rendering -= OnFrame;
        _settingsWindow?.Close();
        _overlay.Close();
        _app.Shutdown();
    }
}
