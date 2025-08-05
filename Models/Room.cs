using System.Text.Json.Serialization;

namespace ChatWithAPIDemo.Models
{
    public class Room
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("room_number")]
        public string RoomNumber { get; set; } = string.Empty;

        [JsonPropertyName("floor")]
        public int Floor { get; set; }

        [JsonPropertyName("Hotel")]
        public Hotel Hotel{ get; set; }

        [JsonPropertyName("capacity")]
        public int Capacity { get; set; }

        [JsonPropertyName("is_sea_view")]
        public bool IsSeaView { get; set; }

        [JsonPropertyName("is_smoking_allowed")]
        public bool IsSmokingAllowed { get; set; }

        [JsonPropertyName("is_available")]
        public bool IsAvailable { get; set; } = true;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }


    }
}
