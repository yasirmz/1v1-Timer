namespace OneVOneTimer.Timing;

/// <summary>
/// The two timers and which one the hotkeys work on. Time comes from <see cref="TimeProvider"/> (the high-resolution
/// stopwatch; a hand-moved clock in tests). Timer 1 is index 0, Timer 2 index 1.
/// </summary>
public sealed class DuelClock(TimeProvider time)
{
    readonly long?[] _since = new long?[2];
    readonly TimeSpan[] _banked = new TimeSpan[2];

    /// <summary>The timer the hotkeys work on (0 or 1).</summary>
    public int Selected { get; private set; }

    /// <summary>Switching timers stops the one you leave and starts the new one.</summary>
    public bool StartTimerOnChange { get; set; }

    public bool IsRunning(int timer) => _since[timer] != null;

    public bool AnyRunning => IsRunning(0) || IsRunning(1);

    public TimeSpan Elapsed(int timer) => Elapsed(timer, time.GetTimestamp());

    /// <summary>A timer's time at <paramref name="now"/> (one reading for both timers on a frame).</summary>
    public TimeSpan Elapsed(int timer, long now) =>
        _since[timer] is { } since ? _banked[timer] + time.GetElapsedTime(since, now) : _banked[timer];

    public long Now() => time.GetTimestamp();

    /// <summary>The selected timer: running, it stops; stopped with time on it, it goes back to 0.00; at 0.00, it starts.</summary>
    public void StartStopReset()
    {
        int t = Selected;
        long now = time.GetTimestamp();
        if (IsRunning(t))
            Stop(t, now);
        else if (TimeFormat.Hundredths(_banked[t]) > 0)
            _banked[t] = TimeSpan.Zero;
        else
        {
            _banked[t] = TimeSpan.Zero; // under a hundredth shows 0.00: it starts from there
            Start(t, now);
        }
    }

    /// <summary>The selected timer: running, it stops; otherwise it starts, going on from its time.</summary>
    public void StartStop()
    {
        int t = Selected;
        long now = time.GetTimestamp();
        if (IsRunning(t))
            Stop(t, now);
        else
            Start(t, now);
    }

    /// <summary>
    /// Makes <paramref name="timer"/> the selected one. With <see cref="StartTimerOnChange"/>, the one left stops and the
    /// new one starts at the same instant. Selecting the selected timer does nothing.
    /// </summary>
    public void Select(int timer)
    {
        if (timer is not (0 or 1) || timer == Selected)
            return;
        if (StartTimerOnChange)
        {
            long now = time.GetTimestamp();
            Stop(Selected, now);
            Start(timer, now);
        }
        Selected = timer;
    }

    void Start(int t, long now) => _since[t] ??= now;

    void Stop(int t, long now)
    {
        if (_since[t] is not { } since)
            return;
        _banked[t] += time.GetElapsedTime(since, now);
        _since[t] = null;
    }
}
