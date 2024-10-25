namespace AcmeDnsPlatform.Api;

public interface IPlatformProvider
{
    public bool RegisterAccount(string[] ip);
    public bool AddTextRecord(string value);
    public bool RemoveTextRecord(string value);
}