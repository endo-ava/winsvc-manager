using System.Collections.Generic;
using Winsvc.Contracts.Manifest;

namespace Winsvc.Core;

public interface IManifestValidator
{
    IEnumerable<string> Validate(ServiceManifest manifest);
}
