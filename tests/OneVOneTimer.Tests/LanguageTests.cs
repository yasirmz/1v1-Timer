using System.Text.RegularExpressions;
using OneVOneTimer.App;

namespace OneVOneTimer.Tests;

/// <summary>The seven languages: each one complete, with its blanks in place.</summary>
public static class LanguageTests
{
    static readonly Regex Blank = new(@"\{\d+(:[^}]*)?\}", RegexOptions.CultureInvariant);

    static string Blanks(string words) => string.Join(",", Blank.Matches(words).Select(m => m.Value).Order());

    [Test]
    public static void Every_language_has_every_text_with_the_same_blanks()
    {
        var english = Text.Load("en");
        var problems = new List<string>();
        foreach (var language in Text.Languages)
        {
            var words = Text.Load(language.Code);
            foreach (var (key, en) in english)
            {
                if (!words.TryGetValue(key, out var w) || string.IsNullOrWhiteSpace(w))
                    problems.Add($"{language.Code}: {key} missing");
                else if (Blanks(w) != Blanks(en))
                    problems.Add($"{language.Code}: {key} blanks differ");
            }
            foreach (string key in words.Keys.Where(k => !english.ContainsKey(k)))
                problems.Add($"{language.Code}: {key} is not one of the timer's texts");
        }
        Assert.True(problems.Count == 0, string.Join("; ", problems.Take(30)));
    }

    [Test]
    public static void Every_language_says_most_things_in_its_own_words()
    {
        var english = Text.Load("en");
        foreach (var language in Text.Languages.Where(l => l.Code != "en"))
        {
            var words = Text.Load(language.Code);
            int same = english.Count(e => words.TryGetValue(e.Key, out var w) && w == e.Value);
            // The name, "Timer", "Controller", "Version {0}" and the like may be the same.
            Assert.True(same <= 8, $"{language.Code}: {same} texts are left in English");
        }
    }

    [Test]
    public static void The_seven_languages_are_capture_s()
    {
        Assert.Equal("ar,en,zh-Hans,ru,es,pt-BR,de", string.Join(",", Text.Languages.Select(l => l.Code)), "the codes, in order");
        Assert.True(Text.Languages.Single(l => l.Code == "ar").RightToLeft, "Arabic is right to left");
        Assert.Equal(1, Text.Languages.Count(l => l.RightToLeft), "only Arabic");
    }

    [Test]
    public static void An_unknown_language_is_english()
    {
        string saved = Text.Language;
        try
        {
            Text.Language = "xx";
            Assert.Equal("en", Text.Language, "fallback");
            Assert.Equal("Settings", Text.Get("tray.settings"), "English words");
            Assert.Equal("no.such.key", Text.Get("no.such.key"), "a missing key shows itself");
        }
        finally
        {
            Text.Language = saved;
        }
    }
}
