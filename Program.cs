using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using ChatWithAPIDemo.Configuration;
using ChatWithAPIDemo.Models;
using ChatWithAPIDemo.Plugins;
using ChatWithAPIDemo.Services;
using ChatWithAPIDemo.Services.Search;
using ChatWithAPIDemo.ValueObjects;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load Azure Search configuration
var azureSearchConfig = AzureSearchConfiguration.LoadFromConfiguration(builder.Configuration);
builder.Services.AddSingleton(azureSearchConfig);

// In-memory data stores
var persons = new List<Person>();
var hotels = new List<Hotel>
{
    new Hotel
    {
        Id = 1,
        Name = "Grand Hotel Istanbul",
        StarRating = 5,
        Address = new Address { City = "Istanbul", Street = "Taksim Square", Country = "TR" }
    },
    new Hotel
    {
        Id = 2,
        Name = "Ankara Palace Hotel",
        StarRating = 4,
        Address = new Address { City = "Ankara", Street = "Kızılay", Country = "TR" }
    },
    new Hotel
    {
        Id = 3,
        Name = "Izmir Beach Resort",
        StarRating = 5,
        Address = new Address { City = "Izmir", Street = "Kordon", Country = "TR" }
    }
};

var rooms = new List<Room>
{
    new Room { Id = 1, RoomNumber = "101", Floor = 1, Hotel = hotels[0], Capacity = 2, IsSeaView = false, RoomType = RoomType.Standard, Price = 500 },
    new Room { Id = 2, RoomNumber = "201", Floor = 2, Hotel = hotels[0], Capacity = 3, IsSeaView = true, RoomType = RoomType.Deluxe, Price = 750 },
    new Room { Id = 3, RoomNumber = "301", Floor = 3, Hotel = hotels[0], Capacity = 4, IsSeaView = true, RoomType = RoomType.Suite, Price = 1000 },
    new Room { Id = 4, RoomNumber = "102", Floor = 1, Hotel = hotels[1], Capacity = 2, IsSeaView = false, RoomType = RoomType.Standard, Price = 400 },
    new Room { Id = 5, RoomNumber = "202", Floor = 2, Hotel = hotels[1], Capacity = 3, IsSeaView = false, RoomType = RoomType.Superior, Price = 600 },
    new Room { Id = 6, RoomNumber = "101", Floor = 1, Hotel = hotels[2], Capacity = 2, IsSeaView = true, RoomType = RoomType.Deluxe, Price = 800 },
    new Room { Id = 7, RoomNumber = "201", Floor = 2, Hotel = hotels[2], Capacity = 4, IsSeaView = true, RoomType = RoomType.Suite, Price = 1200 }
};

var roomAvailabilities = new List<RoomAvailability>();
var reservations = new List<Reservation>();
var reviews = new List<Review>();

var speakers = new List<SpeakerModel>
{
    new SpeakerModel { Id = 1, Name = "Living Room Speaker", VolumeLevel = 50, IsMuted = false, BatteryLevel = 100 },
    new SpeakerModel { Id = 2, Name = "Bedroom Speaker", VolumeLevel = 30, IsMuted = false, BatteryLevel = 75 },
    new SpeakerModel { Id = 3, Name = "Kitchen Speaker", VolumeLevel = 60, IsMuted = true, BatteryLevel = 50 }
};

// Register services as singletons
builder.Services.AddSingleton(persons);
builder.Services.AddSingleton(hotels);
builder.Services.AddSingleton(rooms);
builder.Services.AddSingleton(roomAvailabilities);
builder.Services.AddSingleton(reservations);
builder.Services.AddSingleton(reviews);
builder.Services.AddSingleton(speakers);

// Register business services
builder.Services.AddSingleton<PersonService>();
builder.Services.AddSingleton<HotelService>();
builder.Services.AddSingleton<RoomService>();
builder.Services.AddSingleton<ReservationService>();
builder.Services.AddSingleton<ReviewService>();

// Register plugins
builder.Services.AddSingleton<PPersonPlugin>();
builder.Services.AddSingleton<HotelPlugin>();
builder.Services.AddSingleton<RoomPlugin>();
builder.Services.AddSingleton<ReservationPlugin>();
builder.Services.AddSingleton<ReviewPlugin>();
builder.Services.AddSingleton<SpeakerPlugin>();

