namespace OneVOneTimer.App;

/// <summary>
/// A small log in the data folder's logs\ (this run and the one before), to attach to a GitHub issue when something
/// goes wrong. Writing it never fails the app.
/// </summary>
public static class Log
{
    static readonly object Gate = new();
    static string? _path;

    public static void Open(string dataDir)
    {
        try
        {
            string dir = Path.Combine(dataDir, "logs");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "1v1timer.log");
            if (File.Exists(_path))
                File.Move(_path, Path.Combine(dir, "1v1timer.old.log"), overwrite: true);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            _path = null;
        }
        Info($"1v1 Timer {AppInfo.Version} started");
    }

    public static void Info(string line) => Write("info", line);
    public static void Warn(string line) => Write("warn", line);
    public static void Error(string line) => Write("error", line);

    static void Write(string level, string line)
    {
        if (_path == null)
            return;
        lock (Gate)
        {
            try
            {
                File.AppendAllText(_path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {level} {line}{Environment.NewLine}");
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
            }
        }
    }
}
