using System.Text.Json.Serialization;

namespace AcmeDnsOtc.DataTransferObjects
{
    public class RegisterRequestDto
    {
        [JsonPropertyName("allowfrom")]
        public string[]? AllowFrom { get; set; }
    }
}
