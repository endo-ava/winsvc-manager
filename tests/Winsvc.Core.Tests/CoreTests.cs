using Xunit;
using Winsvc.Contracts.Manifest;
using Winsvc.Core;
using Winsvc.Infrastructure;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Winsvc.Core.Tests;

public class CoreTests
{
    [Fact]
    public void WinSwXmlGenerator_ShouldGenerateValidXml()
    {
        var generator = new WinSwXmlGenerator();
        var manifest = new ServiceManifest
        {
            Id = "sample-service",
            DisplayName = "Sample Service",
            Description = "Sample managed service",
            Runtime = new RuntimeConfig
            {
                WorkDir = @"C:\svc\runtimes\sample-service",
                Executable = @"C:\svc\runtimes\sample-service\app.exe",
                Arguments = new() { "--serve" }
            },
            Service = new ServiceConfig
            {
                StartMode = "delayed-auto",
                OnFailure = "restart",
                ResetFailure = "1 hour"
            }
        };
        manifest.Env["SERVICE_HOST"] = "127.0.0.1";

        // Act
        var xml = generator.Generate(manifest);

        // Assert
        Assert.Contains("<id>sample-service</id>", xml);
        Assert.Contains(@"<executable>C:\svc\runtimes\sample-service\app.exe</executable>", xml);
        Assert.Contains("<env name=\"SERVICE_HOST\" value=\"127.0.0.1\" />", xml);
        Assert.Contains("<onfailure action=\"restart\" delay=\"10 sec\" />", xml);
    }

    [Fact]
    public void ManifestValidator_ShouldReturnErrorsForInvalidManifest()
    {
        var validator = new ManifestValidator();
        var manifest = new ServiceManifest(); // empty
        
        var errors = validator.Validate(manifest).ToList();
        
        Assert.Contains("Id is required.", errors);
    }
    
    [Fact]
    public async Task YamlManifestReader_ShouldReadYamlFile()
    {
        // Arrange
        var yaml = @"
id: sample-service
displayName: Sample Service
description: Sample managed service

runtime:
  workDir: C:\svc\runtimes\sample-service
  executable: C:\svc\runtimes\sample-service\app.exe
  arguments:
    - --serve

service:
  wrapperDir: C:\svc\services\sample-service
  startMode: delayed-auto
  onFailure: restart
  resetFailure: 1 hour

env:
  SERVICE_HOST: 127.0.0.1
  SERVICE_PORT: ""8010""

health:
  url: http://127.0.0.1:8010/health
  timeoutSec: 5
";
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, yaml);
        
        var reader = new YamlManifestReader();

        // Act
        var manifest = await reader.ReadAsync(tempFile);

        // Assert
        Assert.Equal("sample-service", manifest.Id);
        Assert.Equal("Sample Service", manifest.DisplayName);
        Assert.Equal(@"C:\svc\runtimes\sample-service", manifest.Runtime.WorkDir);
        Assert.Single(manifest.Runtime.Arguments);
        Assert.Equal("--serve", manifest.Runtime.Arguments[0]);
        Assert.Equal("127.0.0.1", manifest.Env["SERVICE_HOST"]);
        Assert.Equal("http://127.0.0.1:8010/health", manifest.Health.Url);

        // Cleanup
        File.Delete(tempFile);
    }
}
