using System.Text.Json.Serialization;

namespace AcmeDnsPlatform.DataTransferObjects
{
    public class RegisterRequestDto
    {
        [JsonPropertyName("allowfrom")]
        public string[]? AllowFrom { get; set; }
    }
}
