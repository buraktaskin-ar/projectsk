using System.Text.Json.Serialization;

namespace ChatWithAPIDemo.Models
{
    public class AvailabilitySlot
    {
        

        [JsonPropertyName("start_date")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime End { get; set; }

        [JsonPropertyName("current_status")]
        public AvailabilityStatus Status { get; set; } = AvailabilityStatus.Blocked;

        [JsonPropertyName("note")]
        public string? Note { get; set; }


    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AvailabilityStatus
    {
        Available,
        Blocked,
        OutOfService,
        Reserved
    }
}
