using AcmeDnsPlatform.Api;

namespace AcmeDnsPlatform.Provider.Bunny;

public class BunnyPlatformProvider : Api.IPlatformProvider
{
    private HttpClient _httpClient;
    private IPlatformAuthentication _platformAuthentication;
    private string _zoneId;

    public BunnyPlatformProvider(IPlatformAuthentication platformAuthentication, string zoneId)
    {
        _httpClient = new HttpClient();
        _platformAuthentication = platformAuthentication;
        _zoneId = zoneId;
    }
    
    public bool RegisterAccount(string[] ip)
    {
        throw new NotImplementedException();
    }

    public bool AddTextRecord(string value)
    {
        throw new NotImplementedException();
    }

    public bool RemoveTextRecord(string value)
    {
        throw new NotImplementedException();
    }
}