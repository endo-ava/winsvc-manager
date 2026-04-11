using System.Threading.Tasks;
using Winsvc.Contracts.Manifest;

namespace Winsvc.Core;

public interface IManifestReader
{
    Task<ServiceManifest> ReadAsync(string path);
}
