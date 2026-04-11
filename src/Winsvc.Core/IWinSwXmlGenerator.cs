using Winsvc.Contracts.Manifest;

namespace Winsvc.Core;

public interface IWinSwXmlGenerator
{
    string Generate(ServiceManifest manifest);
}
