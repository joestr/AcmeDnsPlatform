using System.Text.Json.Serialization;

namespace AcmeDnsPlatform.DataTransferObjects
{
    public class UpdateRequestDto
    {
        [JsonPropertyName("subdomain")]
        public string SubDomain { get; set; }
        [JsonPropertyName("txt")]
        public string TxtRecordValue { get; set; }

    }
}
