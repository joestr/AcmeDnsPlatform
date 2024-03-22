using System.Text.Json.Serialization;

namespace AcmeDnsOtc.DataTransferObjects
{
    public class UpdateRequestDto
    {
        [JsonPropertyName("subdomain")]
        public string SubDomain { get; set; }
        [JsonPropertyName("txt")]
        public string TxtRecordValue { get; set; }

    }
}
