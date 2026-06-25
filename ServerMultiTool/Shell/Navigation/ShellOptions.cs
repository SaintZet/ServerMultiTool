namespace ServerMultiTool.Shell.Navigation;

public sealed class ShellOptions
{
    public PageRoute StartupRoute { get; init; } = AppRoutes.Pipeline;

    // Keep string key until all call sites migrate to typed route configuration.
    public string StartupPageKey { get; init; } = AppRoutes.Pipeline.Key;
}

