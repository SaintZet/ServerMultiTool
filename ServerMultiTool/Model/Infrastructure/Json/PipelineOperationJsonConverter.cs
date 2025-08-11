using ServerMultiTool.Model.Features.Pipeline.Operations.Base;
using ServerMultiTool.Model.Features.Pipeline.Operations.Base.Enums;
using ServerMultiTool.Model.Features.Pipeline.Operations.Execution;
using ServerMultiTool.Model.Features.Pipeline.Operations.FileDelivery;
using ServerMultiTool.Model.Features.Pipeline.Operations.Network;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerMultiTool.Model.Infrastructure.Json
{
    public class PipelineOperationJsonConverter : JsonConverter<PipelineOperationBase>
    {
        public override PipelineOperationBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("OperationType", out var typeProp))
                throw new JsonException("Missing OperationType discriminator for PipelineOperation.");

            OperationType type;
            if (typeProp.ValueKind == JsonValueKind.String)
            {
                var typeStr = typeProp.GetString();
                if (!Enum.TryParse<OperationType>(typeStr, out type))
                    throw new JsonException($"Unknown OperationType: {typeStr}");
            }
            else if (typeProp.ValueKind == JsonValueKind.Number)
            {
                var typeInt = typeProp.GetInt32();
                type = (OperationType)typeInt;
            }
            else
            {
                throw new JsonException("OperationType must be string or number.");
            }

            return type switch
            {
                OperationType.DeliverySpecifiedFilesOperation => JsonSerializer.Deserialize<DeliverySpecifiedFilesOperation>(root.GetRawText(), options),
                OperationType.DeliveryBinOperation => JsonSerializer.Deserialize<DeliveryBinOperation>(root.GetRawText(), options),
                OperationType.MsBuildOperation => JsonSerializer.Deserialize<MsBuildOperation>(root.GetRawText(), options),
                OperationType.GitPullOperation => JsonSerializer.Deserialize<GitPullOperation>(root.GetRawText(), options),
                OperationType.HttpPingOperation => JsonSerializer.Deserialize<HttpPingOperation>(root.GetRawText(), options),
                OperationType.ProcessExecutionOperation => JsonSerializer.Deserialize<ProcessExecutionOperation>(root.GetRawText(), options),
                OperationType.SqlExecutionOperation => JsonSerializer.Deserialize<SqlExecutionOperation>(root.GetRawText(), options),
                OperationType.WebBrowserOperation => JsonSerializer.Deserialize<WebBrowserOperation>(root.GetRawText(), options),
                _ => throw new JsonException($"Unknown operation type: {type}")
            };
        }

        public override void Write(Utf8JsonWriter writer, PipelineOperationBase value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }
}
