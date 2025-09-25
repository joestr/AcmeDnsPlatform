namespace AcmeDnsPlatform.Provider.BunnyDns;

public class BunnyDnsRecord
{
    public int Id { get; set; }
    public int Type { get; set; }
    public int Ttl { get; set; }
    public string? Value { get; set; }
    public string? Name { get; set; }
}