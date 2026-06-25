namespace ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums
{
    public enum OperationType
    {
        // File delivery operations
        DeliveryBinOperation = 1,
        DeliverySpecifiedFilesOperation = 2,

        // Network operations
        HttpPingOperation = 3,
        WebBrowserOperation = 6,

        // Execution operations
        ProcessExecutionOperation = 4,
        SqlExecutionOperation = 5,

        // Source control and build operations
        MsBuildOperation = 7,
        GitPullOperation = 8,
    }
}
