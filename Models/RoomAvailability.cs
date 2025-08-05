using System.Text.Json.Serialization;

namespace ChatWithAPIDemo.Models
{
    public class RoomAvailability
    {
        [JsonPropertyName("id")] public int? Id { get; set; }
        [JsonPropertyName("room")] public Room Room { get; set; }
        [JsonPropertyName("availability_slot")] public AvailabilitySlot AvailabilitySlot { get; set; }
    }
}
