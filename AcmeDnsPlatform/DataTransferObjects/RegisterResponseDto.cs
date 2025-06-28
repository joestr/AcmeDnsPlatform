using System.Text.Json.Serialization;

namespace AcmeDnsPlatform.DataTransferObjects
{
    public class RegisterResponseDto
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = "";
        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
        [JsonPropertyName("fulldomain")]
        public string FullDomain { get; set; } = "";
        [JsonPropertyName("subdomain")]
        public string SubDomain { get; set; } = "";
        [JsonPropertyName("allowfrom")]
        public List<string> AllowFrom { get; set; } = new List<string>();
    }
}
