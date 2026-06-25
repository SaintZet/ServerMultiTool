using System.Threading.Tasks;

namespace ServerMultiTool.Model.Infrastructure.Interfaces;

public interface IAutoUpdateService
{
    /// <summary>
    /// Start background update loop. Call after main UI is shown.
    /// </summary>
    void Start();

    /// <summary>
    /// Check for updates and show the update UI if a newer version is available.
    /// </summary>
    Task CheckForUpdatesAsync();

    /// <summary>
    /// Check for updates silently (no UI unless update is found).
    /// </summary>
    Task CheckForUpdatesQuietlyAsync();

    /// <summary>
    /// Re-configure the updater with a new feed URL (called after settings save).
    /// </summary>
    void Configure(string feedUrl, string publicKey, bool checkOnStartup);
}

