using System.Threading.Tasks;
using Winsvc.Contracts;
using Winsvc.Contracts.Manifest;

namespace Winsvc.Core;

public interface IHealthChecker
{
    Task<HealthState> CheckAsync(HealthConfig config);
}
