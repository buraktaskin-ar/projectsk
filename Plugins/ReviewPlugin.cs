using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using ChatWithAPIDemo.Services;

namespace ChatWithAPIDemo.Plugins
{
    public class ReviewPlugin
    {
        private readonly ReviewService _reviewService;
        private static readonly JsonSerializerOptions J = new() { WriteIndented = true };

        public ReviewPlugin(ReviewService reviewService) => _reviewService = reviewService;

        [KernelFunction, Description("Get all reviews")]
        public string GetAllReviews() => JsonSerializer.Serialize(_reviewService.GetAllReviews(), J);

        [KernelFunction, Description("Get reviews for a specific hotel")]
        public string GetReviewsByHotelId([Description("Hotel id")] int hotelId)
            => JsonSerializer.Serialize(_reviewService.GetReviewsByHotelId(hotelId), J);

        [KernelFunction, Description("Get reviews written by a person")]
        public string GetReviewsByPersonId([Description("Person id")] int personId)
            => JsonSerializer.Serialize(_reviewService.GetReviewsByPersonId(personId), J);

        [KernelFunction, Description("Get a single review by id")]
        public string GetReviewById([Description("Review id (int)")] int reviewId)
            => JsonSerializer.Serialize(_reviewService.GetReviewById(reviewId), J);

        [KernelFunction, Description("Get average rating for a hotel")]
        public string GetAverageRatingForHotel([Description("Hotel id")] int hotelId)
            => JsonSerializer.Serialize(new { hotelId, average = _reviewService.GetAverageRatingForHotel(hotelId) }, J);

        [KernelFunction, Description("Delete a review by id")]
        public string DeleteReview([Description("Review id (int)")] int reviewId)
            => JsonSerializer.Serialize(new { success = _reviewService.DeleteReview(reviewId) }, J);
    }
}