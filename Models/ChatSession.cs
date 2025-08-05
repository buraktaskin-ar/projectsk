using Microsoft.SemanticKernel.ChatCompletion;

namespace ChatWithAPIDemo.Models
{

    public class ChatSession
    {
        public string Id { get; set; } = string.Empty;
        public ChatHistory ChatHistory { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }
    }
}
