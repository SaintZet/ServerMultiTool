using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ServerMultiTool.Model.Common.Logs;

namespace ServerMultiTool.Model.Common.ProcessExecutor;

public class ProcessExecutor(Logger logger)
{
    public async Task<ProcessOutput> StartProcessOnceAsync(ProcessStartInfo startInfo, CancellationToken cancellationToken = default, bool publishLogs = true) =>
        await StartProcessWithRetriesAsync(startInfo, 1, cancellationToken, publishLogs);

    public async Task<ProcessOutput> StartProcessWithRetriesAsync(ProcessStartInfo startInfo, int retryCount, CancellationToken cancellationToken = default, bool publishLogs = true)
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
        var cancelMessage = $"{baseMessage} was cancelled. ";

        var messageDetails = string.Empty;

        LogInfo(startMessage, publishLogs);

        for (var retryNumber = 1; retryNumber <= retryCount; retryNumber++)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    LogInfo(cancelMessage, publishLogs);
                    throw new OperationCanceledException(cancellationToken);
                }

                response = await RunProcessAndGetOutputAsync(startInfo, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    LogInfo(cancelMessage, publishLogs);
                    throw new OperationCanceledException(cancellationToken);
                }

                messageDetails = response.Output;

                if (response.Success is not true)
                {
                    LogWarn(errorMessage + $"Retry {retryNumber} of {retryCount}.", messageDetails, publishLogs);
                    continue;
                }

                LogSuccess(successMessage, messageDetails, publishLogs);
                return response;
            }
            catch (OperationCanceledException)
            {
                LogInfo(cancelMessage, publishLogs);
                throw;
            }
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            LogError(errorMessage, messageDetails, publishLogs);
        }

        return response;
    }

    private void LogInfo(string message, bool publishLogs)
    {
        if (publishLogs)
            logger.LogInfoWithPublish(message);
        else
            logger.LogInfo(message);
    }

    private void LogWarn(string message, string? details, bool publishLogs)
    {
        if (publishLogs)
            logger.LogWarnWithPublish(message, details);
        else
            logger.LogWarn(message, details);
    }

    private void LogSuccess(string message, string? details, bool publishLogs)
    {
        if (publishLogs)
            logger.LogSuccessWithPublish(message, details);
        else
            logger.LogSuccess(message, details);
    }

    private void LogError(string message, string? details, bool publishLogs)
    {
        if (publishLogs)
            logger.LogErrorWithPublish(message, details);
        else
            logger.LogError(message, details);
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
            using var process = new Process();
            process.StartInfo = startInfo;

            if (process is null)
                throw new InvalidOperationException("Failed to start process. Process is null");

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            try
            {
                await process.WaitForExitAsync(cancellationToken);

                // Only try to get output if not cancelled
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Task.WhenAll(outputTask, errorTask);
                    return new ProcessOutput(process.ExitCode, await outputTask, await errorTask);
                }
                else
                {
                    // Try to kill the process if it's still running
                    if (!process.HasExited)
                    {
                        try { process.Kill(true); } catch { /* ignore errors during cleanup */ }
                    }
                    throw new OperationCanceledException(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Try to kill the process if it's still running
                if (!process.HasExited)
                {
                    try { process.Kill(true); } catch { /* ignore errors during cleanup */ }
                }
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ProcessOutput(-1, $"Error: {ex.Message}");
        }
    }
}
