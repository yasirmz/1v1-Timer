using System.Globalization;
using System.Text.Json;

namespace OneVOneTimer.Settings;

public static class SettingsStore
{
    const string DataVariable = "ONEVONETIMER_DATA";

    /// <summary>%AppData%\1v1 Timer; ONEVONETIMER_DATA points a test or development run at another folder.</summary>
    public static string AppDataDir { get; } = Environment.GetEnvironmentVariable(DataVariable) is { Length: > 0 } dev
        ? dev
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppInfo.Name);

    public static string DefaultPath { get; } = Path.Combine(AppDataDir, "settings.json");

    /// <summary>
    /// A run with a data folder of its own (a test or development run): it never touches what belongs to the installed
    /// timer outside that folder (Windows's start-up entry, the installer's language).
    /// </summary>
    public static bool OwnDataFolder { get; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(DataVariable));

    static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>
    /// The settings in <paramref name="path"/>: the defaults (in Windows' language) when there is no file; when it cannot
    /// be read as settings, the defaults too, and the file kept beside it as settings.bad.json. Never throws.
    /// </summary>
    public static AppSettings Load(string path, CultureInfo? uiCulture = null)
    {
        AppSettings settings;
        if (!File.Exists(path))
        {
            settings = new AppSettings();
        }
        else
        {
            string? json;
            try
            {
                json = ReadShared(path);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                App.Log.Warn("settings: " + e.Message);
                json = null;
            }
            try
            {
                settings = json == null ? new AppSettings() : Deserialize(json);
            }
            catch (JsonException e)
            {
                App.Log.Warn("settings: not readable, kept as settings.bad.json: " + e.Message);
                try
                {
                    File.WriteAllText(Path.Combine(Path.GetDirectoryName(path)!, "settings.bad.json"), json);
                }
                catch (Exception io) when (io is IOException or UnauthorizedAccessException)
                {
                }
                settings = new AppSettings();
            }
        }

        settings.Normalize(uiCulture);
        return settings;
    }

    /// <summary>
    /// Reads without blocking a concurrent save (which replaces the file) and retries briefly while something else (an
    /// antivirus scan) holds it.
    /// </summary>
    static string ReadShared(string path)
    {
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (IOException) when (attempt < 20 && File.Exists(path))
            {
                Thread.Sleep(50);
            }
        }
    }

    /// <summary>Written to a temporary file first, then moved over the old one, so a crash mid-save never leaves half a file.</summary>
    public static void Save(string path, AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, Serialize(settings));
        File.Move(tmp, path, overwrite: true);
    }

    public static string Serialize(AppSettings settings) => JsonSerializer.Serialize(settings, Options);

    public static AppSettings Deserialize(string json) => JsonSerializer.Deserialize<AppSettings>(json, Options) ?? new AppSettings();
}
