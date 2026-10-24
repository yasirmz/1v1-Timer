using OneVOneTimer.App;

namespace OneVOneTimer;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        // Velopack's install, update and uninstall steps run the timer with arguments of their own (handled here).
        InstallPaths.VelopackHooks().Run();
        System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);

        using var mutex = new Mutex(true, AppInfo.InstanceMutex, out bool first);
        if (!first)
        {
            // Already running: a second launch means "show me the settings". This launch may bring a window to the front
            // (the user just started it); the running timer may too, so its settings open in front, not behind.
            AllowSetForegroundWindow(-1 /* ASFW_ANY */);
            if (EventWaitHandle.TryOpenExisting(AppInfo.ShowSettingsEvent, out var show))
                using (show)
                    show.Set();
            return 0;
        }

        Log.Open(Settings.SettingsStore.AppDataDir);
        Resilience.InstallHandlers();
        int code = new TimerApp(background: args.Contains("--background")).Run();
        GC.KeepAlive(mutex);
        return code;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern bool AllowSetForegroundWindow(int processId);
}
