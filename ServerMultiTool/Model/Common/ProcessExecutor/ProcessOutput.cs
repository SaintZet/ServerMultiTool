namespace ServerMultiTool.Model.Common.ProcessExecutor;

public class ProcessOutput
{
    public bool Success { get; }
    public string? Output { get; }

    public ProcessOutput()
        : this(false, string.Empty) { }

    public ProcessOutput(int exitCode, string output)
        : this(exitCode is 0, output) { }

    public ProcessOutput(int exitCode, string output, string error)
        : this(exitCode is 0, output + error) { }

    private ProcessOutput(bool success, string output)
    {
        Success = success;
        Output = output.Replace("\n\n", "\n").TrimEnd('\n');
    }
}
