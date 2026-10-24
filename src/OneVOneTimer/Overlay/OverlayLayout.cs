namespace OneVOneTimer.Overlay;

/// <summary>A rectangle on the screen in pixels, as Windows gives it (right and bottom just outside).</summary>
public readonly record struct PixelRect(int Left, int Top, int Right, int Bottom)
{
    public int Width => Right - Left;
    public int Height => Bottom - Top;
}

/// <summary>
/// The timer window's shape: two boxes side by side whose numbers grow and shrink with the window. Its height sets the
/// size of the numbers and its width follows, so dragging any edge or corner keeps the shape. All in screen pixels.
/// </summary>
public static class OverlayLayout
{
    /// <summary>The window's height in font sizes: room above and below the digits.</summary>
    public const double HeightEm = 1.32;
    /// <summary>Room on each side of a timer's text, in font sizes.</summary>
    public const double SideEm = 0.3;
    public const int MinHeight = 24, MaxHeight = 1000;
    /// <summary>A new timer's height on a screen at 100%.</summary>
    public const int DefaultHeight = 48;

    // Windows' WM_SIZING edges and WM_NCHITTEST answers.
    public const int WmszLeft = 1, WmszRight = 2, WmszTop = 3, WmszTopLeft = 4, WmszTopRight = 5, WmszBottom = 6, WmszBottomLeft = 7, WmszBottomRight = 8;
    public const int HtCaption = 2, HtLeft = 10, HtRight = 11, HtTop = 12, HtTopLeft = 13, HtTopRight = 14, HtBottom = 15, HtBottomLeft = 16, HtBottomRight = 17;

    public static int ClampHeight(int height) => Math.Clamp(height, MinHeight, MaxHeight);

    /// <summary>A box's width in font sizes: its text's room (never less than 9:59.99's) and the sides.</summary>
    public static double BoxEm(double textEm) => textEm + 2 * SideEm;

    /// <summary>The whole window's width over its height.</summary>
    public static double Aspect(double boxEm) => 2 * boxEm / HeightEm;

    public static int WidthFor(int height, double aspect) => Math.Max(1, (int)Math.Round(height * aspect));

    /// <summary>
    /// What Windows proposes while an edge or corner is dragged (<paramref name="proposed"/>), kept to the window's shape
    /// and size limits: a side edge sets the width, the top or bottom the height, and a corner whichever was pulled
    /// further from <paramref name="current"/>. The side or corner opposite the one dragged stays where it is.
    /// </summary>
    public static PixelRect Sizing(PixelRect proposed, PixelRect current, int edge, double aspect)
    {
        double byWidth = proposed.Width / aspect;
        int height = edge switch
        {
            WmszLeft or WmszRight => (int)Math.Round(byWidth),
            WmszTop or WmszBottom => proposed.Height,
            _ => Math.Abs(proposed.Width - current.Width) / (double)Math.Max(1, current.Width)
                 >= Math.Abs(proposed.Height - current.Height) / (double)Math.Max(1, current.Height)
                ? (int)Math.Round(byWidth)
                : proposed.Height,
        };
        height = ClampHeight(height);
        int width = WidthFor(height, aspect);
        bool leftMoves = edge is WmszLeft or WmszTopLeft or WmszBottomLeft;
        bool topMoves = edge is WmszTop or WmszTopLeft or WmszTopRight;
        int left = leftMoves ? proposed.Right - width : proposed.Left;
        int top = topMoves ? proposed.Bottom - height : proposed.Top;
        return new PixelRect(left, top, left + width, top + height);
    }

    /// <summary>
    /// What the mouse is over at (<paramref name="x"/>, <paramref name="y"/>) inside a <paramref name="width"/> ×
    /// <paramref name="height"/> window: a corner or an edge (resize) within <paramref name="band"/> pixels of it,
    /// otherwise the middle, which moves the window.
    /// </summary>
    public static int HitTest(int x, int y, int width, int height, int band)
    {
        band = Math.Max(1, Math.Min(band, Math.Min(width, height) / 4));
        int corner = band * 2;
        bool left = x < band, right = x >= width - band, top = y < band, bottom = y >= height - band;
        bool nearLeft = x < corner, nearRight = x >= width - corner, nearTop = y < corner, nearBottom = y >= height - corner;
        if ((top && nearLeft) || (left && nearTop)) return HtTopLeft;
        if ((top && nearRight) || (right && nearTop)) return HtTopRight;
        if ((bottom && nearLeft) || (left && nearBottom)) return HtBottomLeft;
        if ((bottom && nearRight) || (right && nearBottom)) return HtBottomRight;
        if (left) return HtLeft;
        if (right) return HtRight;
        if (top) return HtTop;
        if (bottom) return HtBottom;
        return HtCaption;
    }
}
