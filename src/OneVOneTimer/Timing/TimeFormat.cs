using System.Globalization;

namespace OneVOneTimer.Timing;

/// <summary>How a timer reads: seconds with two decimals ("12.34"), and from a minute on "m:ss.ff" ("1:33.45", "75:12.34").</summary>
public static class TimeFormat
{
    /// <summary>The widest text a timer has room for from the start (9:59.99), as a pattern: past it, the window widens.</summary>
    public const string Reserved = "0:00.00";

    /// <summary>Whole hundredths (cut, never rounded up: 59.999 is 59.99, a stopwatch never shows time not reached yet).</summary>
    public static long Hundredths(TimeSpan t) => t <= TimeSpan.Zero ? 0 : t.Ticks / (TimeSpan.TicksPerMillisecond * 10);

    public static string Format(TimeSpan t)
    {
        long hundredths = Hundredths(t);
        long seconds = hundredths / 100, fraction = hundredths % 100;
        return seconds < 60
            ? string.Create(CultureInfo.InvariantCulture, $"{seconds}.{fraction:00}")
            : string.Create(CultureInfo.InvariantCulture, $"{seconds / 60}:{seconds % 60:00}.{fraction:00}");
    }

    /// <summary>
    /// The text with every digit as 0 ("1:33.45" is "0:00.00"): texts with one pattern take the same room, since every
    /// digit is as wide as the others, so the window and the numbers keep still while the time runs.
    /// </summary>
    public static string Pattern(string text) => string.Create(text.Length, text, (span, s) =>
    {
        for (int i = 0; i < s.Length; i++)
            span[i] = char.IsAsciiDigit(s[i]) ? '0' : s[i];
    });
}
