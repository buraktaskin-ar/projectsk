namespace ChatWithAPIDemo.Configuration;

public class AzureSearchConfiguration
{
    public string SearchEndpoint { get; set; } = string.Empty;
    public string SearchKey { get; set; } = string.Empty;
    public string AzureOpenAIEndpoint { get; set; } = string.Empty;
    public string AzureOpenAIKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = "hotels-hybrid-index";
    public string EmbeddingDeploymentName { get; set; } = "text-embedding-ada-002";
    public string ChatDeploymentName { get; set; } = "gpt-4o-mini";

    public static AzureSearchConfiguration LoadFromConfiguration(IConfiguration configuration)
    {
        var config = new AzureSearchConfiguration();
        configuration.GetSection("AzureSearch").Bind(config);

        // Also check environment variables as fallback
        config.SearchEndpoint = config.SearchEndpoint ?? Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT") ?? "";
        config.SearchKey = config.SearchKey ?? Environment.GetEnvironmentVariable("AZURE_SEARCH_ADMIN_KEY") ?? "";
        config.AzureOpenAIEndpoint = config.AzureOpenAIEndpoint ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "";
        config.AzureOpenAIKey = config.AzureOpenAIKey ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "";

        return config;
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(SearchEndpoint) &&
               !string.IsNullOrEmpty(SearchKey) &&
               !string.IsNullOrEmpty(AzureOpenAIEndpoint) &&
               !string.IsNullOrEmpty(AzureOpenAIKey);
    }
}