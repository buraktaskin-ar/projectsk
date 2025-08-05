using System.Text.Json.Serialization;

namespace ChatWithAPIDemo.Models
{
    public class Person
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } 

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string? Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("loyalty_points")]
        public int? LoyaltyPoints { get; set; }
    }




}
