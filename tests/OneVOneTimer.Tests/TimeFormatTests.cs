using OneVOneTimer.Timing;

namespace OneVOneTimer.Tests;

public static class TimeFormatTests
{
    static string F(double seconds) => TimeFormat.Format(TimeSpan.FromTicks((long)Math.Round(seconds * TimeSpan.TicksPerSecond)));

    [Test]
    public static void Seconds_with_two_decimals_under_a_minute()
    {
        Assert.Equal("0.00", F(0), "zero");
        Assert.Equal("0.05", F(0.05), "five hundredths");
        Assert.Equal("9.99", F(9.99), "one digit");
        Assert.Equal("12.34", F(12.34), "two digits");
        Assert.Equal("59.99", F(59.99), "just under a minute");
    }

    [Test]
    public static void Minutes_from_sixty_seconds()
    {
        Assert.Equal("1:00.00", F(60), "a minute");
        Assert.Equal("1:33.45", F(93.45), "the spec's example");
        Assert.Equal("9:59.99", F(599.99), "the widest the window is made for");
        Assert.Equal("10:00.00", F(600), "ten minutes");
        Assert.Equal("60:00.00", F(3600), "an hour stays in minutes");
        Assert.Equal("75:12.34", F(75 * 60 + 12.34), "past an hour");
    }

    [Test]
    public static void Hundredths_are_cut_never_rounded_up()
    {
        Assert.Equal("59.99", F(59.999), "never shows a minute not reached yet");
        Assert.Equal("0.00", F(0.0099), "under a hundredth");
        Assert.Equal("1:33.44", F(93.4499), "cut in minutes too");
    }

    [Test]
    public static void Negative_time_reads_zero()
    {
        Assert.Equal("0.00", TimeFormat.Format(TimeSpan.FromSeconds(-3)), "never negative");
    }

    [Test]
    public static void Patterns_put_every_digit_as_zero()
    {
        Assert.Equal("00.00", TimeFormat.Pattern("12.34"), "seconds");
        Assert.Equal("0:00.00", TimeFormat.Pattern("1:33.45"), "minutes");
        Assert.Equal(TimeFormat.Reserved, TimeFormat.Pattern(F(599.99)), "9:59.99 fits the room kept from the start");
        Assert.Equal("00:00.00", TimeFormat.Pattern(F(600)), "10:00.00 needs more");
    }
}
