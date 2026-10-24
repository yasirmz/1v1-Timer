namespace OneVOneTimer.App;

/// <summary>Keeps the timer alive through errors: they are logged, and one on the UI thread does not end the app.</summary>
public static class Resilience
{
    public static void InstallHandlers()
    {
        System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
        System.Windows.Forms.Application.ThreadException += (_, e) => Log.Error("ui thread: " + e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Error("fatal: " + e.ExceptionObject);
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Warn("task: " + e.Exception);
            e.SetObserved();
        };
    }
}
