namespace ChatWithAPIDemo.Models
{
    public class ChatRequest
    {
        public string Message { get; set; }

        public ChatRequest() { }

        public ChatRequest(string message)
        {
            Message = message;
        }
    }
}
