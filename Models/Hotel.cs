using System.Text.Json.Serialization;
using ChatWithAPIDemo.ValueObjects;
namespace ChatWithAPIDemo.Models
{
    public class Hotel
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("star_rating")]
        public int StarRating { get; set; }

        [JsonPropertyName("address")]
        public Address Address { get; set; }


    }
}
