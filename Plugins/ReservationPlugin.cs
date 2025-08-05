using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using ChatWithAPIDemo.Services;

namespace ChatWithAPIDemo.Plugins;

public class ReservationPlugin
{
    private readonly ReservationService _reservationService;
    private static readonly JsonSerializerOptions J = new() { WriteIndented = true };

    public ReservationPlugin(ReservationService reservationService) => _reservationService = reservationService;

    [KernelFunction, Description("Get all reservations")]
    public string GetAllReservations()
        => JsonSerializer.Serialize(_reservationService.GetAllReservations(), J);

    [KernelFunction, Description("Get reservations for a specific hotel")]
    public string GetReservationsByHotelId([Description("Hotel id")] int hotelId)
        => JsonSerializer.Serialize(_reservationService.GetReservationsByHotelId(hotelId), J);

    [KernelFunction, Description("Get reservations for a specific person")]
    public string GetReservationsByPersonId([Description("Person id")] int personId)
        => JsonSerializer.Serialize(_reservationService.GetReservationsByPersonId(personId), J);

    [KernelFunction, Description("Get a reservation by id")]
    public string GetReservationById([Description("Reservation id")] int reservationId)
        => JsonSerializer.Serialize(_reservationService.GetReservationById(reservationId), J);

    [KernelFunction, Description("Create a reservation (checks RoomAvailability)")]
    public string CreateReservation(
        [Description("Person id")] int personId,
        [Description("Hotel id")] int hotelId,
        [Description("Room id")] int roomId,
        [Description("Check-in date (yyyy-MM-dd or ISO-8601)")] string checkIn,
        [Description("Check-out date (yyyy-MM-dd or ISO-8601)")] string checkOut)
    {
        if (!TryParseDate(checkIn, out var ci) || !TryParseDate(checkOut, out var co))
            return JsonSerializer.Serialize(new { error = "Invalid date format" }, J);

        var res = _reservationService.CreateReservation(personId, hotelId, roomId, ci, co);
        if (res is null)
            return JsonSerializer.Serialize(new { error = "CreateReservation failed (not found or not available)" }, J);

        return JsonSerializer.Serialize(res, J);
    }

    [KernelFunction, Description("Cancel a reservation")]
    public string CancelReservation([Description("Reservation id")] int reservationId)
    {
        var ok = _reservationService.CancelReservation(reservationId);
        return JsonSerializer.Serialize(new { success = ok }, J);
    }

    private static bool TryParseDate(string s, out DateTime dt) =>
        DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt);
}