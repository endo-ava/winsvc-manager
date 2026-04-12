using System.IO;
using Microsoft.AspNetCore.Http;
using Winsvc.Contracts;
using Winsvc.Contracts.Manifest;
using Winsvc.Core;
using Winsvc.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://127.0.0.1:8011");

builder.Services.AddSingleton<IManifestReader, YamlManifestReader>();
builder.Services.AddSingleton<IManifestValidator, ManifestValidator>();
builder.Services.AddSingleton<IServiceConfigGenerator, WinSwXmlGenerator>();
builder.Services.AddSingleton<IHealthChecker, HttpClientHealthChecker>();
builder.Services.AddSingleton<IWindowsServiceMonitor, WindowsServiceMonitor>();
builder.Services.AddSingleton<IServiceManager, WinSwServiceManager>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new ApiInfoResponse("winsvc-manager", "ok")));

app.MapGet("/services/windows", async (IWindowsServiceMonitor monitor) =>
{
    var services = await monitor.GetAllServicesAsync();
    var response = services
        .OrderBy(service => service.Id, StringComparer.OrdinalIgnoreCase)
        .Select(MapWindowsServiceResponse);

    return Results.Ok(response);
});

app.MapGet("/services/managed", async (
    IManifestReader manifestReader,
    IManifestValidator manifestValidator,
    IWindowsServiceMonitor monitor,
    CancellationToken cancellationToken) =>
{
    var manifests = await LoadManifestsAsync(manifestReader, manifestValidator, cancellationToken);
    var response = new List<ManagedServiceResponse>();

    foreach (var manifest in manifests)
    {
        var windowsService = await monitor.GetServiceAsync(manifest.Id);
        response.Add(new ManagedServiceResponse(
            manifest.Id,
            manifest.DisplayName,
            manifest.Description,
            manifest.Type,
            MapServiceState(windowsService),
            windowsService?.StartMode,
            manifest.Health.Url));
    }

    return Results.Ok(response);
});

app.MapGet("/services/{id}", async (
    string id,
    IManifestReader manifestReader,
    IManifestValidator manifestValidator,
    IWindowsServiceMonitor monitor,
    CancellationToken cancellationToken) =>
{
    var manifest = await LoadManifestAsync(id, manifestReader, manifestValidator, cancellationToken);
    if (manifest is null)
    {
        return Results.NotFound(new ErrorResponse($"Managed service '{id}' was not found."));
    }

    var windowsService = await monitor.GetServiceAsync(manifest.Id);

    return Results.Ok(new ManagedServiceDetailResponse(
        manifest.Id,
        manifest.DisplayName,
        manifest.Description,
        manifest.Type,
        MapServiceState(windowsService),
        windowsService?.StartMode,
        manifest.Health.Url,
        manifest.Service.WrapperDir,
        manifest.Runtime.WorkDir,
        manifest.Exposure.TailscaleServe.Enabled,
        manifest.Exposure.TailscaleServe.HttpsPort,
        manifest.Exposure.TailscaleServe.Target));
});

app.MapGet("/services/{id}/health", async (
    string id,
    IManifestReader manifestReader,
    IManifestValidator manifestValidator,
    IHealthChecker healthChecker,
    CancellationToken cancellationToken) =>
{
    var manifest = await LoadManifestAsync(id, manifestReader, manifestValidator, cancellationToken);
    if (manifest is null)
    {
        return Results.NotFound(new ErrorResponse($"Managed service '{id}' was not found."));
    }

    var health = await healthChecker.CheckAsync(manifest.Health);

    return Results.Ok(new ServiceHealthResponse(
        manifest.Id,
        health.ToString(),
        manifest.Health.Url,
        manifest.Health.TimeoutSec));
});

app.MapPost("/services/{id}/start", async (
    string id,
    IManifestReader manifestReader,
    IManifestValidator manifestValidator,
    IServiceManager serviceManager,
    CancellationToken cancellationToken) =>
{
    var manifest = await LoadManifestAsync(id, manifestReader, manifestValidator, cancellationToken);
    if (manifest is null)
    {
        return Results.NotFound(new ErrorResponse($"Managed service '{id}' was not found."));
    }

    await serviceManager.StartAsync(manifest);
    return Results.Ok(new ServiceActionResponse(manifest.Id, "start", "queued"));
});

