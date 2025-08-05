
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ChatWithAPIDemo.Models;


namespace ChatWithAPIDemo.Controllers
{

    [ApiController]
    [Route("api")]
    public class ChatController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;
        private static readonly Dictionary<string, ChatSession> _sessions = new();

        public ChatController(Kernel kernel, IChatCompletionService chatService)
        {
            _kernel = kernel;
            _chatService = chatService;
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
            return Ok(session);
        }

        [HttpPost("chat/{sessionId}")]
        public async Task<ActionResult<string>> Chat(string sessionId, [FromBody] ChatRequest request)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return NotFound();
            }
            session.LastAccessedAt = DateTime.UtcNow;
            session.ChatHistory.AddUserMessage(request.Message);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };
            var results = await _chatService.GetChatMessageContentsAsync(
                session.ChatHistory, settings, _kernel
            );
            var response = results.FirstOrDefault()?.Content;
            session.ChatHistory.AddAssistantMessage(response);

            return Ok(response);
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
                MessageCount = session.ChatHistory.Count()
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
    }

}
