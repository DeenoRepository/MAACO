namespace MAACO.App.Services;

public interface IApiClient
{
    Task<bool> CheckHealthAsync(CancellationToken cancellationToken);
}
