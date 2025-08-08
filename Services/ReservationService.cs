using ChatWithAPIDemo.Models;

namespace ChatWithAPIDemo.Services;

public class ReservationService
{
    private readonly List<Reservation> _reservations;
    private readonly RoomService _roomService;
    private readonly PersonService _personService;
    private readonly HotelService _hotelService;

    public ReservationService(List<Reservation> reservations, RoomService roomService, PersonService personService, HotelService hotelService)
    {
        _reservations = reservations;
        _roomService = roomService;
        _personService = personService;
        _hotelService = hotelService;
    }

    public List<Reservation> GetAllReservations() => _reservations;

    public List<Reservation> GetReservationsByPersonId(Guid personId)
    {
        return _reservations.Where(r => r.Person.Id == personId).ToList();
    }

    public List<Reservation> GetReservationsByHotelId(int hotelId)
    {
        return _reservations.Where(r => r.Hotel.Id == hotelId).ToList();
    }

    public Reservation? GetReservationById(int id)
    {
        return _reservations.FirstOrDefault(r => r.Id == id);
    }

    // Yeni metod: Person bilgileri ile rezervasyon oluşturma
    public Reservation? CreateReservationWithNewPerson(
        string firstName,
        string lastName,
        string email,
        string? phone,
        int hotelId,
        int roomId,
        DateTime checkIn,
        DateTime checkOut)
    {
        // Önce kişiyi oluştur veya bul
        var existingPerson = _personService.FindPersonByEmail(email);
        Person person;

        if (existingPerson == null)
        {
            // Yeni kişi oluştur
            person = _personService.CreatePerson(firstName, lastName, email, phone);
        }
        else
        {
            person = existingPerson;
        }

        // Rezervasyonu oluştur
        return CreateReservation(person.Id, hotelId, roomId, checkIn, checkOut);
    }

    public Reservation? CreateReservation(Guid personId, int hotelId, int roomId, DateTime checkIn, DateTime checkOut)
    {
        // Kişi kontrolü
        var person = _personService.FindPersonById(personId);
        if (person == null)
            return null;

        // Otel kontrolü
        var hotel = _hotelService.GetHotelById(hotelId);
        if (hotel == null)
            return null;

        // Oda kontrolü
        var room = _roomService.GetRoomById(roomId);
        if (room == null)//|| !room.IsAvailable
            return null;

        // Oda müsaitlik kontrolü
        if (!_roomService.IsRoomAvailable(roomId, checkIn, checkOut))
            return null;

        // Fiyat hesaplama
        var totalDays = (checkOut - checkIn).Days;
        if (totalDays <= 0) totalDays = 1; // En az 1 gün
        var totalPrice = room.Price * totalDays;

        var reservation = new Reservation
        {
            Id = _reservations.Count > 0 ? _reservations.Max(r => r.Id) + 1 : 1,
            Person = person,
            Hotel = hotel,
            Room = room,
            CheckIn = checkIn,
            CheckOut = checkOut,
            TotalPrice = totalPrice
        };

        _reservations.Add(reservation);

        // Oda müsaitliğini güncelle
        _roomService.BlockRoomAvailability(roomId, checkIn, checkOut, $"Reserved for {person.FirstName} {person.LastName}");

        // Loyalty points ekle
        _personService.AddLoyaltyPoints(personId, (int)(totalPrice / 10)); // Her 10 TL için 1 puan

        return reservation;
    }

    public bool CancelReservation(int reservationId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation == null)
            return false;

        // Oda müsaitliğini serbest bırak
        _roomService.FreeRoomAvailability(reservation.Room.Id, reservation.CheckIn, reservation.CheckOut);

        _reservations.Remove(reservation);
        return true;
    }
}