using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OneVOneTimer.Input;

/// <summary>
/// Xbox controllers (and others that work like one) through XInput, read on a background thread: a connected slot every
/// few milliseconds, an empty one every 2 seconds (asking an empty slot is slow), and with none connected the thread
/// mostly sleeps. Controllers are read whichever app is in front. Presses are posted to the UI thread.
/// </summary>
public sealed class PadPoller : IDisposable
{
    public static readonly TimeSpan EmptySlotInterval = TimeSpan.FromSeconds(2);
    /// <summary>A trigger counts as pressed above this (of 255) and as let go below <see cref="TriggerRelease"/>.</summary>
    public const int TriggerPress = 64, TriggerRelease = 32;
    const int Slots = 4, ErrorSuccess = 0;

    static readonly PadButton[] All = Enum.GetValues<PadButton>();

    readonly SynchronizationContext _ui;
    readonly Thread _thread;
    volatile bool _running = true;

    public event Action<InputBinding>? Pressed;

    /// <param name="ui">Where presses are posted (the UI thread).</param>
    public PadPoller(SynchronizationContext ui)
    {
        _ui = ui;
        _thread = new Thread(Run) { IsBackground = true, Name = "1v1 Timer controllers" };
        _thread.Start();
    }

    void Run()
    {
        var previous = new int[Slots];
        var connected = new bool[Slots];
        var nextLook = new long[Slots];
        var clock = Stopwatch.StartNew();
        try
        {
            while (_running)
            {
                long now = clock.ElapsedMilliseconds;
                for (int slot = 0; slot < Slots; slot++)
                {
                    if (!connected[slot] && now < nextLook[slot])
                        continue;
                    if (XInputGetState(slot, out var state) != ErrorSuccess)
                    {
                        connected[slot] = false;
                        nextLook[slot] = now + (long)EmptySlotInterval.TotalMilliseconds;
                        continue;
                    }
                    var pad = state.Gamepad;
                    int buttons = Buttons(pad.Buttons, pad.LeftTrigger, pad.RightTrigger, previous[slot]);
                    // Buttons already held when a controller connects are not presses.
                    int pressed = connected[slot] ? buttons & ~previous[slot] : 0;
                    if (!connected[slot])
                        TimerResolution.Raise(); // a controller is there: the 4 ms waits are 4 ms
                    connected[slot] = true;
                    previous[slot] = buttons;
                    if (pressed != 0)
                        foreach (var button in All)
                            if ((pressed & (int)button) != 0)
                                _ui.Post(b => Pressed?.Invoke((InputBinding)b!), InputBinding.Pad(button));
                }
                // With no controller connected nothing is read: the thread only wakes to look at the empty slots.
                Thread.Sleep(connected.Contains(true) ? 4 : 250);
            }
        }
        catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException)
        {
            App.Log.Warn("xinput: " + e.Message); // no controllers this run; the keyboard and mouse still work
        }
    }

    /// <summary>
    /// The buttons held, the triggers among them: a trigger counts once pulled past <see cref="TriggerPress"/> and until
    /// let back below <see cref="TriggerRelease"/>, so a trigger resting half way never flickers.
    /// </summary>
    internal static int Buttons(ushort buttons, byte leftTrigger, byte rightTrigger, int previous)
    {
        int held = buttons & 0xF3FF; // XInput's buttons (0x0400 and 0x0800 are not buttons)
        if (Trigger(leftTrigger, (previous & (int)PadButton.LT) != 0))
            held |= (int)PadButton.LT;
        if (Trigger(rightTrigger, (previous & (int)PadButton.RT) != 0))
            held |= (int)PadButton.RT;
        return held;
    }

    static bool Trigger(byte value, bool wasHeld) => wasHeld ? value >= TriggerRelease : value > TriggerPress;

    public void Dispose()
    {
        _running = false;
        _thread.Join(500);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct XINPUT_GAMEPAD
    {
        public ushort Buttons;
        public byte LeftTrigger, RightTrigger;
        public short ThumbLX, ThumbLY, ThumbRX, ThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct XINPUT_STATE
    {
        public uint PacketNumber;
        public XINPUT_GAMEPAD Gamepad;
    }

    [DllImport("xinput1_4.dll")]
    static extern int XInputGetState(int userIndex, out XINPUT_STATE state);
}
