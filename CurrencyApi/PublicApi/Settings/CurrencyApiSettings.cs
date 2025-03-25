namespace Fuse8.BackendInternship.PublicApi.Settings;

public class CurrencyApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    
    public void Validate()
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new InvalidOperationException("API ключ не указан.");
        }
    }
}