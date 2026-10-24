using System.Globalization;
using System.Windows;
using System.Windows.Media;
using OneVOneTimer.Timing;

namespace OneVOneTimer.Overlay;

/// <summary>
/// The two timers' numbers, drawn to fit the window: the font size follows the window's height. Each timer's text is
/// placed by its pattern (<see cref="TimeFormat.Pattern"/>), so the digits keep still while the time runs.
/// </summary>
public sealed class OverlayFace : FrameworkElement
{
    static readonly Typeface Face = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
    const double MeasureSize = 100; // measurements are made at this size, then scaled

    readonly Dictionary<string, double> _widths = [];
    /// <summary>The outline's width in font sizes (the transparent background), drawn half outside the digits.</summary>
    const double OutlineEm = 0.09;

    readonly string[] _text = ["0.00", "0.00"];
    readonly Brush[] _fill = [Brushes.White, Brushes.White];
    Brush? _outline;
    readonly double _digitTop, _digitHeight;

    public OverlayFace()
    {
        var zero = Formatted("0", MeasureSize, Brushes.White, 1).BuildGeometry(new Point()).Bounds;
        _digitTop = zero.Top / MeasureSize;
        _digitHeight = zero.Height / MeasureSize;
        BoxEm = OverlayLayout.BoxEm(WidthEm(TimeFormat.Reserved));
    }

    /// <summary>A timer box's width in font sizes (both boxes are as wide): grows past 9:59.99.</summary>
    public double BoxEm { get; private set; }

    /// <summary>
    /// Shows these texts and colors; <paramref name="outline"/> draws a thin edge around each number (the transparent
    /// background). True when the boxes got wider (the window must change shape).
    /// </summary>
    public bool Show(string timer1, Brush fill1, string timer2, Brush fill2, Brush? outline)
    {
        bool changed = timer1 != _text[0] || timer2 != _text[1] || !ReferenceEquals(fill1, _fill[0]) || !ReferenceEquals(fill2, _fill[1])
            || !ReferenceEquals(outline, _outline);
        (_text[0], _text[1], _fill[0], _fill[1], _outline) = (timer1, timer2, fill1, fill2, outline);
        double box = OverlayLayout.BoxEm(Math.Max(WidthEm(TimeFormat.Reserved), Math.Max(WidthEm(TimeFormat.Pattern(timer1)), WidthEm(TimeFormat.Pattern(timer2)))));
        bool wider = Math.Abs(box - BoxEm) > 1e-6;
        BoxEm = box;
        if (changed || wider)
            InvalidateVisual();
        return wider;
    }

    protected override void OnRender(DrawingContext dc)
    {
        double height = ActualHeight, width = ActualWidth;
        if (height <= 0 || width <= 0)
            return;
        double em = height / OverlayLayout.HeightEm, box = width / 2;
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        double y = height / 2 - (_digitTop + _digitHeight / 2) * em;
        for (int i = 0; i < 2; i++)
        {
            var text = Formatted(_text[i], em, _fill[i], pixelsPerDip);
            double x = i * box + (box - WidthEm(TimeFormat.Pattern(_text[i])) * em) / 2;
            if (_outline == null)
            {
                dc.DrawText(text, new Point(x, y));
                continue;
            }
            var shape = text.BuildGeometry(new Point(x, y));
            dc.DrawGeometry(null, new Pen(_outline, OutlineEm * em) { LineJoin = PenLineJoin.Round }, shape);
            dc.DrawGeometry(_fill[i], null, shape);
        }
    }

    /// <summary>A text's width in font sizes (all digits are one width in Segoe UI, so a pattern stands for its texts).</summary>
    double WidthEm(string text)
    {
        if (!_widths.TryGetValue(text, out double w))
            _widths[text] = w = Formatted(text, MeasureSize, Brushes.White, 1).WidthIncludingTrailingWhitespace / MeasureSize;
        return w;
    }

    static FormattedText Formatted(string text, double size, Brush brush, double pixelsPerDip) =>
        new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Face, size, brush, pixelsPerDip);
}
