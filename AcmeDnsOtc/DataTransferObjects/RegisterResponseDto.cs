using System.Text.Json.Serialization;

namespace AcmeDnsOtc.DataTransferObjects
{
    public class RegisterResponseDto
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
        [JsonPropertyName("fulldomain")]
        public string FullDomain { get; set; }
        [JsonPropertyName("subdomain")]
        public string SubDomain { get; set; }
        [JsonPropertyName("allowfrom")]
        public string[] AllowFrom { get; set; }
    }
}
