using System.Text.Json.Serialization;

namespace ChatWithAPIDemo.Models;
/*
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
*/

public class Room
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("room_number")]
    public string RoomNumber { get; set; } = string.Empty;

    [JsonPropertyName("floor")]
    public int Floor { get; set; }

    [JsonPropertyName("hotel")]
    public Hotel Hotel { get; set; }

    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    [JsonPropertyName("is_sea_view")]
    public bool IsSeaView { get; set; }

    [JsonPropertyName("room_type")]
    public RoomType RoomType { get; set; } = RoomType.Standard;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("room_availability")]
    public List<RoomAvailability> RoomAvailabilities { get; set; } = new List<RoomAvailability>();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoomType
{
    Standard,
    Deluxe,
    Superior,
    Suite,
    Presidential
}

