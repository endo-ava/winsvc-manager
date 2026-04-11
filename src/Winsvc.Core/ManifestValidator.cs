using System.Collections.Generic;
using Winsvc.Contracts.Manifest;

namespace Winsvc.Core;

public class ManifestValidator : IManifestValidator
{
    public IEnumerable<string> Validate(ServiceManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Id))
            yield return "Id is required.";
            
        if (manifest.Runtime == null)
            yield return "Runtime configuration is required.";
        else if (string.IsNullOrWhiteSpace(manifest.Runtime.Executable))
            yield return "Runtime Executable is required.";
        
        if (manifest.Service == null)
            yield return "Service configuration is required.";
    }
}
