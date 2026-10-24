using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OneVOneTimer.App;

/// <summary>1v1-timer-icon.ico, built into the program: the windows, the taskbar and the tray (the .exe carries it too).</summary>
public static class AppIcon
{
    static readonly Uri IcoUri = new("pack://application:,,,/1v1Timer;component/app.ico");

    /// <summary>For a WPF window's title bar and taskbar button (Windows picks the frame for each size).</summary>
    public static ImageSource Window() => BitmapFrame.Create(IcoUri);

    /// <summary>The frame nearest <paramref name="size"/> (the tray's small icon size for the screen's scale).</summary>
    public static System.Drawing.Icon Tray(System.Drawing.Size size)
    {
        using var stream = System.Windows.Application.GetResourceStream(IcoUri)!.Stream;
        return new System.Drawing.Icon(stream, size);
    }
}
