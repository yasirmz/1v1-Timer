using OneVOneTimer.Timing;

namespace OneVOneTimer.Tests;

public static class TimerColorTests
{
    static TimeSpan S(double seconds) => TimeSpan.FromSeconds(seconds);

    static TimerColor Timer2(double timer1, double timer2, int selected = 0) => TimerColors.For(1, selected, S(timer1), S(timer2));

    [Test]
    public static void Timer_2_turns_last_20_in_the_20_seconds_before_timer_1s_time()
    {
        Assert.Equal(TimerColor.Last20, Timer2(60, 40), "exactly 20 s before");
        Assert.Equal(TimerColor.Last20, Timer2(60, 45.5), "inside the window");
        Assert.Equal(TimerColor.Last20, Timer2(60, 59.99), "a hundredth before");
        Assert.Equal(TimerColor.Timer, Timer2(60, 39.99), "just before the window");
        Assert.Equal(TimerColor.Timer, Timer2(60, 10), "far before");
    }

    [Test]
    public static void Timer_2_goes_back_to_its_color_once_it_reaches_timer_1()
    {
        Assert.Equal(TimerColor.Timer, Timer2(60, 60), "reached");
        Assert.Equal(TimerColor.Timer, Timer2(60, 60.01), "passed");
        Assert.Equal(TimerColor.Selected, Timer2(60, 75, selected: 1), "passed and selected");
    }

    [Test]
    public static void Both_timers_must_have_time()
    {
        Assert.Equal(TimerColor.Timer, Timer2(0, 0), "both at zero");
        Assert.Equal(TimerColor.Timer, Timer2(15, 0), "Timer 2 at zero");
        Assert.Equal(TimerColor.Timer, Timer2(0, 5), "Timer 1 at zero");
        Assert.Equal(TimerColor.Last20, Timer2(15, 0.01), "Timer 2 just started, Timer 1 under 20 s");
    }

    [Test]
    public static void Last_20_comes_before_selected_which_comes_before_timer()
    {
        Assert.Equal(TimerColor.Last20, Timer2(30, 20, selected: 1), "Last 20 Seconds over Selected Timer");
        Assert.Equal(TimerColor.Selected, TimerColors.For(0, 0, S(30), S(20)), "Timer 1 selected");
        Assert.Equal(TimerColor.Timer, TimerColors.For(0, 1, S(30), S(20)), "Timer 1 not selected");
        Assert.Equal(TimerColor.Selected, Timer2(30, 5, selected: 1), "Timer 2 selected, outside the window");
    }

    [Test]
    public static void Timer_1_never_takes_the_last_20_color()
    {
        Assert.Equal(TimerColor.Timer, TimerColors.For(0, 1, S(40), S(60)), "Timer 1 close behind Timer 2");
        Assert.Equal(TimerColor.Selected, TimerColors.For(0, 0, S(40), S(60)), "and selected");
    }
}
