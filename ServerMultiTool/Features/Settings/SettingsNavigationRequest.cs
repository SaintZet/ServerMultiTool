namespace ServerMultiTool.Features.Settings;

public sealed record SettingsNavigationRequest(string TabKey, string? PipelineProfileName = null);

