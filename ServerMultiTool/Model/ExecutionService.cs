namespace ServerMultiTool.Model;

public abstract class ExecutionService
{
    protected readonly Logger Logger;
    protected readonly ProcessExecutor ProcessExecutor;
    public ExecutionService()
    {
        Logger = new Logger(GetType());
        ProcessExecutor = new ProcessExecutor(Logger);
    }
}