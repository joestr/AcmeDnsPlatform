using System.Text.Json.Serialization;

namespace AcmeDnsPlatform.DataTransferObjects
{
    public class RegisterRequestDto
    {
        [JsonPropertyName("allowfrom")]
        public List<string> AllowFrom { get; set; } = new List<string>();
    }
}
