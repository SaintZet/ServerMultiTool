using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Models.Integrations.Git.Contracts;
using ServerMultiTool.Models.Integrations.Git.Data;
using ServerMultiTool.Models.Settings.Global.Contracts;

namespace ServerMultiTool.Models.Integrations.Git.Services;

public class GitIntegrationService : IGitIntegrationService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(GitIntegrationService));
    
    private readonly IGlobalSettingsService _globalSettingsService;
    
    public GitIntegrationService(IGlobalSettingsService globalSettingsService) => 
        _globalSettingsService = globalSettingsService;

    public async Task ExecuteGitOperationsAsync(GitIntegrationSettings settings)
    {
        if (settings.Enable is false)
            return;
        
        Log.Info($"Current Branch: {await GetCurrentBranchName()}");
        
        if (settings.ShouldPull)
            await GitPull();
    }
    
    public async Task<string> GetCurrentBranchName()
    {
        var startInfo = new ProcessStartInfo("git")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Arguments = "rev-parse --abbrev-ref HEAD",
            CreateNoWindow = true,
            WorkingDirectory = _globalSettingsService.GlobalSettings.SolutionDirectory,
        };

        using var process = new Process { StartInfo = startInfo };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
            return output.TrimEnd('\n');

        var error = await process.StandardError.ReadToEndAsync();
        Log.Error($"Ошибка при получении текущей ветки: {error}");
        throw new Exception(); //TODO
    }

    private async Task GitPull()
    {
        var startInfo = new ProcessStartInfo("git", "pull")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            CreateNoWindow = true,
            WorkingDirectory = _globalSettingsService.GlobalSettings.SolutionDirectory,
        };
        
        using var process = new Process { StartInfo = startInfo };
        
        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        if (process.ExitCode is not 0)
            Log.Error($"Git Pull завершился с ошибкой.\n{output}");
        else
            Log.Info($"Git Pull: {output.Trim('\n')}");
    }
}