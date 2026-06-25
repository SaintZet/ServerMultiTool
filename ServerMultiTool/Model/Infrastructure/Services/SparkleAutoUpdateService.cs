using System;
using System.Threading.Tasks;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.UI.WPF;
using ServerMultiTool.Model.Infrastructure.Interfaces;

namespace ServerMultiTool.Model.Infrastructure.Services;

public class SparkleAutoUpdateService : IAutoUpdateService
{
    private SparkleUpdater? _sparkle;
    private bool _checkOnStartup;
    private bool _isStarted;

    public SparkleAutoUpdateService(string? feedUrl, string? publicKey, bool checkOnStartup)
    {
        Configure(feedUrl ?? string.Empty, publicKey ?? string.Empty, checkOnStartup);
    }

    public void Start()
    {
        _isStarted = true;
        RestartLoopIfNeeded();
    }

    public Task CheckForUpdatesAsync()
    {
        if (_sparkle is null)
            return Task.CompletedTask;

        _sparkle.CheckForUpdatesAtUserRequest(ignoreSkippedVersions: true);
        return Task.CompletedTask;
    }

    public Task CheckForUpdatesQuietlyAsync()
    {
        if (_sparkle is null)
            return Task.CompletedTask;

        _sparkle.CheckForUpdatesQuietly(ignoreSkippedVersions: false);
        return Task.CompletedTask;
    }

    public void Configure(string feedUrl, string publicKey, bool checkOnStartup)
    {
        _checkOnStartup = checkOnStartup;

        if (!string.IsNullOrWhiteSpace(feedUrl) && !string.IsNullOrWhiteSpace(publicKey))
        {
            InitSparkle(feedUrl, publicKey);
            RestartLoopIfNeeded();
        }
        else
        {
            _sparkle?.StopLoop();
            _sparkle?.Dispose();
            _sparkle = null;
        }
    }

    private void InitSparkle(string feedUrl, string publicKey)
    {
        _sparkle?.StopLoop();
        _sparkle?.Dispose();

        var signatureVerifier = new Ed25519Checker(
            SecurityMode.Strict,
            publicKey,
            publicKeyFile: string.Empty,
            readFileBeingVerifiedInChunks: true,
            chunkSize: 25 * 1024 * 1024);

        _sparkle = new SparkleUpdater(feedUrl, signatureVerifier)
        {
            UIFactory = new UIFactory(),
            RelaunchAfterUpdate = true
        };

    }

    private void RestartLoopIfNeeded()
    {
        if (!_isStarted || _sparkle is null)
            return;

        _sparkle.StopLoop();
        _sparkle.StartLoop(_checkOnStartup, forceInitialCheck: true, TimeSpan.FromHours(24));
    }
}


