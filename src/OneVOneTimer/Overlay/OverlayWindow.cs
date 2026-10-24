using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace OneVOneTimer.Overlay;

/// <summary>
/// The timer window: borderless, always on top, with a taskbar button, and never the foreground window, so pressing,
/// dragging or resizing it never takes the focus from the game. Dragging the middle moves it; an edge or a corner
/// resizes it, keeping its shape (<see cref="OverlayLayout"/>). Placed and sized in screen pixels.
/// </summary>
public sealed class OverlayWindow : Window
{
    const int GWL_STYLE = -16, GWL_EXSTYLE = -20;
    const nint WS_MAXIMIZEBOX = 0x00010000;
    const nint WS_EX_TRANSPARENT = 0x20, WS_EX_TOOLWINDOW = 0x80, WS_EX_APPWINDOW = 0x40000, WS_EX_NOACTIVATE = 0x08000000;
    const int WM_MOUSEACTIVATE = 0x0021, WM_GETMINMAXINFO = 0x0024, WM_WINDOWPOSCHANGING = 0x0046, WM_CONTEXTMENU = 0x007B,
        WM_NCHITTEST = 0x0084, WM_NCLBUTTONDBLCLK = 0x00A3, WM_NCRBUTTONDOWN = 0x00A4, WM_NCRBUTTONUP = 0x00A5,
        WM_SYSCOMMAND = 0x0112, WM_SIZING = 0x0214, WM_ENTERSIZEMOVE = 0x0231, WM_EXITSIZEMOVE = 0x0232;
    const int SC_MAXIMIZE = 0xF030, MA_NOACTIVATE = 3;
    const uint SWP_NOSIZE = 0x1, SWP_NOMOVE = 0x2, SWP_NOZORDER = 0x4, SWP_NOACTIVATE = 0x10;
    static readonly IntPtr HWND_TOPMOST = new(-1);
    /// <summary>How close to an edge (in pixels at 100%) the mouse resizes instead of moving.</summary>
    const double EdgeBand = 7;

    IntPtr _hwnd;
    bool _clickThrough;
    PixelRect? _pending;

    public OverlayFace Face { get; } = new();

    /// <summary>The user moved or resized the window (the mouse let go): the place to save.</summary>
    public event Action? Placed;

    /// <summary>The window was right-clicked: the tray's menu shows there.</summary>
    public event Action? MenuWanted;

