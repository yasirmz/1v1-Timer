using System.Reflection;
using System.Runtime.InteropServices;

namespace OneVOneTimer.Tests;

/// <summary>
/// What keeps 1v1 Timer clear of anti-cheat (Vanguard, Easy Anti-Cheat, BattlEye, VAC): it never touches a game. These
/// tests read every Windows function the program itself calls and fail when one is added that is not on the list below,
/// so a change that would open another program's memory, put code into a game, fake a key or a click, or hook input
/// can never slip in unnoticed. A new function needs a reason on this list first.
/// </summary>
public static class AntiCheatTests
{
    /// <summary>Every Windows function the timer calls, and why. None of them reads or changes another program.</summary>
    static readonly Dictionary<string, string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        // Input, read only: Windows hands every app a copy (Raw Input, XInput); the game keeps every key and button.
        ["user32.dll!RegisterRawInputDevices"] = "asks Windows for a copy of key, mouse and controller input",
        ["user32.dll!GetRawInputData"] = "reads that copy (keys and mouse buttons)",
        ["user32.dll!GetRawInputBuffer"] = "reads that copy (controller reports, in batches)",
        ["user32.dll!GetRawInputDeviceInfoW"] = "a controller's name and button layout",
        ["user32.dll!PeekMessage"] = "empties the controller thread's own message queue",
        ["hid.dll!HidP_GetCaps"] = "a controller's layout, from Windows' description of it",
        ["hid.dll!HidP_GetValueCaps"] = "where a controller's d-pad is",
        ["hid.dll!HidP_GetUsages"] = "which buttons a controller report says are held",
        ["hid.dll!HidP_GetUsageValue"] = "which way the d-pad points",
        ["xinput1_4.dll!XInputGetState"] = "an Xbox controller's buttons",
        ["user32.dll!MapVirtualKey"] = "the character a key types, to show its name",
        // The timer's own windows only.
        ["user32.dll!GetWindowLongPtrW"] = "the timer window's own style",
        ["user32.dll!SetWindowLongPtrW"] = "the timer window's own style (no focus, Clickthrough)",
        ["user32.dll!SetWindowPos"] = "moves the timer window and keeps it on top",
        ["user32.dll!GetWindowRect"] = "the timer window's own place and size",
        ["user32.dll!IsIconic"] = "whether the timer window is minimized",
        ["user32.dll!AllowSetForegroundWindow"] = "a second launch lets the running timer show its settings",
        ["dwmapi.dll!DwmSetWindowAttribute"] = "rounded corners on the timer's own tray menu",
        // Install and uninstall: the timer's own copies only, never their memory.
        ["kernel32.dll!OpenProcess"] = "the uninstaller finds 1v1 Timer's own copies (query-only access, see below)",
        ["kernel32.dll!QueryFullProcessImageName"] = "the program file of such a copy",
        ["kernel32.dll!CloseHandle"] = "closes that",
        // Timing.
        ["winmm.dll!timeBeginPeriod"] = "1 ms waits in this process, for the controller threads",
    };

    /// <summary>What anti-cheat bans for. Not one of these may ever appear in the timer's code.</summary>
    static readonly string[] Forbidden =
    [
        "ReadProcessMemory", "WriteProcessMemory", "NtReadVirtualMemory", "NtWriteVirtualMemory", "VirtualAllocEx",
        "VirtualProtectEx", "CreateRemoteThread", "NtCreateThreadEx", "QueueUserAPC", "SetThreadContext", "SuspendThread",
        "SetWindowsHookEx", "SendInput", "keybd_event", "mouse_event", "BlockInput", "DeviceIoControl", "CreateService",
        "NtLoadDriver", "DebugActiveProcess", "PROCESS_VM_READ", "PROCESS_VM_WRITE", "PROCESS_ALL_ACCESS",
    ];

    static IEnumerable<(string Name, MethodInfo Method)> NativeCalls() =>
        typeof(AppInfo).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(m => m.Attributes.HasFlag(MethodAttributes.PinvokeImpl))
            .Select(m =>
            {
                var import = m.GetCustomAttribute<DllImportAttribute>();
                string dll = Path.GetFileName(import?.Value ?? "?");
                if (!dll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    dll += ".dll";
                return ($"{dll}!{import?.EntryPoint ?? m.Name}", m);
            });

    [Test]
    public static void Every_windows_function_the_timer_calls_is_on_the_safe_list()
    {
        var calls = NativeCalls().ToList();
        Assert.True(calls.Count > 10, $"the program's native calls were found ({calls.Count})");
        var unknown = calls.Where(c => !Allowed.ContainsKey(c.Name)).Select(c => $"{c.Name} ({c.Method.DeclaringType?.Name})").Distinct().ToList();
        Assert.True(unknown.Count == 0, "not on the anti-cheat safe list (say why it is safe there first): " + string.Join(", ", unknown));
    }

    [Test]
    public static void No_function_for_touching_a_game_or_faking_input_is_ever_called()
    {
        var used = NativeCalls().Select(c => c.Name.Split('!')[1]).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var bad = Forbidden.Where(f => used.Any(u => u.StartsWith(f, StringComparison.OrdinalIgnoreCase))).ToList();
        Assert.True(bad.Count == 0, "the timer calls what anti-cheat bans for: " + string.Join(", ", bad));
    }

    [Test]
    public static void The_source_never_names_those_functions_either()
    {
        // Also catches a call made another way than [DllImport] (a function pointer, a library loaded by hand).
        string src = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "src"));
        if (!Directory.Exists(src))
            src = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src"));
        Assert.True(Directory.Exists(Path.Combine(src, "OneVOneTimer")), $"the source is at {src}");
        var hits = new List<string>();
        foreach (string file in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories).Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")))
        {
            string text = File.ReadAllText(file);
            hits.AddRange(Forbidden.Where(f => text.Contains(f, StringComparison.OrdinalIgnoreCase)).Select(f => $"{Path.GetFileName(file)}: {f}"));
            if (text.Contains("LoadLibrary", StringComparison.Ordinal) || text.Contains("GetProcAddress", StringComparison.Ordinal))
                hits.Add($"{Path.GetFileName(file)}: loads a library by hand");
        }
        Assert.True(hits.Count == 0, "found in the source: " + string.Join(", ", hits));
    }

    [Test]
    public static void Other_programs_are_only_ever_asked_their_name()
    {
        // OpenProcess appears once: the uninstaller asks 1v1 Timer's own copies for their program file, with
        // PROCESS_QUERY_LIMITED_INFORMATION (0x1000), which cannot read or change a program's memory.
        string src = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "src"));
        if (!Directory.Exists(src))
            src = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src"));
        var calls = Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
            .SelectMany(f => File.ReadLines(f).Select(l => (File: Path.GetFileName(f), Line: l.Trim())))
            .Where(l => l.Line.Contains("OpenProcess(", StringComparison.Ordinal) && !l.Line.Contains("extern", StringComparison.Ordinal))
            .ToList();
        Assert.Equal(1, calls.Count, "OpenProcess calls");
        Assert.True(calls.All(c => c.Line.Contains("0x1000", StringComparison.Ordinal)), "query-only access: " + string.Join("; ", calls.Select(c => c.Line)));
    }
}
