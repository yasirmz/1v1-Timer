using System.Collections.Concurrent;
using System.Text.Json;

namespace OneVOneTimer.App;

/// <summary>
/// The words the timer shows (the tray and the settings window): one file per language in App/Lang, built into the
/// program. Only the language in use is read, and English fills any gap. Another language is one more file and one more
/// line in <see cref="Languages"/>.
/// </summary>
public static class Text
{
    /// <summary>A language: its code (its file's name), its name in itself, and whether it is written right to left.</summary>
    public sealed record Lang(string Code, string Name, bool RightToLeft = false);

    /// <summary>Every language, in the order they are offered (Capture's seven).</summary>
    public static IReadOnlyList<Lang> Languages { get; } =
    [
        new("ar", "العربية", RightToLeft: true),
        new("en", "English"),
        new("zh-Hans", "简体中文"),
        new("ru", "Русский"),
        new("es", "Español"),
        new("pt-BR", "Português (Brasil)"),
        new("de", "Deutsch"),
    ];

    /// <summary>The language an unknown or missing one becomes.</summary>
    public const string Fallback = "en";

    static Lang? Find(string? code) => Languages.FirstOrDefault(l => l.Code == code);

    public static bool IsSupported(string? code) => Find(code) != null;

    static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> Loaded = new();
    static Lang _current = Find(Fallback)!;

    /// <summary>The language in use (an unknown code is <see cref="Fallback"/>); its file is read when a word is first asked for.</summary>
    public static string Language
    {
        get => _current.Code;
        set => _current = Find(value) ?? Find(Fallback)!;
    }

    /// <summary>Written right to left: the settings window and the tray menu are mirrored.</summary>
    public static bool IsRightToLeft => _current.RightToLeft;

    static IReadOnlyDictionary<string, string> Words => Load(_current.Code);

    static IReadOnlyDictionary<string, string> English => Load(Fallback);

    /// <summary>A language's words, read once from the program (Lang.&lt;code&gt;.json).</summary>
    internal static IReadOnlyDictionary<string, string> Load(string code) => Loaded.GetOrAdd(code, c =>
    {
        using var stream = typeof(Text).Assembly.GetManifestResourceStream($"Lang.{c}.json")
            ?? throw new InvalidOperationException($"The {c} words are missing from the program.");
        return JsonSerializer.Deserialize<Dictionary<string, string>>(stream) ?? [];
    });

    public static string Get(string key) =>
        Words.TryGetValue(key, out var s) ? s : English.TryGetValue(key, out var en) ? en : key;

    public static string Format(string key, params object[] args) => string.Format(Get(key), args);

    public static IEnumerable<string> AllKeys() => English.Keys;
}
