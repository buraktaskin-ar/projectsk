using System.Text.Json.Serialization;

namespace ChatWithAPIDemo.Models
{
    public class Reservation
    {

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("person")]
        public Person Person { get; set; }

        [JsonPropertyName("hotel")]
        public Hotel Hotel { get; set; }

        [JsonPropertyName("room")]
        public Room Room { get; set; }

        [JsonPropertyName("check_in")]
        public DateTime CheckIn { get; set; }

        [JsonPropertyName("check_out")]
        public DateTime CheckOut { get; set; }

        [JsonPropertyName("total_price")]
        public decimal TotalPrice { get; set; }


    }
}
