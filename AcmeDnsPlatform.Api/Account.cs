namespace AcmeDnsPlatform.Api;

public class Account : Credentials
{
    public string Subdomain { get; set; } = "";
    public string FullDomain { get; set; } = "";
}