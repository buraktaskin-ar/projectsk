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
    public string GetReservationsByPersonId([Description("Person id (GUID)")] string personId)
    {
        if (!Guid.TryParse(personId, out var id))
            return JsonSerializer.Serialize(new { error = "Invalid GUID format" }, J);

        return JsonSerializer.Serialize(_reservationService.GetReservationsByPersonId(id), J);
    }

    [KernelFunction, Description("Get a reservation by id")]
    public string GetReservationById([Description("Reservation id")] int reservationId)
        => JsonSerializer.Serialize(_reservationService.GetReservationById(reservationId), J);

    [KernelFunction, Description("Create a reservation with new person (no ID needed, will be auto-generated)")]
    public string CreateReservationWithNewPerson(
        [Description("Person's first name")] string firstName,
        [Description("Person's last name")] string lastName,
        [Description("Person's email")] string email,
        [Description("Person's phone (optional)")] string? phone,
        [Description("Hotel id")] int hotelId,
        [Description("Room id")] int roomId,
        [Description("Check-in date (yyyy-MM-dd or ISO-8601)")] string checkIn,
        [Description("Check-out date (yyyy-MM-dd or ISO-8601)")] string checkOut)
    {
        if (!TryParseDate(checkIn, out var ci) || !TryParseDate(checkOut, out var co))
            return JsonSerializer.Serialize(new { error = "Invalid date format. Please use yyyy-MM-dd format." }, J);

        if (co <= ci)
            return JsonSerializer.Serialize(new { error = "Check-out date must be after check-in date." }, J);

        var res = _reservationService.CreateReservationWithNewPerson(
            firstName, lastName, email, phone, hotelId, roomId, ci, co);

        if (res is null)
            return JsonSerializer.Serialize(new
            {
                error = "Reservation failed",
                reasons = new[] {
                    "Hotel not found",
                    "Room not found or unavailable",
                    "Room already booked for selected dates"
                }
            }, J);

        return JsonSerializer.Serialize(new
        {
            success = true,
            message = "Reservation created successfully",
            reservation = res
        }, J);
    }

    [KernelFunction, Description("Create a reservation for existing person (requires person ID)")]
    public string CreateReservation(
        [Description("Person id (GUID)")] string personId,
        [Description("Hotel id")] int hotelId,
        [Description("Room id")] int roomId,
        [Description("Check-in date (yyyy-MM-dd or ISO-8601)")] string checkIn,
        [Description("Check-out date (yyyy-MM-dd or ISO-8601)")] string checkOut)
    {
        if (!Guid.TryParse(personId, out var pid))
            return JsonSerializer.Serialize(new { error = "Invalid person ID (GUID) format" }, J);

        if (!TryParseDate(checkIn, out var ci) || !TryParseDate(checkOut, out var co))
            return JsonSerializer.Serialize(new { error = "Invalid date format. Please use yyyy-MM-dd format." }, J);

        if (co <= ci)
            return JsonSerializer.Serialize(new { error = "Check-out date must be after check-in date." }, J);

        var res = _reservationService.CreateReservation(pid, hotelId, roomId, ci, co);
        if (res is null)
            return JsonSerializer.Serialize(new
            {
                error = "Reservation failed",
                possibleReasons = new[] {
                    "Person not found",
                    "Hotel not found",
                    "Room not found or unavailable",
                    "Room already booked for selected dates"
                }
            }, J);

        return JsonSerializer.Serialize(new
        {
            success = true,
            message = "Reservation created successfully",
            reservation = res
        }, J);
    }

    [KernelFunction, Description("Cancel a reservation")]
    public string CancelReservation([Description("Reservation id")] int reservationId)
    {
        var ok = _reservationService.CancelReservation(reservationId);
        return JsonSerializer.Serialize(new
        {
            success = ok,
            message = ok ? "Reservation cancelled successfully" : "Reservation not found"
        }, J);
    }

    private static bool TryParseDate(string s, out DateTime dt) =>
        DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt);
}