using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ChatWithAPIDemo.Models;
using ChatWithAPIDemo.Services.Search;

namespace ChatWithAPIDemo.Controllers
{
    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private readonly AIChatService? _aiChatService;
        private readonly AzureSearchService? _azureSearchService;
        private static readonly Dictionary<string, ChatSession> _sessions = new();

        public ChatController(
            Kernel kernel,
            IChatCompletionService chatService,
            AIChatService? aiChatService = null,
            AzureSearchService? azureSearchService = null)
        {
            _kernel = kernel;
            _chatService = chatService;
            _aiChatService = aiChatService;
            _azureSearchService = azureSearchService;
        }

        [HttpPost("start-session")]
        public ActionResult<ChatSession> StartSession()
        {
            var session = new ChatSession
            {
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                ChatHistory = new ChatHistory()
            };
            _sessions[session.Id] = session;

            var response = new
            {
                session.Id,
                session.CreatedAt,
                session.LastAccessedAt,
                AISearchEnabled = _aiChatService?.IsConfigured ?? false,
                Message = _aiChatService?.IsConfigured == true
                    ? "Session started with AI Search capabilities"
                    : "Session started (AI Search not configured - using standard features)"
            };

            return Ok(response);
        }

        [HttpPost("chat/{sessionId}")]
        public async Task<ActionResult<string>> Chat(string sessionId, [FromBody] ChatRequest request)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return NotFound("Session not found");
            }

            session.LastAccessedAt = DateTime.UtcNow;
            session.ChatHistory.AddUserMessage(request.Message);

            string response;

            // Check if the message is asking for hotel search/recommendations and AI is configured
            if (_aiChatService?.IsConfigured == true && IsHotelSearchQuery(request.Message))
            {
                // Use AI Search for hotel-related queries
                response = await _aiChatService.ProcessQueryAsync(request.Message, _kernel);
            }
            else
            {
                // Use standard chat with function calling for other queries
                var settings = new OpenAIPromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };

                var results = await _chatService.GetChatMessageContentsAsync(
                    session.ChatHistory, settings, _kernel
                );
                response = results.FirstOrDefault()?.Content ?? "I couldn't process your request.";
            }

            session.ChatHistory.AddAssistantMessage(response);
            return Ok(response);
        }

        [HttpPost("search/hotels")]
        public async Task<ActionResult> SearchHotels([FromBody] HotelSearchRequest request)
        {
            if (_azureSearchService?.IsConfigured != true)
            {
                return BadRequest("AI Search is not configured");
            }

            try
            {
                if (request.SearchType == "semantic")
                {
                    var results = await _azureSearchService.SearchSemanticAsync(request.Query);
                    var hotels = new List<object>();
                    await foreach (var result in results.GetResultsAsync())
                    {
                        hotels.Add(new
                        {
                            result.Document.HotelName,
                            result.Document.City,
                            result.Document.Country,
                            result.Document.StarRating,
                            result.Document.PricePerNight,
                            result.Document.Description
                        });
                    }
                    return Ok(hotels);
                }
                else if (request.SearchType == "amenities" && request.MaxPrice.HasValue)
                {
                    var results = await _azureSearchService.SearchWithAmenitiesFilterAsync(request.MaxPrice.Value);
                    var hotels = new List<object>();
                    await foreach (var result in results.GetResultsAsync())
                    {
                        hotels.Add(new
                        {
                            result.Document.HotelName,
                            result.Document.City,
                            result.Document.PricePerNight,
                            result.Document.HasGym,
                            result.Document.HasPool
                        });
                    }
                    return Ok(hotels);
                }
                else
                {
                    return BadRequest("Invalid search parameters");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Search error: {ex.Message}");
            }
        }

        [HttpGet("session/{sessionId}")]
        public ActionResult<object> GetSession(string sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return NotFound();
            }
            return Ok(new
            {
                session.Id,
                session.CreatedAt,
                session.LastAccessedAt,
                MessageCount = session.ChatHistory.Count(),
                AISearchEnabled = _aiChatService?.IsConfigured ?? false
            });
        }

        [HttpGet("session/{sessionId}/history")]
        public ActionResult<ChatHistory> GetHistory(string sessionId)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return NotFound();
            }
            return Ok(session.ChatHistory);
        }

        [HttpDelete("session/{sessionId}")]
        public ActionResult DeleteSession(string sessionId)
        {
            if (_sessions.Remove(sessionId))
            {
                return Ok("Session deleted");
            }
            return NotFound();
        }

        [HttpGet("ai-status")]
        public ActionResult GetAIStatus()
        {
            return Ok(new
            {
                AISearchEnabled = _aiChatService?.IsConfigured ?? false,
                SemanticSearchEnabled = _azureSearchService?.IsConfigured ?? false,
                Message = _aiChatService?.IsConfigured == true
                    ? "AI Search is fully configured and operational"
                    : "AI Search is not configured - using standard features only"
            });
        }

        private bool IsHotelSearchQuery(string message)
        {
            var keywords = new[] {
                "hotel", "find", "search", "recommend", "suggestion", "looking for",
                "pool", "gym", "spa", "amenities", "cancellation", "pet", "wifi",
                "price", "cheap", "luxury", "budget", "family", "business"
            };

            var messageLower = message.ToLower();
            return keywords.Any(keyword => messageLower.Contains(keyword));
        }
    }

    public class HotelSearchRequest
    {
        public string Query { get; set; } = string.Empty;
        public string SearchType { get; set; } = "semantic"; // "semantic" or "amenities"
        public double? MaxPrice { get; set; }
    }
}