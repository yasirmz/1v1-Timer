using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OneVOneTimer.Timing;

namespace OneVOneTimer.Overlay;

/// <summary>
/// The two timers' numbers, drawn to fit the window: the font size follows the window's height. Each timer's text is
/// placed by its pattern (<see cref="TimeFormat.Pattern"/>), so the digits keep still while the time runs.
/// Light enough to run beside a game: each character (0-9, "." and ":") is drawn once per size and color, outline and
/// all, into a small picture, and a frame only places those pictures (the CPU draws, never the graphics card).
/// </summary>
public sealed class OverlayFace : FrameworkElement
{
    static readonly Typeface Face = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
    const double MeasureSize = 100; // measurements are made at this size, then scaled
    /// <summary>The outline's width in font sizes (the transparent background), drawn half outside the digits.</summary>
    const double OutlineEm = 0.09;
    /// <summary>More characters than this kept means old colors are in there too: the pictures start again.</summary>
    const int MaxGlyphs = 64;

    readonly Dictionary<char, double> _advances = [];
    readonly Dictionary<(char, Brush), BitmapSource> _glyphs = [];
    double _glyphEm, _glyphScale;
    Brush? _glyphOutline;

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
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor); // the pictures are drawn at their own size
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
        double scale = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        // Another size, screen scale or outline: the pictures are drawn again (a resize, a settings change).
        if (em != _glyphEm || scale != _glyphScale || !ReferenceEquals(_outline, _glyphOutline) || _glyphs.Count > MaxGlyphs)
        {
            _glyphs.Clear();
            (_glyphEm, _glyphScale, _glyphOutline) = (em, scale, _outline);
        }
        double pad = Pad(em);
        double y = height / 2 - (_digitTop + _digitHeight / 2) * em - pad;
        for (int i = 0; i < 2; i++)
        {
            double x = i * box + (box - WidthEm(TimeFormat.Pattern(_text[i])) * em) / 2;
            foreach (char c in _text[i])
            {
                var glyph = Glyph(c, _fill[i], em, scale);
                // On whole screen pixels, so the picture is shown exactly as drawn.
                dc.DrawImage(glyph, new Rect(Snap(x - pad, scale), Snap(y, scale), glyph.PixelWidth / scale, glyph.PixelHeight / scale));
                x += Advance(c) * em;
            }
        }
    }

    /// <summary>Room around a character for its outline and any part of it past its width.</summary>
    static double Pad(double em) => OutlineEm * em + 2;

    static double Snap(double dip, double scale) => Math.Round(dip * scale) / scale;

    BitmapSource Glyph(char c, Brush fill, double em, double scale)
    {
        if (_glyphs.TryGetValue((c, fill), out var cached))
            return cached;
        var text = Formatted(c.ToString(), em, fill, scale);
        double pad = Pad(em);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            if (_outline == null)
            {
                dc.DrawText(text, new Point(pad, pad));
            }
            else
            {
                var shape = text.BuildGeometry(new Point(pad, pad));
                dc.DrawGeometry(null, new Pen(_outline, OutlineEm * em) { LineJoin = PenLineJoin.Round }, shape);
                dc.DrawGeometry(fill, null, shape);
            }
        }
        int w = (int)Math.Ceiling((text.WidthIncludingTrailingWhitespace + 2 * pad) * scale);
        int h = (int)Math.Ceiling((text.Height + 2 * pad) * scale);
        var bitmap = new RenderTargetBitmap(Math.Max(1, w), Math.Max(1, h), 96 * scale, 96 * scale, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        _glyphs[(c, fill)] = bitmap;
        return bitmap;
    }

    /// <summary>A character's width in font sizes.</summary>
    double Advance(char c)
    {
        if (!_advances.TryGetValue(c, out double w))
            _advances[c] = w = Formatted(c.ToString(), MeasureSize, Brushes.White, 1).WidthIncludingTrailingWhitespace / MeasureSize;
        return w;
    }

    /// <summary>A text's width in font sizes: its characters' widths (all digits are one width in Segoe UI, so a pattern stands for its texts).</summary>
    double WidthEm(string text)
    {
        double w = 0;
        foreach (char c in text)
            w += Advance(c);
        return w;
    }

    static FormattedText Formatted(string text, double size, Brush brush, double pixelsPerDip) =>
        new(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Face, size, brush, pixelsPerDip);
}