app.MapPost("/services/{id}/stop", async (
    string id,
    IManifestReader manifestReader,
    IManifestValidator manifestValidator,
    IServiceManager serviceManager,
    CancellationToken cancellationToken) =>
{
    var manifest = await LoadManifestAsync(id, manifestReader, manifestValidator, cancellationToken);
    if (manifest is null)
    {
        return Results.NotFound(new ErrorResponse($"Managed service '{id}' was not found."));
    }

    await serviceManager.StopAsync(manifest);
    return Results.Ok(new ServiceActionResponse(manifest.Id, "stop", "queued"));
});

app.MapPost("/services/{id}/restart", async (
    string id,
    IManifestReader manifestReader,
    IManifestValidator manifestValidator,
    IServiceManager serviceManager,
    CancellationToken cancellationToken) =>
{
    var manifest = await LoadManifestAsync(id, manifestReader, manifestValidator, cancellationToken);
    if (manifest is null)
    {
        return Results.NotFound(new ErrorResponse($"Managed service '{id}' was not found."));
    }

    await serviceManager.RestartAsync(manifest);
    return Results.Ok(new ServiceActionResponse(manifest.Id, "restart", "queued"));
});

app.Run();

static async Task<IReadOnlyList<ServiceManifest>> LoadManifestsAsync(
    IManifestReader manifestReader,
    IManifestValidator manifestValidator,
    CancellationToken cancellationToken)
{
    var manifestDirectory = Path.Combine(AppContext.BaseDirectory, "manifests");
    if (!Directory.Exists(manifestDirectory))
    {
        manifestDirectory = Path.Combine(Directory.GetCurrentDirectory(), "manifests");
    }

    if (!Directory.Exists(manifestDirectory))
    {
        return Array.Empty<ServiceManifest>();
    }

    var paths = Directory
        .EnumerateFiles(manifestDirectory, "*.y*ml", SearchOption.TopDirectoryOnly)
        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);

    var manifests = new List<ServiceManifest>();

    foreach (var path in paths)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var manifest = await manifestReader.ReadAsync(path);
        var errors = manifestValidator.Validate(manifest).ToArray();
        if (errors.Length > 0)
        {
            continue;
        }

        manifests.Add(manifest);
    }

    return manifests;
}

static async Task<ServiceManifest?> LoadManifestAsync(
    string id,
    IManifestReader manifestReader,
    IManifestValidator manifestValidator,
    CancellationToken cancellationToken)
{
    var manifests = await LoadManifestsAsync(manifestReader, manifestValidator, cancellationToken);
    return manifests.FirstOrDefault(manifest => string.Equals(manifest.Id, id, StringComparison.OrdinalIgnoreCase));
}

static string MapServiceState(WindowsServiceInfo? windowsService)
{
    return (windowsService?.State ?? ServiceState.NotFound).ToString();
}

static WindowsServiceResponse MapWindowsServiceResponse(WindowsServiceInfo service)
{
    return new WindowsServiceResponse(
        service.Id,
        service.DisplayName,
        service.State.ToString(),
        service.StartMode);
}

internal sealed record ApiInfoResponse(string Name, string Status);

internal sealed record ErrorResponse(string Error);

internal sealed record WindowsServiceResponse(
    string Id,
    string DisplayName,
    string State,
    string StartMode);

internal sealed record ManagedServiceResponse(
    string Id,
    string DisplayName,
    string Description,
    string Type,
    string State,
    string? StartMode,
    string HealthUrl);

internal sealed record ManagedServiceDetailResponse(
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

internal sealed record ServiceHealthResponse(
    string Id,
    string Health,
    string Url,
    int TimeoutSec);

internal sealed record ServiceActionResponse(
    string Id,
    string Action,
    string Status);
