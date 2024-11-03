namespace AcmeDnsPlatform.Api;

public interface IPlatformDnsManagement
{
    public Account RegisterAccount(string[] allowFrom);
    public bool AddTextRecord(string subdomain, string value);
    public bool RemoveTextRecord(string subdomain, string value);
}