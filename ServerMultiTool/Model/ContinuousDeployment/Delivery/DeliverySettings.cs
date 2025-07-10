using System;
using System.Collections.Generic;

namespace ServerMultiTool.Model.ContinuousDeployment.Delivery;

[Serializable]
public class DeliverySettings
{
    public bool EnableDeliveryBin { get; set; }
    
    public bool EnableCustomDelivery { get; set; }
    public List<DeliveryDirectory>? CustomDeliveryDirectories { get; set; }
}

[Serializable]
public class DeliveryDirectory
{
    public required string Source { get; set; }
    public required string Destination { get; set; }
}