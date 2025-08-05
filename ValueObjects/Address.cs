using System.Text.Json.Serialization;

namespace ChatWithAPIDemo.ValueObjects
{
    public class Address
    {
        [JsonPropertyName("city")]
        public string? City { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("zip_code")]
        public string? ZipCode { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = "TR";

    }
}
