namespace OneVOneTimer.Timing;

/// <summary>Which of the four colors a timer's numbers take (the fourth, Background, is behind them).</summary>
public enum TimerColor { Timer, Selected, Last20 }

public static class TimerColors
{
    public static readonly TimeSpan Last20Window = TimeSpan.FromSeconds(20);

    /// <summary>
    /// The color of <paramref name="timer"/> (0 = Timer 1, 1 = Timer 2): Last 20 Seconds, then Selected Timer, then Timer.
    /// </summary>
    public static TimerColor For(int timer, int selected, TimeSpan timer1, TimeSpan timer2) =>
        timer == 1 && InLast20(timer1, timer2) ? TimerColor.Last20
        : timer == selected ? TimerColor.Selected
        : TimerColor.Timer;

    /// <summary>
    /// Timer 2 is in the last 20 seconds before it reaches Timer 1's time (both above 0). Once it reaches that time it
    /// is past the window and back to its usual color.
    /// </summary>
    public static bool InLast20(TimeSpan timer1, TimeSpan timer2) =>
        timer1 > TimeSpan.Zero && timer2 > TimeSpan.Zero && timer2 < timer1 && timer1 - timer2 <= Last20Window;
}
