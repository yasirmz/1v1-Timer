using OneVOneTimer.Timing;

namespace OneVOneTimer.Tests;

/// <summary>A clock moved by hand: the timer rules to the tick.</summary>
sealed class FakeTime : TimeProvider
{
    long _ticks = 5_000_000;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;
    public override long GetTimestamp() => _ticks;
    public void Advance(double seconds) => _ticks += TimeSpan.FromSeconds(seconds).Ticks;
}

public static class TimerRuleTests
{
    static (DuelClock Clock, FakeTime Time) New()
    {
        var time = new FakeTime();
        return (new DuelClock(time), time);
    }

    static TimeSpan S(double seconds) => TimeSpan.FromSeconds(seconds);

    [Test]
    public static void Start_stop_reset_starts_at_zero_stops_when_running_and_resets_when_stopped()
    {
        var (clock, time) = New();
        Assert.Equal(0, clock.Selected, "Timer 1 selected at the start");
        clock.StartStopReset();
        Assert.True(clock.IsRunning(0), "at 0.00 it starts");
        time.Advance(2.5);
        clock.StartStopReset();
        Assert.True(!clock.IsRunning(0), "running: it stops");
        Assert.Equal(S(2.5), clock.Elapsed(0), "the time stays");
        time.Advance(1);
        Assert.Equal(S(2.5), clock.Elapsed(0), "stopped: the time does not move");
        clock.StartStopReset();
        Assert.Equal(TimeSpan.Zero, clock.Elapsed(0), "stopped with time: back to 0.00");
        Assert.True(!clock.IsRunning(0), "a reset does not start it");
        clock.StartStopReset();
        Assert.True(clock.IsRunning(0), "at 0.00 again: it starts");
    }

    [Test]
    public static void Start_stop_goes_on_from_the_time_it_had()
    {
        var (clock, time) = New();
        clock.StartStop();
        time.Advance(3);
        clock.StartStop();
        time.Advance(10);
        clock.StartStop();
        time.Advance(1.25);
        Assert.Equal(S(4.25), clock.Elapsed(0), "3 s, then 1.25 s more");
        clock.StartStop();
        Assert.True(!clock.IsRunning(0), "and stops again");
    }

    [Test]
    public static void Reset_only_touches_the_selected_timer()
    {
        var (clock, time) = New();
        clock.StartStop();
        time.Advance(5);
        clock.StartStop();
        clock.Select(1);
        clock.StartStop();
        time.Advance(2);
        clock.StartStop();
        clock.StartStopReset(); // Timer 2 is selected and stopped with time
        Assert.Equal(TimeSpan.Zero, clock.Elapsed(1), "Timer 2 reset");
        Assert.Equal(S(5), clock.Elapsed(0), "Timer 1 untouched");
    }

    [Test]
    public static void Selecting_without_the_option_only_changes_which_timer_the_keys_work_on()
    {
        var (clock, time) = New();
        clock.StartStop();
        clock.Select(1);
        Assert.Equal(1, clock.Selected, "Timer 2 selected");
        Assert.True(clock.IsRunning(0), "Timer 1 keeps running");
        Assert.True(!clock.IsRunning(1), "Timer 2 not started");
        time.Advance(1);
        Assert.Equal(S(1), clock.Elapsed(0), "Timer 1 counted on");
    }

    [Test]
    public static void Start_timer_on_change_hands_over_at_the_same_instant()
    {
        var (clock, time) = New();
        clock.StartTimerOnChange = true;
        clock.StartStop();
        time.Advance(7.31);
        clock.Select(1);
        Assert.True(!clock.IsRunning(0), "the one left stops");
        Assert.True(clock.IsRunning(1), "the new one starts");
        Assert.Equal(S(7.31), clock.Elapsed(0), "Timer 1 stopped right at the switch");
        Assert.Equal(TimeSpan.Zero, clock.Elapsed(1), "Timer 2 started right at the switch");
        time.Advance(2);
        clock.Select(0);
        Assert.Equal(S(2), clock.Elapsed(1), "Timer 2 ran exactly until the switch back");
        Assert.True(clock.IsRunning(0), "Timer 1 goes on");
        time.Advance(1);
        Assert.Equal(S(8.31), clock.Elapsed(0), "from its own time");
    }

    [Test]
    public static void Start_timer_on_change_starts_the_new_timer_even_when_none_ran()
    {
        var (clock, _) = New();
        clock.StartTimerOnChange = true;
        clock.Select(1);
        Assert.True(clock.IsRunning(1), "switching starts the new one");
        Assert.True(!clock.IsRunning(0), "the other stays stopped");
    }

    [Test]
    public static void Selecting_the_selected_timer_does_nothing()
    {
        var (clock, time) = New();
        clock.StartTimerOnChange = true;
        clock.StartStop();
        time.Advance(1);
        clock.Select(0);
        Assert.True(clock.IsRunning(0), "still running");
        Assert.True(!clock.IsRunning(1), "Timer 2 not started");
        clock.StartStop();
        clock.Select(0);
        Assert.True(!clock.IsRunning(0), "a stopped timer is not started by selecting it again");
    }

    [Test]
    public static void Under_a_hundredth_shows_0_00_so_start_stop_reset_starts_it()
    {
        var (clock, time) = New();
        clock.StartStop();
        time.Advance(0.004);
        clock.StartStop();
        Assert.Equal("0.00", TimeFormat.Format(clock.Elapsed(0)), "shows 0.00");
        clock.StartStopReset();
        Assert.True(clock.IsRunning(0), "at 0.00: start, not reset");
        time.Advance(1);
        Assert.Equal(S(1), clock.Elapsed(0), "from 0.00");
    }
}