    public OverlayWindow()
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        ResizeMode = ResizeMode.CanResize; // a sizing frame for the edges, drawn nowhere
        ShowInTaskbar = true;
        ShowActivated = false;
        Topmost = true;
        Title = AppInfo.Name;
        Icon = App.AppIcon.Window();
        WindowStartupLocation = WindowStartupLocation.Manual;
        (Left, Top, Width, Height) = (-32000, -32000, 200, 48); // placed in pixels once it has a handle
        UseLayoutRounding = true;
        TextOptions.SetTextFormattingMode(this, TextFormattingMode.Ideal);
        TextOptions.SetTextRenderingMode(this, TextRenderingMode.Grayscale);
        Content = Face;
    }

    /// <summary>Being moved or resized right now (it is left alone meanwhile).</summary>
    public bool Moving { get; private set; }

    /// <summary>The window's width over its height for the numbers shown now.</summary>
    public double Aspect => OverlayLayout.Aspect(Face.BoxEm);

    public PixelRect Bounds
    {
        get
        {
            if (_hwnd == IntPtr.Zero)
                return _pending ?? default;
            GetWindowRect(_hwnd, out var r);
            return new PixelRect(r.Left, r.Top, r.Right, r.Bottom);
        }
    }

    /// <summary>Clicks go through the window to the game (it cannot be moved, resized or right-clicked meanwhile).</summary>
    public bool ClickThrough
    {
        get => _clickThrough;
        set
        {
            _clickThrough = value;
            if (_hwnd != IntPtr.Zero)
                SetExStyle();
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        _hwnd = new WindowInteropHelper(this).Handle;
        SetExStyle();
        SetWindowLongPtr(_hwnd, GWL_STYLE, GetWindowLongPtr(_hwnd, GWL_STYLE) & ~WS_MAXIMIZEBOX);
        HwndSource.FromHwnd(_hwnd).AddHook(Hook);
        if (_pending is { } place)
            Place(place);
    }

    /// <summary>
    /// A taskbar button (APPWINDOW, never TOOLWINDOW) on a window that is never made the foreground window (NOACTIVATE),
    /// and with Clickthrough, invisible to the mouse (TRANSPARENT).
    /// </summary>
    void SetExStyle()
    {
        nint ex = (GetWindowLongPtr(_hwnd, GWL_EXSTYLE) | WS_EX_NOACTIVATE | WS_EX_APPWINDOW) & ~WS_EX_TOOLWINDOW;
        ex = _clickThrough ? ex | WS_EX_TRANSPARENT : ex & ~WS_EX_TRANSPARENT;
        SetWindowLongPtr(_hwnd, GWL_EXSTYLE, ex);
    }

    /// <summary>Puts the window at <paramref name="rect"/> (screen pixels).</summary>
    public void Place(PixelRect rect)
    {
        _pending = rect;
        if (_hwnd != IntPtr.Zero)
            SetWindowPos(_hwnd, IntPtr.Zero, rect.Left, rect.Top, rect.Width, rect.Height, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    /// <summary>The same height, the width the numbers need now (a timer past 9:59.99 takes more room).</summary>
    public void Reshape()
    {
        var r = Bounds;
        int width = OverlayLayout.WidthFor(r.Height, Aspect);
        if (Moving || r.Height <= 0 || width == r.Width)
            return;
        Place(new PixelRect(r.Left, r.Top, r.Left + width, r.Bottom));
    }

    /// <summary>Back on top of other always-on-top windows (a borderless game can cover it), never activated.</summary>
    public void KeepOnTop()
    {
        if (_hwnd != IntPtr.Zero && !Moving)
            SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (msg)
        {
            case WM_MOUSEACTIVATE:
                handled = true;
                return MA_NOACTIVATE;
            case WM_NCHITTEST:
            {
                GetWindowRect(hwnd, out var r);
                long point = lParam.ToInt64();
                int x = (short)(point & 0xFFFF) - r.Left, y = (short)((point >> 16) & 0xFFFF) - r.Top;
                int band = (int)Math.Round(EdgeBand * VisualTreeHelper.GetDpi(this).DpiScaleX);
                handled = true;
                return OverlayLayout.HitTest(x, y, r.Right - r.Left, r.Bottom - r.Top, band);
            }
            case WM_SIZING:
            {
                var proposed = Marshal.PtrToStructure<RECT>(lParam);
                GetWindowRect(hwnd, out var current);
                var fit = OverlayLayout.Sizing(proposed.Pixels, current.Pixels, (int)wParam, Aspect);
                Marshal.StructureToPtr(new RECT(fit), lParam, false);
                handled = true;
                return 1;
            }
            case WM_WINDOWPOSCHANGING:
            {
                // Any other size change (a snap to the screen's side, another monitor's scale) keeps the shape too.
                var pos = Marshal.PtrToStructure<WINDOWPOS>(lParam);
                if ((pos.Flags & SWP_NOSIZE) == 0 && pos.Cy > 0 && !IsIconic(hwnd) && pos.X > -32000)
                {
                    pos.Cy = OverlayLayout.ClampHeight(pos.Cy);
                    pos.Cx = OverlayLayout.WidthFor(pos.Cy, Aspect);
                    Marshal.StructureToPtr(pos, lParam, false);
                }
                break;
            }
            case WM_GETMINMAXINFO:
            {
                var info = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                info.MinTrackSize = new POINT(OverlayLayout.WidthFor(OverlayLayout.MinHeight, Aspect), OverlayLayout.MinHeight);
                info.MaxTrackSize = new POINT(OverlayLayout.WidthFor(OverlayLayout.MaxHeight, Aspect), OverlayLayout.MaxHeight);
                Marshal.StructureToPtr(info, lParam, false);
                handled = true;
                return IntPtr.Zero;
            }
            case WM_ENTERSIZEMOVE:
                Moving = true;
                break;
            case WM_EXITSIZEMOVE:
                Moving = false;
                Placed?.Invoke();
                break;
            case WM_NCLBUTTONDBLCLK:
            case WM_NCRBUTTONDOWN:
            case WM_CONTEXTMENU:
                handled = true; // never maximized, never Windows' own window menu
                break;
            case WM_NCRBUTTONUP:
                handled = true;
                MenuWanted?.Invoke();
                break;
            case WM_SYSCOMMAND when ((int)wParam & 0xFFF0) == SC_MAXIMIZE:
                handled = true;
                break;
        }
        return IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int Left, Top, Right, Bottom;
        public RECT(PixelRect r) => (Left, Top, Right, Bottom) = (r.Left, r.Top, r.Right, r.Bottom);
        public readonly PixelRect Pixels => new(Left, Top, Right, Bottom);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int X, Y;
        public POINT(int x, int y) => (X, Y) = (x, y);
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MINMAXINFO
    {
        public POINT Reserved, MaxSize, MaxPosition, MinTrackSize, MaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WINDOWPOS
    {
        public IntPtr Hwnd, InsertAfter;
        public int X, Y, Cx, Cy;
        public uint Flags;
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    static extern nint GetWindowLongPtr(IntPtr hwnd, int index);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    static extern nint SetWindowLongPtr(IntPtr hwnd, int index, nint value);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hwnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

    [DllImport("user32.dll")]
    static extern bool IsIconic(IntPtr hwnd);
}
