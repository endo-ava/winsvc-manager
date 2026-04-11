using System.IO;
using System.Threading.Tasks;
using Winsvc.Contracts.Manifest;
using Winsvc.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Winsvc.Infrastructure;

public class YamlManifestReader : IManifestReader
{
    public async Task<ServiceManifest> ReadAsync(string path)
    {
        var content = await File.ReadAllTextAsync(path);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var manifest = deserializer.Deserialize<ServiceManifest>(content);
        return manifest ?? new ServiceManifest();
    }
}
