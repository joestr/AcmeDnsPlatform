namespace AcmeDnsPlatform.Api;

public interface IPlatformDnsManagement
{
    public string GetDomain();
    public bool AddTextRecord(string subdomain, string value);
    public bool RemoveTextRecord(string subdomain, string value);
}