// Azure Search Services (conditional registration)
if (azureSearchConfig.IsConfigured())
{
    // Register Azure Search clients
    builder.Services.AddSingleton<SearchIndexClient>(sp =>
        new SearchIndexClient(
            new Uri(azureSearchConfig.SearchEndpoint),
            new AzureKeyCredential(azureSearchConfig.SearchKey)
        )
    );

    builder.Services.AddSingleton<SearchClient>(sp =>
        new SearchClient(
            new Uri(azureSearchConfig.SearchEndpoint),
            azureSearchConfig.IndexName,
            new AzureKeyCredential(azureSearchConfig.SearchKey)
        )
    );

    builder.Services.AddSingleton<AzureSearchService>();
    builder.Services.AddSingleton<AIChatService>();

    Console.WriteLine("✅ Azure AI Search configured successfully");
}
else
{
    // Register null services when Azure Search is not configured
    builder.Services.AddSingleton<SearchClient?>(sp => null);
    builder.Services.AddSingleton<AzureSearchService>(sp => new AzureSearchService(null, null));
    builder.Services.AddSingleton<AIChatService>(sp => new AIChatService(null, null, null));

    Console.WriteLine("⚠️ Azure AI Search not configured - using standard features only");
}

// Semantic Kernel setup
builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    // Configure OpenAI or Azure OpenAI based on configuration
    if (azureSearchConfig.IsConfigured())
    {
        // Azure OpenAI configuration for both chat and embeddings
        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: azureSearchConfig.ChatDeploymentName,
            endpoint: azureSearchConfig.AzureOpenAIEndpoint,
            apiKey: azureSearchConfig.AzureOpenAIKey);

        kernelBuilder.AddAzureOpenAITextEmbeddingGeneration(
            deploymentName: azureSearchConfig.EmbeddingDeploymentName,
            endpoint: azureSearchConfig.AzureOpenAIEndpoint,
            apiKey: azureSearchConfig.AzureOpenAIKey);
    }
    else
    {
        // Standard OpenAI configuration (update with your settings)
        var openAiKey = builder.Configuration["OpenAI:ApiKey"];
        if (!string.IsNullOrEmpty(openAiKey))
        {
            kernelBuilder.AddOpenAIChatCompletion("gpt-4", openAiKey);
        }
    }

    // Register plugins
    kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<PPersonPlugin>(), "PersonPlugin");
    kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<HotelPlugin>(), "HotelPlugin");
    kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<RoomPlugin>(), "RoomPlugin");
    kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<ReservationPlugin>(), "ReservationPlugin");
    kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<ReviewPlugin>(), "ReviewPlugin");
    kernelBuilder.Plugins.AddFromObject(sp.GetRequiredService<SpeakerPlugin>(), "SpeakerPlugin");

    return kernelBuilder.Build();
});

builder.Services.AddSingleton<IChatCompletionService>(sp =>
{
    var kernel = sp.GetRequiredService<Kernel>();
    try
    {
        return kernel.GetRequiredService<IChatCompletionService>();
    }
    catch
    {
        // Return a null implementation or mock if not configured
        throw new InvalidOperationException("Chat completion service is not configured. Please configure OpenAI or Azure OpenAI.");
    }
});

// Optional: Register embedding service if configured
builder.Services.AddSingleton<ITextEmbeddingGenerationService?>(sp =>
{
    if (azureSearchConfig.IsConfigured())
    {
        var kernel = sp.GetRequiredService<Kernel>();
        try
        {
            return kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        }
        catch
        {
            return null;
        }
    }
    return null;
});

var app = builder.Build();

// Initialize Azure Search Index if configured
if (azureSearchConfig.IsConfigured())
{
    var scope = app.Services.CreateScope();
    var searchIndexClient = scope.ServiceProvider.GetService<SearchIndexClient>();
    var searchClient = scope.ServiceProvider.GetService<SearchClient>();
    var embeddingService = scope.ServiceProvider.GetService<ITextEmbeddingGenerationService>();

    if (searchIndexClient != null && searchClient != null && embeddingService != null)
    {
        try
        {
            // You can optionally initialize the index here
            Console.WriteLine("Azure Search index ready for use");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not initialize Azure Search index: {ex.Message}");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();