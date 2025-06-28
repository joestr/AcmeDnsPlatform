using System.Text.Json.Serialization;

namespace AcmeDnsPlatform.DataTransferObjects
{
    public class UpdateResponeDto
    {
        [JsonPropertyName("txt")]
        public string TxtRecordValue { get; set; } = "";
    }
}
