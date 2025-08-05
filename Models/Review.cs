using System.Text.Json.Serialization;

namespace ChatWithAPIDemo.Models
{
    public class Review
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;


        [JsonPropertyName("person")]
        public Person Person { get; set; }

        [JsonPropertyName("hotel")]
        public Hotel Hotel { get; set; }


    }
}
