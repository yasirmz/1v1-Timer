using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OneVOneTimer.Input;
using InputBinding = OneVOneTimer.Input.InputBinding;

namespace OneVOneTimer.Ui;

/// <summary>
/// A read-only box (Capture's hotkey box) that takes the next key, mouse button or controller button while it has
/// focus. The input comes from the timer's own listeners (<see cref="Offer"/>), not from the window, so mouse buttons and
/// controllers work too. Esc cancels, Delete clears. While a box listens, the timer's hotkeys do nothing.
/// </summary>
public sealed class BindingBox : TextBox
{
    /// <summary>The box waiting for an input now, or null.</summary>
    public static BindingBox? Listening { get; private set; }

    public static readonly DependencyProperty BindingTextProperty = DependencyProperty.Register(
        nameof(BindingText), typeof(string), typeof(BindingBox),
        new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, _) => ((BindingBox)d).Refresh()));

    public static readonly DependencyProperty IsPadProperty = DependencyProperty.Register(
        nameof(IsPad), typeof(bool), typeof(BindingBox), new PropertyMetadata(false, (d, _) => ((BindingBox)d).Refresh()));

    /// <summary>The binding as the settings keep it ("H", "MouseBack", "DPadLeft"; "" = none).</summary>
    public string BindingText
    {
        get => (string)GetValue(BindingTextProperty);
        set => SetValue(BindingTextProperty, value);
    }

    /// <summary>A controller box (takes controller buttons) rather than a keyboard/mouse box.</summary>
    public bool IsPad
    {
        get => (bool)GetValue(IsPadProperty);
        set => SetValue(IsPadProperty, value);
    }

    public BindingBox()
    {
        IsReadOnly = true;
        IsReadOnlyCaretVisible = false;
        IsUndoEnabled = false;
        ContextMenu = null;
        FlowDirection = FlowDirection.LeftToRight;
        HorizontalContentAlignment = HorizontalAlignment.Center;
        Cursor = Cursors.Hand;
        Loaded += (_, _) => Refresh();
        Unloaded += (_, _) =>
        {
            if (Listening == this)
                Listening = null;
        };
    }

    /// <summary>
    /// An input while this box listens: Esc cancels, Delete clears, and a key or mouse button (keyboard box) or a
    /// controller button (controller box) becomes the binding. Anything else is left out.
    /// </summary>
    public void Offer(InputBinding input)
    {
        if (input.Kind == InputKind.Key && input.Code == (int)System.Windows.Forms.Keys.Escape)
        {
            Finish();
            return;
        }
        if (input.Kind == InputKind.Key && input.Code == (int)System.Windows.Forms.Keys.Delete)
        {
            BindingText = "";
            Finish();
            return;
        }
        if (IsPad != (input.Kind == InputKind.Pad))
            return;
        if (input.Kind == InputKind.Key && !BindingParser.IsBindable((System.Windows.Forms.Keys)input.Code))
            return;
        BindingText = BindingParser.Format(input);
        Finish();
    }

    /// <summary>Done listening: the focus leaves the box, which shows its binding again.</summary>
    static void Finish() => Keyboard.ClearFocus();

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        Listening = this;
        Text = App.Text.Get(IsPad ? "hotkeys.pressPad" : "hotkeys.press");
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        if (Listening == this)
            Listening = null;
        Refresh();
    }

    // The keys reach the box through Offer; the window itself does nothing with them meanwhile (no Tab, no Enter).
    protected override void OnPreviewKeyDown(KeyEventArgs e) => e.Handled = true;

    protected override void OnPreviewTextInput(TextCompositionEventArgs e) => e.Handled = true;

    void Refresh()
    {
        if (IsKeyboardFocused)
            return;
        Text = BindingParser.TryParse(BindingText, IsPad, out var binding) ? BindingParser.Display(binding) : App.Text.Get("hotkeys.none");
    }
}
