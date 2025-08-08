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
        public string GetReviewsByPersonId([Description("Person id (GUID)")] string personId)
        {
            if (!Guid.TryParse(personId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid GUID format" }, J);

            return JsonSerializer.Serialize(_reviewService.GetReviewsByPersonId(id), J);
        }

        [KernelFunction, Description("Get a single review by id")]
        public string GetReviewById([Description("Review id (int)")] int reviewId)
            => JsonSerializer.Serialize(_reviewService.GetReviewById(reviewId), J);

        [KernelFunction, Description("Create a review")]
        public string CreateReview(
            [Description("Person id (GUID)")] string personId,
            [Description("Hotel id")] int hotelId,
            [Description("Rating (1-5)")] int rating,
            [Description("Review comment")] string comment)
        {
            if (!Guid.TryParse(personId, out var id))
                return JsonSerializer.Serialize(new { error = "Invalid GUID format" }, J);

            if (rating < 1 || rating > 5)
                return JsonSerializer.Serialize(new { error = "Rating must be between 1 and 5" }, J);

            var review = _reviewService.CreateReview(id, hotelId, rating, comment);
            if (review == null)
                return JsonSerializer.Serialize(new { error = "Failed to create review. Person or hotel not found." }, J);

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Review created successfully",
                review = review
            }, J);
        }

        [KernelFunction, Description("Get average rating for a hotel")]
        public string GetAverageRatingForHotel([Description("Hotel id")] int hotelId)
            => JsonSerializer.Serialize(new
            {
                hotelId,
                averageRating = _reviewService.GetAverageRatingForHotel(hotelId),
                reviewCount = _reviewService.GetReviewsByHotelId(hotelId).Count
            }, J);

        [KernelFunction, Description("Delete a review by id")]
        public string DeleteReview([Description("Review id (int)")] int reviewId)
        {
            var success = _reviewService.DeleteReview(reviewId);
            return JsonSerializer.Serialize(new
            {
                success,
                message = success ? "Review deleted successfully" : "Review not found"
            }, J);
        }
    }
}