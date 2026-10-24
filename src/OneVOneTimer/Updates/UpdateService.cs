using OneVOneTimer.App;
using Velopack;
using Velopack.Sources;

namespace OneVOneTimer.Updates;

/// <summary>
/// The timer's updates through Velopack: looks for a newer release on GitHub, and when the user presses Update, downloads
/// it and restarts into it. Failures are logged and shown as "couldn't check", never as an error window. Off in a
/// development build (not installed by the Setup).
/// </summary>
public sealed class UpdateService
{
    readonly UpdateManager? _manager;
    readonly SynchronizationContext _ui;
    UpdateInfo? _found;

    UpdateService(UpdateManager? manager, SynchronizationContext ui) => (_manager, _ui) = (manager, ui);

    /// <summary>The service for this timer: Velopack's when installed, from GitHub or a local test folder.</summary>
    /// <param name="ui">Where <see cref="Changed"/> and <see cref="Restarting"/> are raised (the UI thread).</param>
    public static UpdateService ForThisInstall(string appDataDir, SynchronizationContext ui)
    {
        if (InstallPaths.Root == null)
            return new UpdateService(null, ui);
        string? folder = UpdateRules.LocalFeed(appDataDir);
        return new UpdateService(folder != null
            ? new UpdateManager(folder)
            : new UpdateManager(new GithubSource(AppInfo.Repository, null, prerelease: AppInfo.Version.Contains('-'))), ui);
    }

    /// <summary>This install can update (the installed app, not a development build).</summary>
    public bool Works => _manager != null;

    public UpdateState State { get; private set; }

    /// <summary>The newer version found, or null.</summary>
    public string? NewVersion { get; private set; }

    public int Percent { get; private set; }

    public DateTime? LastCheck { get; private set; }

    /// <summary>Something above changed (on the thread the service was made on).</summary>
    public event Action? Changed;

    /// <summary>Velopack's updater took over: the timer must close now (it starts again in the new version).</summary>
    public event Action? Restarting;

    public async Task Check()
    {
        if (_manager == null || State is UpdateState.Checking or UpdateState.Downloading or UpdateState.Restarting)
            return;
        LastCheck = DateTime.UtcNow;
        Set(UpdateState.Checking);
        try
        {
            _found = await _manager.CheckForUpdatesAsync().ConfigureAwait(false);
            NewVersion = _found?.TargetFullRelease.Version.ToString();
            Log.Info("update check: " + (NewVersion ?? "up to date"));
            Set(NewVersion != null ? UpdateState.Available : UpdateState.UpToDate);
        }
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            Log.Warn("update check: " + e.Message);
            Set(NewVersion != null ? UpdateState.Available : UpdateState.CheckFailed);
        }
    }

    /// <summary>Downloads the version found and hands over to Velopack's updater, which restarts the timer into it.</summary>
    public async Task UpdateNow()
    {
        if (_manager == null || _found == null || State is UpdateState.Downloading or UpdateState.Restarting)
            return;
        Percent = 0;
        Set(UpdateState.Downloading);
        try
        {
            await _manager.DownloadUpdatesAsync(_found, p =>
            {
                Percent = p;
                Raise(Changed);
            }, CancellationToken.None).ConfigureAwait(false);
            Set(UpdateState.Restarting);
            _manager.WaitExitThenApplyUpdates(_found.TargetFullRelease, silent: true, restart: true, []);
            Raise(Restarting);
        }
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            Log.Warn("update: " + e.Message);
            Set(UpdateState.Failed);
        }
    }

    void Set(UpdateState state)
    {
        State = state;
        Raise(Changed);
    }

    void Raise(Action? handler)
    {
        if (handler != null)
            _ui.Post(_ => handler(), null);
    }
}
