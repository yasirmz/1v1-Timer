using Microsoft.Win32;

namespace OneVOneTimer.App;

/// <summary>
/// The timer's language where the install wizard looks for its previous one (Inno Setup's UsePreviousLanguage reads the
/// key of its AppId, "1v1Timer.Wizard"): installing over the timer then opens in its language. The key has no
/// DisplayName, so Apps &amp; features does not list it.
/// </summary>
public static class WizardLanguage
{
    public static string DefaultKey => @"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + PackageId() + ".Wizard_is1";
    public const string ValueName = "Inno Setup: Language";

    /// <summary>The wizard's name for a language (its [Languages] names carry no dash).</summary>
    public static string InnoName(string code) => code.Replace("-", "", StringComparison.Ordinal);

    public static void Remember(string code, string? keyPath = null)
    {
        string name = InnoName(code);
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath ?? DefaultKey);
        if (key.GetValue(ValueName) as string != name)
            key.SetValue(ValueName, name);
    }

    /// <summary>On uninstall: the key goes (none is fine).</summary>
    public static void Forget(string? keyPath = null) =>
        Registry.CurrentUser.DeleteSubKeyTree(keyPath ?? DefaultKey, throwOnMissingSubKey: false);

    /// <summary>Velopack's id for this install, or the program's name when it has none (a development run).</summary>
    static string PackageId()
    {
        try
        {
            return Velopack.Locators.VelopackLocator.Current?.AppId is { Length: > 0 } id ? id : AppInfo.ExeName;
        }
        catch (Exception)
        {
            return AppInfo.ExeName; // not installed by Velopack
        }
    }
}
