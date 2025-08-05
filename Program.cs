using ChatWithAPIDemo.ValueObjects;
using ChatWithAPIDemo.Models;
using ChatWithAPIDemo.Plugins;
using ChatWithAPIDemo.Services;
using Microsoft.AspNetCore.Builder;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

WebApplicationBuilder webAppBuilder = WebApplication.CreateBuilder(args);

                   
IKernelBuilder azureBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-4o-mini",
        endpoint: "https://ai-newchapter2-resource.cognitiveservices.azure.com/",
        apiKey: "G68kZEXqNXh23qb4aMUJD23DsfTKlkq8V649zMUpD5HiUDzhJFXRJQQJ99BGACYeBjFXJ3w3AAAAACOGhqWj"
    );


                        // Dummy Data

var hotels = new List<Hotel>
{
    new Hotel { Id = 1, Name = "Aegean Breeze",  StarRating = 4, Address = new Address { City = "Izmir",    Street = "Cumhuriyet Blv. 123" } },
    new Hotel { Id = 2, Name = "Golden Horn Inn", StarRating = 5, Address = new Address { City = "Istanbul", Street = "Istiklal Cd. 45"     } },
    new Hotel { Id = 3, Name = "Anatolia Suites", StarRating = 3, Address = new Address { City = "Ankara",   Street = "Ataturk Blv. 210"    } }
};

var persons = new List<Person>
{
    new Person { Id = 1, FirstName = "Ahmet", LastName = "Yýlmaz", Email = "ahmet@email.com", Phone = "+905551234567", LoyaltyPoints = 100 },
    new Person { Id =2, FirstName = "Elif", LastName = "Kaya", Email = "elif@email.com", Phone = "+905559876543", LoyaltyPoints = 250 },
    new Person { Id =3, FirstName = "Mehmet", LastName = "Demir", Email = "mehmet@email.com", Phone = "+905551112233", LoyaltyPoints = 75 },
    new Person { Id = 4, FirstName = "Zeynep", LastName = "Özkan", Email = "zeynep@email.com", Phone = "+905554445566", LoyaltyPoints = 320 }
};

var rooms = new List<Room>
{
    // Aegean Breeze (Hotel ID: 1) Rooms
    new Room { Id = 1, RoomNumber = "101", Floor = 1, Capacity = 2, IsSeaView = true, IsSmokingAllowed = false, IsAvailable = true, Price = 450m, Hotel = hotels[0] },
    new Room { Id = 2, RoomNumber = "102", Floor = 1, Capacity = 3, IsSeaView = true, IsSmokingAllowed = false, IsAvailable = true, Price = 550m, Hotel = hotels[0]},
    new Room { Id = 3, RoomNumber = "201", Floor = 2, Capacity = 2, IsSeaView = false, IsSmokingAllowed = true, IsAvailable = true, Price = 350m, Hotel = hotels[0] },
    
    // Golden Horn Inn (Hotel ID: 2) Rooms
    new Room { Id = 4, RoomNumber = "301", Floor = 3, Capacity = 2, IsSeaView = true, IsSmokingAllowed = false, IsAvailable = true, Price = 750m, Hotel = hotels[1] },
    new Room { Id = 5, RoomNumber = "302", Floor = 3, Capacity = 4, IsSeaView = true, IsSmokingAllowed = false, IsAvailable = true, Price = 950m,Hotel = hotels[1] },
    new Room { Id = 6, RoomNumber = "401", Floor = 4, Capacity = 2, IsSeaView = false, IsSmokingAllowed = false, IsAvailable = true, Price = 650m ,Hotel = hotels[1]},
    
    // Anatolia Suites (Hotel ID: 3) Rooms
    new Room { Id = 7, RoomNumber = "101", Floor = 1, Capacity = 2, IsSeaView = false, IsSmokingAllowed = true, IsAvailable = true, Price = 250m,Hotel = hotels[2] },
    new Room { Id = 8, RoomNumber = "102", Floor = 1, Capacity = 3, IsSeaView = false, IsSmokingAllowed = false, IsAvailable = true, Price = 300m ,Hotel = hotels[2]},
    new Room { Id = 9, RoomNumber = "201", Floor = 2, Capacity = 1, IsSeaView = false, IsSmokingAllowed = false, IsAvailable = false, Price = 200m, Hotel = hotels[2] }
};


