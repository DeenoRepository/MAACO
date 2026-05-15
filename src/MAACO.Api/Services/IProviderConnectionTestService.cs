using MAACO.Api.Contracts.Settings;

namespace MAACO.Api.Services;

public interface IProviderConnectionTestService
{
    Task<ProviderConnectionTestResultDto> TestAsync(TestProviderConnectionRequest request, CancellationToken cancellationToken);
}
