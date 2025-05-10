namespace API.Application.Adapters.HttpClient.Configuration;

public class KeycloakSettings
{
    public string base_url { get; set; }
    public string client_id { get; set; }
    public string client_secret { get; set; }
    public string grant_type { get; set; }
    public string content_type { get; set; }
    public string realms { get; set; }
} 