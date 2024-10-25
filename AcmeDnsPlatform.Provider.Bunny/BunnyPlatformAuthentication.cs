using AcmeDnsPlatform.Api;

namespace AcmeDnsPlatform.Provider.Bunny;

public class BunnyPlatformAuthentication : IPlatformAuthentication
{
    private readonly string _accessKey;
    
    public BunnyPlatformAuthentication(string accessKey)
    {
        _accessKey = accessKey;
    }
    
    public void AddAuthentication(HttpRequestMessage request)
    {
        request.Headers.Add("AccessKey", new []{this._accessKey});
    }
}