using Winsvc.Core;
using Winsvc.Infrastructure;

namespace Winsvc.Hosting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWinsvcServices(this IServiceCollection services)
    {
        services.AddSingleton<IManifestReader, YamlManifestReader>();
        services.AddSingleton<IManifestValidator, ManifestValidator>();
        services.AddSingleton<IServiceConfigGenerator, WinSwXmlGenerator>();
        services.AddSingleton<IHealthChecker, HttpClientHealthChecker>();
        services.AddSingleton<IWindowsServiceMonitor, WindowsServiceMonitor>();
        services.AddSingleton<IServiceManager, WinSwServiceManager>();
        return services;
    }
}
