using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;

public class GitService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(GitService));

    private readonly string _solutionDirectory;

    public GitService(string solutionDirectory) => 
        _solutionDirectory = solutionDirectory;
    
    public async Task ExecuteAsync(PipelineProfile pipeline)
    {
        var settings = pipeline.GitSettings;
        if (settings.Enable is false)
        {
            Log.Info($"Git Integration is disabled by {nameof(PipelineProfile)}.");
            return;
        }

        try
        {
            var branchName = await GetCurrentBranchName();
            Log.Info($"Current Branch: {branchName}");
            
            if (settings.ShouldPull)
                await GitPull();
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to make git operations: \n{ex.Message}");
        }
    }

    public async Task<string> GetCurrentBranchName()
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = _solutionDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Arguments = "rev-parse --abbrev-ref HEAD",
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0)
            return output.TrimEnd('\n');

        var error = await process.StandardError.ReadToEndAsync();
        Log.Error($"git rev-parse --abbrev-ref HEAD: Finished with an error.\n{error}");
        throw new Exception(); //TODO
    }

    private async Task GitPull()
    {
        var startInfo = new ProcessStartInfo("git", "pull")
        {
            WorkingDirectory = _solutionDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            CreateNoWindow = true,
        };
        
        using var process = new Process { StartInfo = startInfo };
        
        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
            Log.Error($"git pull: Finished with an error.\n{output}");
        else
            Log.Info($"git pull: {output.Trim('\n')}");
    }
}