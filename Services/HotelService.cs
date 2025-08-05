using ChatWithAPIDemo.Models;
using ChatWithAPIDemo.ValueObjects;

namespace ChatWithAPIDemo.Services;


public class HotelService
{
    private readonly List<Hotel> _hotels;

    public HotelService(List<Hotel> hotels)
    {
        _hotels = hotels;
    }

    public List<Hotel> GetAllHotels() => _hotels;

    public List<Hotel> SearchHotelsByCity(string city)
    {
        return _hotels.Where(h => h.Address.City.Contains(city, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    public List<Hotel> SearchHotelsByStarRating(int minRating)
    {
        return _hotels.Where(h => h.StarRating >= minRating).ToList();
    }

    public Hotel? GetHotelById(int id)
    {
        return _hotels.FirstOrDefault(h => h.Id == id);
    }


    public Hotel AddHotel(string name, int starRating, string city, string street)
    {
        var hotel = new Hotel
        {
            Id = _hotels.Count > 0 ? _hotels.Max(h => h.Id) + 1 : 1,
            Name = name,
            StarRating = starRating,
            Address = new Address { City = city, Street = street }
        };
        _hotels.Add(hotel);
        return hotel;
    }

}
