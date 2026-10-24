using System.Reflection;
using System.Text.RegularExpressions;

namespace OneVOneTimer;

/// <summary>Every name the app registers with Windows, in one place so a rename is a single edit.</summary>
public static class AppInfo
{
    /// <summary>The name people see: the windows, the tray, the data folder.</summary>
    public const string Name = "1v1 Timer";
    /// <summary>The program's file name without ".exe", and Velopack's package id.</summary>
    public const string ExeName = "1v1Timer";
    public const string RunValueName = "1v1Timer";
    public const string Repository = "https://github.com/yasirmz/1v1-Timer";
    /// <summary>Where help, problems and ideas go: the project's GitHub issues.</summary>
    public const string HelpUrl = Repository + "/issues";

    /// <summary>
    /// The one-at-a-time lock, the second launch's "show the settings" and the uninstaller's "close": a run with a data
    /// folder of its own (a test) has names of its own, so it runs beside the installed timer.
    /// </summary>
    public static string InstanceMutex => Named(@"Local\1v1Timer");
    public static string ShowSettingsEvent => Named(@"Local\1v1Timer.ShowSettings");
    public static string ExitEvent => Named(@"Local\1v1Timer.Exit");

    static string Named(string name) => Settings.SettingsStore.OwnDataFolder ? name + ".test" : name;

    /// <summary>The full version, a prerelease part included ("1.0.0-beta.1"), without the build details after "+".</summary>
    public static string Version { get; } =
        (typeof(AppInfo).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0").Split('+')[0];

    /// <summary>A version as people read it: "1.0.0-beta.3" is "1.0.0 beta 3" in their language; any other stays as it is.</summary>
    public static string Display(string version) => Regex.Match(version, @"^(\d+\.\d+\.\d+)-beta\.(\d+)$") is { Success: true } m
        ? App.Text.Format("version.beta", m.Groups[1].Value, m.Groups[2].Value)
        : version;
}
