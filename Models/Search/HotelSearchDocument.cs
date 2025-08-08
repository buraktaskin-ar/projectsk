namespace ChatWithAPIDemo.Models.Search
{
    public class HotelSearchDocument
    {
        public string HotelId { get; set; } = default!;
        public string HotelName { get; set; } = default!;
        public string City { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string Address { get; set; } = default!;
        public int StarRating { get; set; }
        public double PricePerNight { get; set; }
        public string Description { get; set; } = default!;
        public string Amenities { get; set; } = default!;
        public string RoomTypes { get; set; } = default!;
        public string CancellationPolicy { get; set; } = default!;
        public string CheckInCheckOut { get; set; } = default!;
        public string HouseRules { get; set; } = default!;
        public string NearbyAttractions { get; set; } = default!;
        public bool HasPool { get; set; }
        public bool HasGym { get; set; }
        public bool HasSpa { get; set; }
        public bool PetFriendly { get; set; }
        public bool HasParking { get; set; }
        public bool HasWifi { get; set; }
        public float[]? DescriptionVector { get; set; }
        public float[]? AmenitiesVector { get; set; }
    }
}