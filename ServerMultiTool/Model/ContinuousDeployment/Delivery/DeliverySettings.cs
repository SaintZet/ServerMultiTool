using System;

namespace ServerMultiTool.Model.ContinuousDeployment.Delivery;

[Serializable]
public class DeliverySettings
{
    public bool Enable { get; set; }
    public bool DeliveryBin { get; set; }
    public DeliveryDirectories[] DeliveryDirectory { get; set; }
}

[Serializable]
public class DeliveryDirectories
{
    public string Source { get; set; }
    public string Destination { get; set; }
}