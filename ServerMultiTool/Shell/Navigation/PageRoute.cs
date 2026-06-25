namespace ServerMultiTool.Shell.Navigation;

public sealed record PageRoute(string Key, string Title)
{
    public override string ToString() => Key;
}

public static class AppRoutes
{
    public static readonly PageRoute Pipeline = new("Pipeline", "CI/CD Pipeline");
    public static readonly PageRoute JsonParser = new("JsonParser", "Json Parser");
    public static readonly PageRoute InternalTools = new("InternalTools", "Internal Tools");
    public static readonly PageRoute Settings = new("Settings", "Settings");
}

