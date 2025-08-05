// ChatWithAPIDemo/Services/InMemoryDataStore.cs
using System.Collections.Concurrent;
using ChatWithAPIDemo.Models;
using ChatWithAPIDemo.ValueObjects;

namespace ChatWithAPIDemo.Services;

public class InMemoryDataStore
{
    public  List<Hotel> Hotels { get; } = new();
    public List<Room> Rooms { get; } = new();
    public List<Person> People { get; } = new();
    public List<Reservation> Reservations { get; } = new();
    public List<Review> Reviews { get; } = new();
    public List<RoomAvailability> RoomAvailabilities { get; } = new();

    // Room -> Hotel mapping (since Room has no HotelId)
    public Dictionary<int, int> RoomHotelMap { get; } = new();


   

    public InMemoryDataStore()
    {
        // ---- Dummy data ----
        var h1 = new Hotel { Id = 1, Name = "Aegean Breeze", StarRating = 4, Address = new Address() };
        var h2 = new Hotel { Id = 2, Name = "Bosporus View", StarRating = 5, Address = new Address() };
        Hotels.AddRange([h1, h2]);

        var r101 = new Room { Id = 101, RoomNumber = "101", Floor = 1, Capacity = 2, IsSeaView = true, IsSmokingAllowed = false, Price = 150m, IsAvailable = true ,Hotel = h1};
        var r102 = new Room { Id = 102, RoomNumber = "102", Floor = 1, Capacity = 3, IsSeaView = false, IsSmokingAllowed = false, Price = 180m, IsAvailable = true , Hotel = h1 };
        var r201 = new Room { Id = 201, RoomNumber = "201", Floor = 2, Capacity = 2, IsSeaView = true, IsSmokingAllowed = false, Price = 220m, IsAvailable = true,Hotel = h2 };
        var r202 = new Room { Id = 202, RoomNumber = "202", Floor = 2, Capacity = 4, IsSeaView = false, IsSmokingAllowed = true, Price = 250m, IsAvailable = true , Hotel = h2 };
        Rooms.AddRange([r101, r102, r201, r202]);

        RoomHotelMap[r101.Id] = h1.Id;
        RoomHotelMap[r102.Id] = h1.Id;
        RoomHotelMap[r201.Id] = h2.Id;
        RoomHotelMap[r202.Id] = h2.Id;

        var p1 = new Person { FirstName = "Burak", LastName = "Taşkın", Email = "burak@example.com", Phone = "555-111-22-33", LoyaltyPoints = 120 };
        var p2 = new Person { FirstName = "Feyza", LastName = "Taşkın", Email = "feyza@example.com", Phone = "555-444-55-66", LoyaltyPoints = 50 };
        People.AddRange([p1, p2]);

        var today = DateTime.Today;
        // Availability: 10 days open for r101, r201; r102 is blocked for next 3 days; r202 OOS this weekend
        RoomAvailabilities.AddRange(new[]
        {
            new RoomAvailability { Room = r101, AvailabilitySlot = new AvailabilitySlot { Start = today, End = today.AddDays(10), Status = AvailabilityStatus.Available, Note="open range" } },
            new RoomAvailability { Room = r201, AvailabilitySlot = new AvailabilitySlot { Start = today, End = today.AddDays(10), Status = AvailabilityStatus.Available, Note="open range" } },

            new RoomAvailability { Room = r102, AvailabilitySlot = new AvailabilitySlot { Start = today, End = today.AddDays(3), Status = AvailabilityStatus.Blocked, Note="maintenance" } },
            new RoomAvailability { Room = r202, AvailabilitySlot = new AvailabilitySlot { Start = NextSaturday(today), End = NextSaturday(today).AddDays(2), Status = AvailabilityStatus.OutOfService, Note="deep clean" } },
        });

        // A couple of reviews
        Reviews.AddRange(new[]
        {
            new Review { Id = 1, Hotel = h1, Person = p1, Rating = 5, Comment = "Clean rooms, friendly staff." },
            new Review { Id = 2, Hotel = h1, Person = p2, Rating = 4, Comment = "Great location." },
            new Review { Id = 3, Hotel = h2, Person = p1, Rating = 5, Comment = "Amazing view." },
        });
    }

    private static DateTime NextSaturday(DateTime from) =>
        from.AddDays(((int)DayOfWeek.Saturday - (int)from.DayOfWeek + 7) % 7);
}

internal static class DateRange
{
    // Treats ranges as [start, end)
    public static bool Overlaps(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        => aStart < bEnd && bStart < aEnd;
    public static int Nights(DateTime checkIn, DateTime checkOut)
        => (checkOut.Date - checkIn.Date).Days;
}
