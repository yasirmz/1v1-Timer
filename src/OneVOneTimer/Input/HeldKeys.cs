namespace OneVOneTimer.Input;

/// <summary>
/// Which keys are held, so a key Windows repeats while it is held down counts once. A key whose release was never seen
/// (Windows showed a secure screen meanwhile) counts again once it has been quiet for longer than any repeat delay.
/// </summary>
public sealed class HeldKeys
{
    /// <summary>Longer than Windows' longest repeat delay (1 s): a held key repeats more often than this.</summary>
    public const long QuietMs = 1200;

    readonly Dictionary<int, long> _lastDown = [];

    /// <summary>A key went down at <paramref name="nowMs"/>; true when it is a new press, false when the key is held.</summary>
    public bool Press(int key, long nowMs)
    {
        bool held = _lastDown.TryGetValue(key, out long last) && nowMs - last < QuietMs;
        _lastDown[key] = nowMs;
        return !held;
    }

    public void Release(int key) => _lastDown.Remove(key);
}
