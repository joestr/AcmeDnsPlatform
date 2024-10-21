namespace AcmeDnsPlatform.Api;

public interface Provider
{
    public bool RegisterAccount(string[] ip);
    public bool AddTextRecord(string value);
    public bool RemoveTextRecord(string value);
}