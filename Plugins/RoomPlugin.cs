using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using ChatWithAPIDemo.Services;
namespace ChatWithAPIDemo.Plugins;

public class RoomPlugin
{
    private readonly RoomService _roomService;
    private static readonly JsonSerializerOptions J = new() { WriteIndented = true };

    public RoomPlugin(RoomService roomService) => _roomService = roomService;

    [KernelFunction, Description("Get all rooms")]
    public string GetAllRooms() => JsonSerializer.Serialize(_roomService.GetAllRooms(), J);

    [KernelFunction, Description("Get a room by id")]
    public string GetRoomById([Description("Room id")] int roomId)
        => JsonSerializer.Serialize(_roomService.GetRoomById(roomId), J);

    [KernelFunction, Description("Search rooms by minimum capacity")]
    public string SearchByCapacity([Description("Minimum capacity")] int minCapacity)
        => JsonSerializer.Serialize(_roomService.SearchRoomsByCapacity(minCapacity), J);

    [KernelFunction, Description("Search rooms by sea view")]
    public string SearchBySeaView([Description("Has sea view (true/false)")] bool hasSeaView)
        => JsonSerializer.Serialize(_roomService.SearchRoomsBySeaView(hasSeaView), J);

    [KernelFunction, Description("Search rooms by smoking permission")]
    public string SearchBySmokingAllowed([Description("Smoking allowed (true/false)")] bool smokingAllowed)
        => JsonSerializer.Serialize(_roomService.SearchRoomsBySmokingAllowed(smokingAllowed), J);

    [KernelFunction, Description("Search rooms by price range")]
    public string SearchByPriceRange(
        [Description("Min price")] decimal minPrice,
        [Description("Max price")] decimal maxPrice)
        => JsonSerializer.Serialize(_roomService.SearchRoomsByPriceRange(minPrice, maxPrice), J);

    [KernelFunction, Description("Check if a room is available in a date range")]
    public string IsRoomAvailable(
        [Description("Room id")] int roomId,
        [Description("Check-in date (yyyy-MM-dd or ISO-8601)")] string checkIn,
        [Description("Check-out date (yyyy-MM-dd or ISO-8601)")] string checkOut)
    {
        if (!TryParseDate(checkIn, out var ci) || !TryParseDate(checkOut, out var co))
            return JsonSerializer.Serialize(new { error = "Invalid date format" }, J);

        var ok = _roomService.IsRoomAvailable(roomId, ci, co);
        return JsonSerializer.Serialize(new { roomId, checkIn = ci, checkOut = co, available = ok }, J);
    }

    [KernelFunction, Description("Block a room by adding a Reserved availability slot")]
    public string BlockRoomAvailability(
        [Description("Room id")] int roomId,
        [Description("Check-in date (yyyy-MM-dd or ISO-8601)")] string checkIn,
        [Description("Check-out date (yyyy-MM-dd or ISO-8601)")] string checkOut,
        [Description("Note")] string note = "Reserved")
    {
        if (!TryParseDate(checkIn, out var ci) || !TryParseDate(checkOut, out var co))
            return JsonSerializer.Serialize(new { error = "Invalid date format" }, J);

        _roomService.BlockRoomAvailability(roomId, ci, co, note);
        return JsonSerializer.Serialize(new { success = true }, J);
    }

    [KernelFunction, Description("Free a previously reserved availability slot for a room")]
    public string FreeRoomAvailability(
        [Description("Room id")] int roomId,
        [Description("Check-in date (yyyy-MM-dd or ISO-8601)")] string checkIn,
        [Description("Check-out date (yyyy-MM-dd or ISO-8601)")] string checkOut)
    {
        if (!TryParseDate(checkIn, out var ci) || !TryParseDate(checkOut, out var co))
            return JsonSerializer.Serialize(new { error = "Invalid date format" }, J);

        _roomService.FreeRoomAvailability(roomId, ci, co);
        return JsonSerializer.Serialize(new { success = true }, J);
    }

    [KernelFunction, Description("Get all availability slots for a room")]
    public string GetRoomAvailabilities([Description("Room id")] int roomId)
        => JsonSerializer.Serialize(_roomService.GetRoomAvailabilities(roomId), J);

    [KernelFunction, Description("Add a new room")]
    public string AddRoom(
        [Description("Room number")] string roomNumber,
        [Description("Floor")] int floor,
        [Description("Capacity")] int capacity,
        [Description("Sea view (true/false)")] bool isSeaView,
        [Description("Smoking allowed (true/false)")] bool isSmokingAllowed,
        [Description("Price")] decimal price)
        => JsonSerializer.Serialize(_roomService.AddRoom(roomNumber, floor, capacity, isSeaView, isSmokingAllowed, price), J);

    [KernelFunction, Description("Set the high-level availability flag for a room")]
    public string UpdateRoomAvailability(
        [Description("Room id")] int roomId,
        [Description("Is available (true/false)")] bool isAvailable)
        => JsonSerializer.Serialize(new { success = _roomService.UpdateRoomAvailability(roomId, isAvailable) }, J);

    private static bool TryParseDate(string s, out DateTime dt) =>
        DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt);
}
