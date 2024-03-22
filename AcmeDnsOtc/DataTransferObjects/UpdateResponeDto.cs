using System.Text.Json.Serialization;

namespace AcmeDnsOtc.DataTransferObjects
{
    public class UpdateResponeDto
    {
        [JsonPropertyName("txt")]
        public string TxtRecordValue { get; set; }
    }
}
