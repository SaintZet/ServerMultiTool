using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace ServerMultiTool.Model;

public class ProcessOutput
{
    public ProcessOutput()
    {
        Success = false;
        Output = string.Empty;
    }
    
    public ProcessOutput(int exitCode, string output)
    {
        Success = exitCode is 0;
        Output = output;
    }
    
    public ProcessOutput(int exitCode, string output, string error)
    {
        Success = exitCode is 0;
        Output = output + error;
    }

    public bool Success { get; }
    public string Output { get; }
}

public class ProcessExecutor
{
    
    private readonly Logger _logger;

    public ProcessExecutor(Logger logger) => 
        _logger = logger;

    public async Task<ProcessOutput> StartProcessOnceAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken = default) => 
        await StartProcessWithRetriesAsync(startInfo,  1, cancellationToken);

    public async Task<ProcessOutput> StartProcessWithRetriesAsync(ProcessStartInfo startInfo, int retryCount, CancellationToken cancellationToken = default)
    {

        if (startInfo.WorkingDirectory.IsNullOrEmpty())
            TryToSetWorkingDirectory(startInfo);
                
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        var baseMessage = $"Process {startInfo.FileName} {startInfo.Arguments}";
        
        var startMessage = $"{baseMessage} has started.";
        var successMessage = $"{baseMessage} has completed successfully.";
        var errorMessage = $"{baseMessage} has failed.";
        
        var messageDetails = string.Empty;

        _logger.LogInfo(startMessage);
        
        for (var retryNumber = 1; retryNumber <= retryCount; retryNumber++)
        {
            var response = await RunProcessAndGetOutputAsync(startInfo, cancellationToken);
            messageDetails = response.Output;
            
            if (response.Success is not true)
            {
                _logger.LogWarn($"{errorMessage} Retry {retryNumber} of {retryCount}.", messageDetails);
                continue;
            }

            // if (messageDetails.IsNullOrEmpty() is false)
            //     foreach (var line in messageDetails.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            //         _logger.LogInfo(line.TrimEnd('\n'));

            _logger.LogInfo(successMessage, messageDetails);
            return response;
        }

        _logger.LogError(errorMessage, messageDetails);
        
        return new ProcessOutput();
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
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            return new ProcessOutput(process.ExitCode, output, error);
        }
        catch (Exception ex)
        {
            return new ProcessOutput(-1, $"Error: {ex.Message}");
        }
    }

}