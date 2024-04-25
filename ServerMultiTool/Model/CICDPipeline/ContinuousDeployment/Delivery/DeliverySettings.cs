using System;

namespace ServerMultiTool.Model.CICDPipeline.ContinuousDeployment.Delivery;

[Serializable]
public struct DeliverySettings
{
    public bool DeliveryBin { get; set; }
    public DeliveryDirectories[] DeliveryDirectory { get; set; }
}

[Serializable]
public class DeliveryDirectories
{
    public string Source { get; set; }
    public string Destination { get; set; }
}
