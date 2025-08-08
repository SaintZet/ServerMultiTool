namespace ServerMultiTool.Model.Domain.Pipeline
{
    public enum OperationType
    {
        DeliveryBinOperation,
        DeliverySpecifiedFilesOperation,
        HttpPingOperation,
        ProcessExecutionOperation,
        SqlExecutionOperation,
        WebBrowserOperation,
        MsBuild,
        GitPull,
    }
}
