using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel.Embeddings;
using ChatWithAPIDemo.Models.Search;
using System.Text.RegularExpressions;

namespace ChatWithAPIDemo.Services.Search
{
    public class AzureSearchService
    {
        private readonly SearchClient? _searchClient;
        private readonly ITextEmbeddingGenerationService? _embeddingService;
        private readonly bool _isConfigured;

        public AzureSearchService(SearchClient? searchClient, ITextEmbeddingGenerationService? embeddingService)
        {
            _searchClient = searchClient;
            _embeddingService = embeddingService;
            _isConfigured = searchClient != null && embeddingService != null;
        }

        public bool IsConfigured => _isConfigured;

        public async Task<SearchResults<HotelSearchDocument>> SearchCancellationPolicyAsync(string query)
        {
            if (!_isConfigured || _searchClient == null || _embeddingService == null)
                throw new InvalidOperationException("Azure Search is not configured");

            var embedding = await _embeddingService.GenerateEmbeddingAsync(query);

            var options = new SearchOptions
            {
                Size = 3,
                QueryType = SearchQueryType.Full,
                SearchFields = { nameof(HotelSearchDocument.CancellationPolicy) },
                VectorSearch = new VectorSearchOptions
                {
                    Queries = {
                        new VectorizedQuery(embedding.ToArray())
                        {
                            KNearestNeighborsCount = 3,
                            Fields = { nameof(HotelSearchDocument.DescriptionVector) },
                            Weight = 0.3f
                        }
                    }
                }
            };

            return await _searchClient.SearchAsync<HotelSearchDocument>(query, options);
        }

        public async Task<SearchResults<HotelSearchDocument>> SearchWithAmenitiesFilterAsync(double maxPrice)
        {
            if (!_isConfigured || _searchClient == null)
                throw new InvalidOperationException("Azure Search is not configured");

            var filter = $"{nameof(HotelSearchDocument.HasGym)} eq true and " +
                        $"{nameof(HotelSearchDocument.HasPool)} eq true and " +
                        $"{nameof(HotelSearchDocument.PricePerNight)} le {maxPrice}";

            var options = new SearchOptions
            {
                Size = 5,
                Filter = filter,
                OrderBy = { $"{nameof(HotelSearchDocument.PricePerNight)} asc" }
            };

            return await _searchClient.SearchAsync<HotelSearchDocument>("*", options);
        }

        public async Task<SearchResults<HotelSearchDocument>> SearchSemanticAsync(string query)
        {
            if (!_isConfigured || _searchClient == null || _embeddingService == null)
                throw new InvalidOperationException("Azure Search is not configured");

            var embedding = await _embeddingService.GenerateEmbeddingAsync(query);

            var options = new SearchOptions
            {
                Size = 3,
                VectorSearch = new VectorSearchOptions
                {
                    Queries = {
                        new VectorizedQuery(embedding.ToArray())
                        {
                            KNearestNeighborsCount = 3,
                            Fields = {
                                nameof(HotelSearchDocument.DescriptionVector),
                                nameof(HotelSearchDocument.AmenitiesVector)
                            }
                        }
                    }
                }
            };

            return await _searchClient.SearchAsync<HotelSearchDocument>(null, options);
        }

        public async Task<(SearchOptions Options, bool UseTextSearch)> BuildSmartSearchOptionsAsync(string query)
        {
            var options = new SearchOptions { Size = 5 };
            var useTextSearch = true;

            DetectPriceFilter(query, options);

            var amenityFilters = DetectAmenityFilters(query);
            if (amenityFilters.Any())
            {
                CombineFilters(options, string.Join(" and ", amenityFilters));
            }

            if (query.ToLower().Contains("in "))
            {
                options.SearchFields.Add(nameof(HotelSearchDocument.City));
                options.SearchFields.Add(nameof(HotelSearchDocument.Country));
            }

            if (_embeddingService != null)
            {
                await AddVectorSearchAsync(query, options);
            }

            ConfigureTextSearchFields(query, options);
            options.QueryType = SearchQueryType.Full;

            return (options, useTextSearch);
        }

        private void DetectPriceFilter(string query, SearchOptions options)
        {
            if (query.Contains("under $") || query.Contains("less than $") || query.Contains("below $"))
            {
                var priceMatch = Regex.Match(query, @"\$?(\d+)");
                if (priceMatch.Success)
                {
                    var maxPrice = int.Parse(priceMatch.Groups[1].Value);
                    options.Filter = $"{nameof(HotelSearchDocument.PricePerNight)} le {maxPrice}";
                }
            }
        }

        private List<string> DetectAmenityFilters(string query)
        {
            var filters = new List<string>();
            var queryLower = query.ToLower();

            if (queryLower.Contains("gym")) filters.Add($"{nameof(HotelSearchDocument.HasGym)} eq true");
            if (queryLower.Contains("pool")) filters.Add($"{nameof(HotelSearchDocument.HasPool)} eq true");
            if (queryLower.Contains("spa")) filters.Add($"{nameof(HotelSearchDocument.HasSpa)} eq true");
            if (queryLower.Contains("pet")) filters.Add($"{nameof(HotelSearchDocument.PetFriendly)} eq true");
            if (queryLower.Contains("parking")) filters.Add($"{nameof(HotelSearchDocument.HasParking)} eq true");
            if (queryLower.Contains("wifi")) filters.Add($"{nameof(HotelSearchDocument.HasWifi)} eq true");

            return filters;
        }

        private void CombineFilters(SearchOptions options, string newFilter)
        {
            options.Filter = string.IsNullOrEmpty(options.Filter)
                ? newFilter
                : $"{options.Filter} and {newFilter}";
        }

        private async Task AddVectorSearchAsync(string query, SearchOptions options)
        {
            if (_embeddingService == null) return;

            var embedding = await _embeddingService.GenerateEmbeddingAsync(query);
            options.VectorSearch = new VectorSearchOptions
            {
                Queries = {
                    new VectorizedQuery(embedding.ToArray())
                    {
                        KNearestNeighborsCount = 5,
                        Fields = {
                            nameof(HotelSearchDocument.DescriptionVector),
                            nameof(HotelSearchDocument.AmenitiesVector)
                        },
                        Weight = 0.4f
                    }
                }
            };
        }

        private void ConfigureTextSearchFields(string query, SearchOptions options)
        {
            if (query.ToLower().Contains("cancel"))
            {
                options.SearchFields.Add(nameof(HotelSearchDocument.CancellationPolicy));
            }
            else
            {
                options.SearchFields.Add(nameof(HotelSearchDocument.HotelName));
                options.SearchFields.Add(nameof(HotelSearchDocument.Description));
                options.SearchFields.Add(nameof(HotelSearchDocument.Amenities));
            }
        }
    }
}