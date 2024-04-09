using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Models.Build.Contracts;
using ServerMultiTool.Models.Build.Data;

namespace ServerMultiTool.Models.Build.Services;

public class BuildService : IBuildService
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(BuildService));
    
    public async Task ExecuteBuildAsync(BuildSettings settings)
    {
        if (settings.Enable is false)
            return;
        
        var buildTasks = settings.BuildSettingsPerProject
            .Where(project => project.ShouldBuild)
            .Select(ExecuteMsBuildAsync)
            .ToList();

        await Task.WhenAll(buildTasks);
    }

    private static async Task ExecuteMsBuildAsync(ProjectBuildSettings settings)
    {
        var dotnetCommand = GetMsBuildCommand(settings.ProjectPath, settings.Parameters);
        Log.Info(dotnetCommand);
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = dotnetCommand,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = false,
            CreateNoWindow = true
        };
        
        using var process = new Process { StartInfo = startInfo };
        
        process.Start();
        
        var output = await process.StandardOutput.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        if (process.ExitCode is not 0)
            Log.Error($"Сборка проекта {settings.ProjectName} завершилась с ошибкой.\n{output}");
        else
            Log.Info($"Сборка проекта {settings.ProjectName} успешно завершена.");
    }

    private static string GetMsBuildCommand(string projectPath, List<string> parameters)
    {
        var dotnetCommandBuilder = new StringBuilder("msbuild ")
            .Append($"\"{projectPath}\" ");

        foreach (var parameter in parameters) 
            dotnetCommandBuilder.Append($" /p:{parameter} ");
        
        return dotnetCommandBuilder.ToString();
    }
}