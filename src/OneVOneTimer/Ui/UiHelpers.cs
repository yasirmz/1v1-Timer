using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using OneVOneTimer.App;

namespace OneVOneTimer.Ui;

/// <summary>{l:T key} — a localized string from <see cref="Text"/>.</summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class T : MarkupExtension
{
    public T() { }
    public T(string key) => Key = key;
    public string Key { get; set; } = "";
    public override object ProvideValue(IServiceProvider serviceProvider) => Text.Get(Key);
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool on = value is true || (value is string s && s.Length > 0) || (value is int i && i != 0);
        if (parameter as string == "invert")
            on = !on;
        return on ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}

public sealed class RelayCommand(Action<object?> run, Func<object?, bool>? canRun = null) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => canRun?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => run(parameter);
}

public sealed record Option(string Label, object Value);

/// <summary>Where a window opens.</summary>
public static class WindowPlacement
{
    /// <summary>
    /// Puts <paramref name="to"/> where <paramref name="from"/> is (the window built again in another language): at its
    /// normal place and size, maximized when it is.
    /// </summary>
    public static void Carry(Window from, Window to)
    {
        var (bounds, maximized) = Carried(from.WindowState, new Rect(from.Left, from.Top, from.Width, from.Height), from.RestoreBounds);
        to.WindowStartupLocation = WindowStartupLocation.Manual;
        (to.Left, to.Top, to.Width, to.Height) = (bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        if (maximized)
            to.WindowState = WindowState.Maximized;
    }

    /// <summary>
    /// A window's normal place and size, and whether it is maximized: a maximized window's Left and Top are its normal
    /// place but its Width and Height the screen's.
    /// </summary>
    internal static (Rect Bounds, bool Maximized) Carried(WindowState state, Rect current, Rect normal) =>
        state == WindowState.Normal ? (current, false) : (normal, state == WindowState.Maximized);
}