var roomAvailabilities = new List<RoomAvailability>
{
    // Room 1 - Blocked for maintenance
    new RoomAvailability
    {
        Id = 1,
        Room = rooms.First(r => r.Id == 1),
        AvailabilitySlot = new AvailabilitySlot
        {
            Start = DateTime.Now.AddDays(5),
            End = DateTime.Now.AddDays(7),
            Status = AvailabilityStatus.OutOfService,
            Note = "Maintenance work"
        }
    },
    
    // Room 4 - Reserved
    new RoomAvailability
    {
        Id = 2,
        Room = rooms.First(r => r.Id == 4),
        AvailabilitySlot = new AvailabilitySlot
        {
            Start = DateTime.Now.AddDays(10),
            End = DateTime.Now.AddDays(15),
            Status = AvailabilityStatus.Reserved,
            Note = "Guest reservation"
        }
    },
    
    // Room 9 - Completely blocked
    new RoomAvailability
    {
        Id = 3,
        Room = rooms.First(r => r.Id == 9),
        AvailabilitySlot = new AvailabilitySlot
        {
            Start = DateTime.Now.AddDays(-30),
            End = DateTime.Now.AddDays(60),
            Status = AvailabilityStatus.Blocked,
            Note = "Long-term renovation"
        }
    }
};




var reviews = new List<Review>
{
    new Review
    {
        Id = 1,
        Rating = 5,
        Comment = "Harika bir deneyimdi! Deniz manzarasý muhteþem.",
        Person = persons[0],
        Hotel = hotels[0]
    },
    new Review
    {
        Id = 2,
        Rating = 4,
        Comment = "Temiz ve konforlu. Personel çok yardýmcý.",
        Person = persons[1],
        Hotel = hotels[1]
    },
    new Review
    {
        Id = 3,
        Rating = 3,
        Comment = "Fiyat performans olarak iyi ama biraz gürültülü.",
        Person = persons[2],
        Hotel = hotels[2]
    },
    new Review
    {
        Id = 4,
        Rating = 5,
        Comment = "Mükemmel hizmet ve lokasyon!",
        Person = persons[3],
        Hotel = hotels[1]
    }
};

// Reservations
var reservations = new List<Reservation>
{
    new Reservation
    {
        Id = 1,
        Person = persons[0],
        Hotel = hotels[0],
        Room = rooms[0],
        CheckIn = DateTime.Now.AddDays(-5),
        CheckOut = DateTime.Now.AddDays(-2),
        TotalPrice = 1350m // 3 gün * 450
    },
    new Reservation
    {
        Id =2,
        Person = persons[1],
        Hotel = hotels[1],
        Room = rooms[3],
        CheckIn = DateTime.Now.AddDays(20),
        CheckOut = DateTime.Now.AddDays(25),
        TotalPrice = 3750m // 5 gün * 750
    }
};















// ---- App services ----

var hotelService = new HotelService(hotels);
var personService = new PersonService(persons);
var roomService = new RoomService(rooms, roomAvailabilities);
var reviewService = new ReviewService(reviews, personService, hotelService);
var reservationService = new ReservationService(reservations, roomService, personService, hotelService);
// Attach plugin that depends on HotelService

azureBuilder.Plugins.AddFromObject(new HotelPlugin(hotelService));
azureBuilder.Plugins.AddFromObject(new PPersonPlugin(personService));
azureBuilder.Plugins.AddFromObject(new ReservationPlugin(reservationService));





Kernel azureKernel = azureBuilder.Build();

// Dependency Injection
//webAppBuilder.Services.AddSingleton<Kernel>(azureKernel);
//webAppBuilder.Services.AddSingleton(hotelService);
//webAppBuilder.Services.AddSingleton(personService);
webAppBuilder.Services.AddSingleton<Kernel>(azureKernel);
webAppBuilder.Services.AddSingleton(hotelService);
webAppBuilder.Services.AddSingleton(personService);
webAppBuilder.Services.AddSingleton(roomService);
webAppBuilder.Services.AddSingleton(reviewService);
webAppBuilder.Services.AddSingleton(reservationService);

IChatCompletionService chatCompletionService = azureKernel.GetRequiredService<IChatCompletionService>();
webAppBuilder.Services.AddSingleton<IChatCompletionService>(chatCompletionService);
    
   

webAppBuilder.Services.AddControllers();
webAppBuilder.Services.AddEndpointsApiExplorer();
webAppBuilder.Services.AddSwaggerGen();

WebApplication app = webAppBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
