namespace AcmeDnsPlatform.Provider.Bunny;

public class PutDnsRecord
{
    public int Type { get; set; }
    public int Ttl { get; set; }
    public string Value { get; set; }
    public string Name { get; set; }
}