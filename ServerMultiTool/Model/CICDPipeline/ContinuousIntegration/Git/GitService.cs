using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ServerMultiTool.Model.CICDPipeline.PipelineProfiles;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousIntegration.Git;

public class GitService : ExecutionService
{
    public string SolutionDirectory;

    public GitService(string solutionDirectory) => 
        SolutionDirectory = solutionDirectory;

    public async Task<bool> ExecuteAsync(PipelineProfile pipeline)
    {
        var settings = pipeline.GitSettings;
        if (settings.Enable is false)
        {
            Logger.LogInfo($"Git Integration is disabled by {nameof(PipelineProfile)}.");
            return true;
        }

        try
        {
            var branchName = await GetCurrentBranchName();
            Logger.LogInfo($"Current Branch: {branchName}");
            
            if (settings.ShouldPull)
                await GitPull();
            
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to make git operations: \n{ex.Message}");
            return false;
        }
    }

    public async Task<string> GetCurrentBranchName()
    {
        const string fileName = "git";
        const string arguments = "rev-parse --abbrev-ref HEAD";
        
        var info = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = SolutionDirectory };
        
        var response = await ProcessExecutor.StartProcessOnceAsync(info);
        
        return response.Output.TrimEnd('\n'); //TODO formatting inside class
    }

    private async Task<bool> GitPull()
    {
        const string fileName = "git";
        const string arguments = "rev-parse --abbrev-ref HEAD";
        
        var info = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = SolutionDirectory };
        var response = await ProcessExecutor.StartProcessOnceAsync(info);
        
        return response.Success;
    }
}