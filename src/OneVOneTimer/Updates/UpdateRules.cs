namespace OneVOneTimer.Updates;

/// <summary>Where a look for an update stands, as the settings window and the tray show it.</summary>
public enum UpdateState { None, Checking, UpToDate, CheckFailed, Available, Downloading, Restarting, Failed }

/// <summary>The decisions about updates, apart from the downloading itself.</summary>
public static class UpdateRules
{
    /// <summary>The first look waits this long after the timer starts, so a start-up is never slowed by it.</summary>
    public static readonly TimeSpan FirstDelay = TimeSpan.FromSeconds(60);

    public static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    /// <summary>Time to look for an update: turned on, a minute after the start, then once a day.</summary>
    public static bool ShouldCheck(bool enabled, DateTime startedAt, DateTime? lastCheck, DateTime now) =>
        enabled && now - startedAt >= FirstDelay && (lastCheck is not { } last || now - last >= Interval);

    /// <summary>The local update folder named in the data folder's update-source.txt (testing before publishing), if it exists.</summary>
    public static string? LocalFeed(string appDataDir) => LocalSource(Path.Combine(appDataDir, "update-source.txt"));

    public static string? LocalSource(string overrideFile)
    {
        try
        {
            return File.Exists(overrideFile) && File.ReadAllText(overrideFile).Trim() is { Length: > 0 } folder && IsLocalFolder(folder)
                   && Directory.Exists(folder)
                ? folder
                : null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    /// A full path on one of this PC's fixed drives: never a network folder, which would hand updates to another machine.
    /// </summary>
    public static bool IsLocalFolder(string folder)
    {
        if (!Path.IsPathFullyQualified(folder) || folder.StartsWith(@"\\", StringComparison.Ordinal))
            return false;
        try
        {
            return new DriveInfo(Path.GetPathRoot(folder)!).DriveType == DriveType.Fixed;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
