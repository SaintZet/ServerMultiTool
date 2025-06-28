using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.ContinuousDeployment.Delivery;

[Serializable]
public class DeliverySettings
{
    public bool Enable { get; set; }
    public bool DeliveryBin { get; set; }
    public List<DeliveryDirectories>? DeliveryDirectory { get; set; }
}

[Serializable]
public class DeliveryDirectories
{
    public required string Source { get; set; }
    public required string Destination { get; set; }
}