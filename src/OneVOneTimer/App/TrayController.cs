using System.Windows.Forms;

namespace OneVOneTimer.App;

/// <summary>
/// The tray icon (1v1-timer-icon.ico) and its menu: Settings, Clickthrough on/off and Exit, with "Update to …" first
/// when there is one. A double-click opens the settings. The timer's right-click shows the same menu.
/// </summary>
public sealed class TrayController : IDisposable
{
    readonly NotifyIcon _icon = new();
    string? _menuKey;

    public event Action? SettingsClicked, ExitClicked, UpdateClicked, ClickThroughClicked;

    public TrayController()
    {
        _icon.Icon = AppIcon.Tray(SystemInformation.SmallIconSize);
        _icon.Text = AppInfo.Name;
        _icon.Visible = true;
        _icon.DoubleClick += (_, _) => SettingsClicked?.Invoke();
    }

    /// <summary>Everything the menu's items depend on.</summary>
    public static string MenuKey(bool clickThrough, string? update, string language) => $"{clickThrough}|{update}|{language}";

    /// <summary>Rebuilds the menu only when its items change (so an open menu stays open).</summary>
    /// <param name="update">A newer version offered; null when there is none.</param>
    public void Update(bool clickThrough, string? update)
    {
        string key = MenuKey(clickThrough, update, Text.Language);
        if (key == _menuKey)
            return;
        _menuKey = key;

        var menu = TrayMenu.Create(Text.IsRightToLeft);
        if (update != null)
        {
            menu.Items.Add(new ToolStripMenuItem(Text.Format("tray.update", update), null, (_, _) => UpdateClicked?.Invoke())
            {
                ForeColor = TrayMenu.Rgb(TrayMenu.Accent),
                Font = TrayMenu.AccentFont,
            });
            menu.Items.Add(new ToolStripSeparator());
        }
        menu.Items.Add(new ToolStripMenuItem(Text.Get("tray.settings"), null, (_, _) => SettingsClicked?.Invoke()));
        menu.Items.Add(new ToolStripMenuItem(Text.Get("tray.clickthrough"), null, (_, _) => ClickThroughClicked?.Invoke()) { Checked = clickThrough });
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem(Text.Get("tray.exit"), null, (_, _) => ExitClicked?.Invoke()));
        var old = _icon.ContextMenuStrip;
        _icon.ContextMenuStrip = menu;
        old?.Dispose();
    }

    /// <summary>Whether the menu is open (the timer is not raised over it meanwhile).</summary>
    public bool MenuOpen => _icon.ContextMenuStrip?.Visible == true;

    /// <summary>The menu at the mouse pointer (the timer was right-clicked).</summary>
    public void ShowMenu() => _icon.ContextMenuStrip?.Show(Cursor.Position);

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.ContextMenuStrip?.Dispose();
        _icon.Dispose();
    }
}
