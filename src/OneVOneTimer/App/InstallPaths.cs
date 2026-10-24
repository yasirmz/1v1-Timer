using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OneVOneTimer.App;

/// <summary>
/// The installed timer: Velopack's folder (%LocalAppData%\1v1Timer), with the program in current\ beside Update.exe and
/// a small stub 1v1Timer.exe at the top that always starts the current version. A development build is not installed.
/// </summary>
public static class InstallPaths
{
    /// <summary>The install folder when this timer runs from one (null for a development build).</summary>
    public static string? Root { get; } = Environment.ProcessPath is { } exe ? RootFor(exe) : null;

    /// <summary>What the Start menu, autostart and restarts launch: the stub when installed, else this exe.</summary>
    public static string LaunchExe => Root is { } root ? StubFor(root) : Environment.ProcessPath!;

    /// <summary>The install folder of <paramref name="exePath"/> (…\current\1v1Timer.exe beside Update.exe), or null.</summary>
    public static string? RootFor(string exePath)
    {
        string? folder = Path.GetDirectoryName(Path.GetFullPath(exePath));
        string? root = folder == null ? null : Path.GetDirectoryName(folder);
        return root != null && string.Equals(Path.GetFileName(folder), "current", StringComparison.OrdinalIgnoreCase)
            && File.Exists(Path.Combine(root, "Update.exe")) ? root : null;
    }

    public static string StubFor(string root) => Path.Combine(root, AppInfo.ExeName + ".exe");

    /// <summary><paramref name="path"/> is <paramref name="root"/> or a folder in it (any case, trailing slashes ignored).</summary>
    public static bool IsInside(string root, string path)
    {
        string r = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        string p = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        return p.Equals(r, StringComparison.OrdinalIgnoreCase) || p.StartsWith(r + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>The ids of timer processes running from <paramref name="root"/>, this one left out.</summary>
    public static IEnumerable<int> OwnProcesses(string root, IEnumerable<(int Id, string? Path)> processes, int self) =>
        processes.Where(p => p.Id != self && p.Path != null && IsInside(root, p.Path)).Select(p => p.Id);

    /// <summary>
    /// Velopack's start, first in Main: its install, update and uninstall steps run the timer with arguments of their
    /// own. A downloaded update goes in only when the user presses Update.
    /// </summary>
    public static Velopack.VelopackApp VelopackHooks() =>
        Velopack.VelopackApp.Build().SetAutoApplyOnStartup(false).OnBeforeUninstallFastCallback(_ => UninstallHook());

    /// <summary>
    /// Velopack's uninstall hook: closes this install's timer (never another copy's), then removes its start-with-Windows
    /// entry. Velopack then removes the folder; the settings stay.
    /// </summary>
    public static void UninstallHook()
    {
        if (Root is not { } root)
            return;
        if (EventWaitHandle.TryOpenExisting(AppInfo.ExitEvent, out var exit))
            using (exit)
                exit.Set();
        var clock = Stopwatch.StartNew();
        while (Own(root).Count > 0 && clock.Elapsed < TimeSpan.FromSeconds(5))
            Thread.Sleep(200);
        foreach (int id in Own(root))
        {
            try
            {
                using var process = Process.GetProcessById(id);
                process.Kill();
                process.WaitForExit(5000); // its files closed before they are removed
            }
            catch (Exception e) when (e is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                // Gone meanwhile.
            }
        }
        Autostart.RemoveIfPointsInto(root);
        WizardLanguage.Forget();
    }

    static List<int> Own(string root)
    {
        var all = Process.GetProcessesByName(AppInfo.ExeName);
        try
        {
            return OwnProcesses(root, all.Select(p => (p.Id, ImagePath(p.Id))), Environment.ProcessId).ToList();
        }
        finally
        {
            foreach (var p in all)
                p.Dispose();
        }
    }

    /// <summary>A process's program file, or null when Windows does not say (another user's).</summary>
    static string? ImagePath(int processId)
    {
        IntPtr process = OpenProcess(0x1000 /* PROCESS_QUERY_LIMITED_INFORMATION */, false, processId);
        if (process == IntPtr.Zero)
            return null;
        try
        {
            var path = new char[1024];
            int size = path.Length;
            return QueryFullProcessImageName(process, 0, path, ref size) ? new string(path, 0, size) : null;
        }
        finally
        {
            CloseHandle(process);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr OpenProcess(int access, bool inherit, int processId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool QueryFullProcessImageName(IntPtr process, int flags, [Out] char[] path, ref int size);

    [DllImport("kernel32.dll")]
    static extern bool CloseHandle(IntPtr handle);
}
