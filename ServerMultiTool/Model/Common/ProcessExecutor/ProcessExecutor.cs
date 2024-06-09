using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common.Logs;

namespace ServerMultiTool.Model.Common.ProcessExecutor;

public class ProcessExecutor
{
    private readonly Logger _logger;

    public ProcessExecutor(Logger logger) => 
        _logger = logger;

    public async Task<ProcessOutput> StartProcessOnceAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken = default) => 
        await StartProcessWithRetriesAsync(startInfo,  1, cancellationToken);

    public async Task<ProcessOutput> StartProcessWithRetriesAsync(ProcessStartInfo startInfo, int retryCount, CancellationToken cancellationToken = default)
    {
        var response = new ProcessOutput();
        
        if (startInfo.WorkingDirectory.IsNullOrEmpty())
            TryToSetWorkingDirectory(startInfo);
                
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        var baseMessage = $"Process {startInfo.FileName} {startInfo.Arguments}";
        
        var startMessage = $"{baseMessage} has started. ";
        var successMessage = $"{baseMessage} has completed successfully. ";
        var errorMessage = $"{baseMessage} has failed. ";
        
        var messageDetails = string.Empty;

        _logger.LogInfoWithPublish(startMessage);
        
        for (var retryNumber = 1; retryNumber <= retryCount; retryNumber++)
        {
            response = await RunProcessAndGetOutputAsync(startInfo, cancellationToken);
            messageDetails = response.Output;
            
            if (response.Success is not true)
            {
                _logger.LogWarnWithPublish(errorMessage + $"Retry {retryNumber} of {retryCount}.", messageDetails);
                continue;
            }

            _logger.LogInfoWithPublish(successMessage, messageDetails);
            return response;
        }

        _logger.LogErrorWithPublish(errorMessage, messageDetails);
        return response;
    }

    private static void TryToSetWorkingDirectory(ProcessStartInfo startInfo)
    {
        var fileName = startInfo.FileName;
        
        if (!File.Exists(fileName)) 
            return;
        
        var workingDirectory = Path.GetDirectoryName(fileName);
        
        if (Directory.Exists(workingDirectory)) 
            startInfo.WorkingDirectory = workingDirectory;
    }

    private static async Task<ProcessOutput> RunProcessAndGetOutputAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process { StartInfo = startInfo };
            if (process is null)
                throw new InvalidOperationException("Failed to start process. Process is null");

            process.Start();
        
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            var processExitTask = process.WaitForExitAsync(cancellationToken);
            var completedTask = await Task.WhenAny(processExitTask, Task.Delay(-1, cancellationToken));

            if (completedTask == processExitTask)
            {
                if (!process.HasExited)
                    throw new InvalidOperationException("Process has not exited.");

                await Task.WhenAll(outputTask, errorTask);
            }
            else
            {
                throw new TimeoutException("Reading process output timed out.");
            }

            return new ProcessOutput(process.ExitCode, await outputTask, await errorTask);
        }
        catch (Exception ex)
        {
            return new ProcessOutput(-1, $"Error: {ex.Message}");
        }
    }
}