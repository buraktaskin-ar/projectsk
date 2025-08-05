using ChatWithAPIDemo.Services;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace ChatWithAPIDemo.Plugins;

public class HotelPlugin
{
    private readonly HotelService _hotelService;

    public HotelPlugin(HotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [KernelFunction, Description("Get all available hotels")]
    public string GetAllHotels()
    {
        var hotels = _hotelService.GetAllHotels();
        return System.Text.Json.JsonSerializer.Serialize(hotels, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    [KernelFunction, Description("Search hotels by city name")]
    public string SearchHotelsByCity([Description("City name to search for")] string city)
    {
        var hotels = _hotelService.SearchHotelsByCity(city);
        return System.Text.Json.JsonSerializer.Serialize(hotels, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    [KernelFunction, Description("Search hotels by minimum star rating")]
    public string SearchHotelsByStarRating([Description("Minimum star rating (1-5)")] int minRating)
    {
        var hotels = _hotelService.SearchHotelsByStarRating(minRating);
        return System.Text.Json.JsonSerializer.Serialize(hotels, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    [KernelFunction, Description("Create a new hotel")]
    public string CreateHotel(
       [Description("Hotel adı")] string name,
       [Description("Yıldız sayısı (1-5)")] int starRating,
       [Description("Şehir")] string city,
       [Description("Sokak")] string street)
    {
        var newHotel = _hotelService.AddHotel(name, starRating, city, street);
        return System.Text.Json.JsonSerializer.Serialize(
            newHotel,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
        );
    }
}
