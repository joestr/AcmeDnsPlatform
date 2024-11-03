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

public class BunnyPlatformAll : Api.IPlatformDnsManagement, IPlatformAccountManagement
{
    private HttpClient _httpClient;
    private IPlatformAuthentication _platformAuthentication;
    private IHashFunction _hashFunction;
    private string _zoneId;

    public BunnyPlatformAll(IPlatformAuthentication platformAuthentication, IHashFunction hashFunction, string zoneId)
    {
        _httpClient = new HttpClient();
        _platformAuthentication = platformAuthentication;
        _hashFunction = hashFunction;
        _zoneId = zoneId;
    }
    
    public Account RegisterAccount(string[] allowFrom)
    {
        var passwordBytes = new byte[32];
        Random.Shared.NextBytes(passwordBytes);

        var credentials = new Credentials()
        {
            Username = Guid.NewGuid().ToString(),
            Password = Convert.ToBase64String(passwordBytes),
            AllowFrom = allowFrom
        };

        var domain = Guid.NewGuid().ToString();
        
        var credentialsPassword = JsonSerializer.Deserialize<Credentials>(JsonSerializer.Serialize(credentials));
        credentialsPassword.Password = Encoding.Default.GetString(_hashFunction.Hash(Encoding.Default.GetBytes(credentialsPassword.Password)));
        var putRecord = new PutDnsRecord()
        {
            Type = 3,
            Ttl = 8600,
            Name = "creds." + credentials.Username,
            Value = JsonSerializer.Serialize(credentialsPassword)
        };
        
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Put;
        request.RequestUri = new Uri($"https://api.bunny.net/dnszone/{_zoneId}/records");
        request.Content = JsonContent.Create(JsonSerializer.Serialize(putRecord));
        request.Headers.Add(HttpRequestHeader.ContentType.ToString(), MediaTypeNames.Application.Json);

        var result = new Account()
        {
            Username = credentials.Username,
            Password = credentials.Password,
            AllowFrom = credentials.AllowFrom,
            FullDomain = "auth." + domain,
            Subdomain = "auth." + domain
        };

        return result;
    }

    public bool CheckCredentials(string username, string password, string ip)
    {
        throw new NotImplementedException();
    }

    public bool AddTextRecord(string subdomain, string value)
    {
        throw new NotImplementedException();
    }

    public bool RemoveTextRecord(string subdomain, string value)
    {
        throw new NotImplementedException();
    }
}