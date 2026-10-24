using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OneVOneTimer.App;

/// <summary>
/// The tray's right-click menu in the app's own look (Capture's, on the logo's background): the window's near-black,
/// light text, a beige-tinted highlight and rounded corners (Windows 11), instead of Windows' white menu. A checked item
/// shows a beige tick.
/// </summary>
public static class TrayMenu
{
    internal const uint Accent = 0xD9C4A1, Background = 0x170F10;
    static readonly Color Back = Rgb(Background);
    static readonly Color Line = Rgb(0x3A2A2D); // Theme.xaml's Stroke
    static readonly Color Ink = Rgb(0xECE6D6); // TextPrimary
    static readonly Color Muted = Rgb(0xA89E98); // TextSecondary
    static readonly Color Highlight = Rgb(Mix(Background, Accent, 0.13)); // AccentSoft
    static readonly Font MenuFont = new("Segoe UI", 9.75f);
    /// <summary>The offered update, first in the menu: bold, in the accent.</summary>
    internal static readonly Font AccentFont = new("Segoe UI", 9.75f, FontStyle.Bold);

    public static ContextMenuStrip Create(bool rightToLeft)
    {
        var menu = new ContextMenuStrip
        {
            RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No,
            ShowImageMargin = false,
            ShowCheckMargin = true,
            Renderer = new Renderer(),
            BackColor = Back,
            ForeColor = Ink,
            Font = MenuFont,
            Padding = new Padding(4, 6, 4, 6),
        };
        menu.ItemAdded += (_, e) =>
        {
            if (e.Item is ToolStripMenuItem)
                e.Item.Padding = new Padding(6, 5, 10, 5);
        };
        menu.HandleCreated += (_, _) => RoundCorners(menu.Handle);
        return menu;
    }

    sealed class Renderer : ToolStripProfessionalRenderer
    {
        public Renderer() => RoundedEdges = false;

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var fill = new SolidBrush(Back);
            e.Graphics.FillRectangle(fill, e.AffectedBounds);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using var fill = new SolidBrush(Back);
            e.Graphics.FillRectangle(fill, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(Line);
            var r = e.AffectedBounds;
            e.Graphics.DrawRectangle(pen, r.X, r.Y, r.Width - 1, r.Height - 1);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Selected || !e.Item.Enabled)
                return;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // Measured from the menu's edges: items run under one side's padding (the right in English, the left in Arabic).
            int left = 5 - e.Item.Bounds.Left, right = (e.Item.Owner?.Width ?? e.Item.Width) - 5 - e.Item.Bounds.Left;
            using var path = Rounded(new RectangleF(left, 1, right - left, e.Item.Height - 2), 4);
            using var fill = new SolidBrush(Highlight);
            e.Graphics.FillPath(fill, path);
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            // A tick in the accent, in the check margin.
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var r = e.ImageRectangle;
            float s = Math.Min(r.Width, r.Height);
            float x = r.X + (r.Width - s) / 2, y = r.Y + (r.Height - s) / 2;
            using var pen = new Pen(Rgb(Accent), Math.Max(1.6f, s / 8)) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
            e.Graphics.DrawLines(pen, [new PointF(x + s * 0.2f, y + s * 0.52f), new PointF(x + s * 0.42f, y + s * 0.74f), new PointF(x + s * 0.8f, y + s * 0.3f)]);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (e.Item.ForeColor != Rgb(Accent))
                e.TextColor = e.Item.Enabled ? Ink : Muted;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using var pen = new Pen(Line);
            int y = e.Item.Height / 2;
            e.Graphics.DrawLine(pen, 10 - e.Item.Bounds.Left, y, (e.Item.Owner?.Width ?? e.Item.Width) - 10 - e.Item.Bounds.Left, y);
        }
    }

    /// <summary>Windows 11 rounds the menu and draws its edge in the theme's hairline; older Windows ignore both.</summary>
    static void RoundCorners(IntPtr hwnd)
    {
        int round = 2; // DWMWCP_ROUND
        DwmSetWindowAttribute(hwnd, 33, ref round, sizeof(int)); // DWMWA_WINDOW_CORNER_PREFERENCE
        int edge = Line.R | Line.G << 8 | Line.B << 16; // COLORREF
        DwmSetWindowAttribute(hwnd, 34, ref edge, sizeof(int)); // DWMWA_BORDER_COLOR
    }

    /// <summary>A rectangle with corners rounded to <paramref name="radius"/>.</summary>
    static GraphicsPath Rounded(RectangleF r, float radius)
    {
        var p = new GraphicsPath();
        float d = radius * 2;
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    internal static Color Rgb(uint rgb) => Color.FromArgb((int)(rgb >> 16) & 0xFF, (int)(rgb >> 8) & 0xFF, (int)rgb & 0xFF);

    static uint Mix(uint from, uint to, double amount)
    {
        uint Channel(int shift)
        {
            double a = (from >> shift) & 0xFF, b = (to >> shift) & 0xFF;
            return (uint)Math.Round(a + (b - a) * amount) << shift;
        }
        return Channel(16) | Channel(8) | Channel(0);
    }

    [DllImport("dwmapi.dll")] static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
}
