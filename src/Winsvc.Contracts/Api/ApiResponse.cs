namespace Winsvc.Contracts.Api;

public sealed record ApiInfoResponse(string Name, string Status);

public sealed record ErrorResponse(string Error);

public sealed record WindowsServiceResponse(
    string Id,
    string DisplayName,
    string State,
    string StartMode);

public sealed record ManagedServiceResponse(
    string Id,
    string DisplayName,
    string Description,
    string Type,
    string State,
    string? StartMode,
    string HealthUrl);

public sealed record ManagedServiceDetailResponse(
    string Id,
    string DisplayName,
    string Description,
    string Type,
    string State,
    string? StartMode,
    string HealthUrl,
    string WrapperDir,
    string WorkDir,
    bool TailscaleServeEnabled,
    int TailscaleServeHttpsPort,
    string TailscaleServeTarget);

public sealed record ServiceHealthResponse(
    string Id,
    string Health,
    string Url,
    int TimeoutSec);

public sealed record ServiceActionResponse(
    string Id,
    string Action,
    string Status);
