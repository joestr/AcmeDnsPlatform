namespace AcmeDnsPlatform.Provider.Bunny;

public class BunnyDnsZone
{
    public string Domain { get; set; } = "";
    public List<BunnyDnsRecord> Records { get; set; } = new List<BunnyDnsRecord>();
}