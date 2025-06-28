using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AcmeDnsPlatform.Api;

namespace AcmeDnsPlatform.Provider.Bunny;

public class BunnyDns : IPlatformDnsManagement
{
    private HttpClient _httpClient;
    
    private string _zoneId = "";
    private string _accessKey = "";

    public BunnyDns()
    {
        this.GetVariables();
        
        _httpClient = new HttpClient();
    }

    private void GetVariables()
    {
        var accessKey = Environment.GetEnvironmentVariable("BUNNYDNS_ACCESSKEY");
        if (accessKey == null)
        {
            throw new PlatformEnvironmentVariableNotSet("Environment variable \"BUNNYDNS_ACCESSKEY\" not set!");
        }
        _accessKey = accessKey;
        
        var zoneId = Environment.GetEnvironmentVariable("BUNNYDNS_ZONEID");
        if (zoneId == null)
        {
            throw new PlatformEnvironmentVariableNotSet("Environment variable \"BUNNYDNS_ZONEID\" not set!");
        }
        _zoneId = zoneId;
    }

    public string GetDomain()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.bunny.net/dnszone/" + _zoneId),
            Headers =
            {
                { "accept", "application/json" },
                { "AccessKey", _accessKey }
            },
        };
        BunnyDnsZone? zone;
        using (var response = _httpClient.Send(request))
        {
            response.EnsureSuccessStatusCode();
            var body = response.Content.ReadAsStringAsync().Result;
            zone = JsonSerializer.Deserialize<BunnyDnsZone>(body);
        }

        if (zone == null)
        {
            return "";
        }

        return zone.Domain;
    }

    public bool AddTextRecord(string subdomain, string value)
    {
        var dnsRecord = new BunnyDnsRecord()
        {
            Name = subdomain,
            Ttl = 3600,
            Type = 3, /* MX */
            Value = value
        };
        
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri("https://api.bunny.net/dnszone/" + _zoneId + "/records"),
            Headers =
            {
                { "accept", "application/json" },
                { "AccessKey", _accessKey }
            },
            Content = JsonContent.Create(dnsRecord)
        };
        using (var response = _httpClient.Send(request))
        {
            response.EnsureSuccessStatusCode();
            var body = response.Content.ReadAsStringAsync();
        }

        return true;
    }

    public bool RemoveTextRecord(string subdomain, string value)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://api.bunny.net/dnszone/" + _zoneId),
            Headers =
            {
                { "accept", "application/json" },
                { "AccessKey", _accessKey }
            },
        };
        BunnyDnsZone? zone;
        using (var response = _httpClient.Send(request))
        {
            response.EnsureSuccessStatusCode();
            var body = response.Content.ReadAsStringAsync().Result;
            zone = JsonSerializer.Deserialize<BunnyDnsZone>(body);
        }

        if (zone == null)
        {
            return false;
        }

        var txtRecords = zone.Records.Where(record =>
            record.Name == subdomain
            && record.Type == 3);

        foreach (var txtRecord in txtRecords)
        {
            var requestDelete = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri("https://api.bunny.net/dnszone/" + _zoneId + "/records/" + txtRecord.Id),
                Headers =
                {
                    { "accept", "application/json" },
                    { "AccessKey", _accessKey }
                },
            };
            using (var response = _httpClient.Send(requestDelete))
            {
                response.EnsureSuccessStatusCode();
                var body = response.Content.ReadAsStringAsync().Result;
            }
        }

        return true;
    }
}