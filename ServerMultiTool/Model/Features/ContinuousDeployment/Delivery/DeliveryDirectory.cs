using System;

namespace ServerMultiTool.Model.Features.ContinuousDeployment.Delivery;

[Serializable]
public class DeliveryDirectory
{
    public required string Source { get; set; }
    public required string Destination { get; set; }
}