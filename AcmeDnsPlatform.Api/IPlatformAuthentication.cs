namespace AcmeDnsPlatform.Api;

public interface IPlatformAuthentication
{
    public void AddAuthentication(HttpRequestMessage request);
}