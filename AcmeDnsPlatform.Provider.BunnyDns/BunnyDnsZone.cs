namespace AcmeDnsPlatform.Provider.BunnyDns;

public class BunnyDnsZone
{
    public string Domain { get; set; } = "";
    public List<BunnyDnsRecord> Records { get; set; } = new List<BunnyDnsRecord>();
}