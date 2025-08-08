using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ChatWithAPIDemo.Models.Search;

namespace ChatWithAPIDemo.Services.Search
{
    public class AIChatService
    {
        private readonly SearchClient? _searchClient;
        private readonly IChatCompletionService? _chatCompletionService;
        private readonly AzureSearchService? _searchService;
        private readonly ChatHistory _chatHistory;
        private readonly OpenAIPromptExecutionSettings _executionSettings;
        private readonly bool _isConfigured;

        public AIChatService(
            SearchClient? searchClient,
            Kernel? kernel,
            AzureSearchService? searchService)
        {
            _searchClient = searchClient;
            _searchService = searchService;

            if (kernel != null)
            {
                try
                {
                    _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
                }
                catch
                {
                    _chatCompletionService = null;
                }
            }

            _isConfigured = _searchClient != null && _chatCompletionService != null && _searchService != null;
            _chatHistory = InitializeChatHistory();
            _executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.3,
                MaxTokens = 500
            };
        }

        public bool IsConfigured => _isConfigured;

        private ChatHistory InitializeChatHistory()
        {
            var history = new ChatHistory();
            history.AddSystemMessage(
                "You are a helpful hotel reservation assistant. Use the provided context to answer questions about hotels, " +
                "their amenities, policies, and availability. Always mention the hotel name and relevant details. " +
                "If asked about specific features, clearly state which hotels have them. Keep responses informative but concise. " +
                "You can also help with room reservations using the available functions."
            );
            return history;
        }

        public async Task<string> ProcessQueryAsync(string userInput, Kernel kernel)
        {
            if (!_isConfigured || _searchService == null || _searchClient == null || _chatCompletionService == null)
            {
                return "AI Search is not configured. Please use standard reservation features.";
            }

            try
            {
                var searchOptions = await _searchService.BuildSmartSearchOptionsAsync(userInput);

                var searchResponse = await _searchClient.SearchAsync<HotelSearchDocument>(
                    searchOptions.UseTextSearch ? userInput : null,
                    searchOptions.Options);

                var context = await BuildContextAsync(searchResponse);

                var userMessage = $@"User Query: {userInput}

Hotel Information:
{context}";

                _chatHistory.AddUserMessage(userMessage);

                var response = await _chatCompletionService.GetChatMessageContentsAsync(
                    _chatHistory,
                    _executionSettings,
                    kernel);

                var responseContent = response[0].Content ?? "I couldn't process your request.";
                _chatHistory.AddAssistantMessage(responseContent);

                return responseContent;
            }
            catch (Exception ex)
            {
                return $"I encountered an error processing your request. Please try again or use standard search features.";
            }
        }

        private async Task<string> BuildContextAsync(SearchResults<HotelSearchDocument> searchResponse)
        {
            var contextSnippets = new List<string>();

            await foreach (var hit in searchResponse.GetResultsAsync())
            {
                var hotel = hit.Document;
                var snippet = FormatHotelInfo(hotel);
                contextSnippets.Add(snippet);
            }

            return contextSnippets.Count > 0
                ? string.Join("\n---\n", contextSnippets)
                : "No hotels found matching your criteria.";
        }

        private string FormatHotelInfo(HotelSearchDocument hotel)
        {
            return $"Hotel: {hotel.HotelName} ({hotel.City}, {hotel.Country})\n" +
                   $"Rating: {hotel.StarRating}★ | Price: ${hotel.PricePerNight}/night\n" +
                   $"Amenities: {hotel.Amenities}\n" +
                   $"Cancellation: {hotel.CancellationPolicy}\n" +
                   $"Features: Pool={hotel.HasPool}, Gym={hotel.HasGym}, Spa={hotel.HasSpa}, " +
                   $"Parking={hotel.HasParking}, WiFi={hotel.HasWifi}, Pet-friendly={hotel.PetFriendly}";
        }

        public void ClearHistory()
        {
            _chatHistory.Clear();
            _chatHistory.AddSystemMessage(
                "You are a helpful hotel reservation assistant. Use the provided context to answer questions about hotels, " +
                "their amenities, policies, and availability. Always mention the hotel name and relevant details. " +
                "If asked about specific features, clearly state which hotels have them. Keep responses informative but concise."
            );
        }
    }
}