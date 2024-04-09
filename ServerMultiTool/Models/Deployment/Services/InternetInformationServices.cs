using System;
using System.Diagnostics;
using System.Threading.Tasks;
using log4net;
using ServerMultiTool.Models.Deployment.Contracts;

namespace ServerMultiTool.Models.Deployment.Services;

public class InternetInformationServices : IInternetInformationServices
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(InternetInformationServices));
    
    public async Task StopAsync() => 
        await ExecuteCommandAsync("/stop");

    public async Task StartAsync() => 
        await ExecuteCommandAsync("/start");
    
    private static async Task ExecuteCommandAsync(string arguments)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "iisreset.exe",
            Arguments = arguments,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = new Process { StartInfo = processStartInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();
        
        var outputLines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (process.ExitCode is 0)
            foreach (var line in outputLines)
                Log.Info(line);
        else
            Log.Error(error);
    }
}