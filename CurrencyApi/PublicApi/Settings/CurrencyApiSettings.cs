namespace Fuse8.BackendInternship.PublicApi.Settings;

public class CurrencyApiSettings
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    
    public void Validate()
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new InvalidOperationException("API ключ не указан.");
        }
    }
}