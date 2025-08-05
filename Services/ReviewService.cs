using ChatWithAPIDemo.Models;
namespace ChatWithAPIDemo.Services
{

    public class ReviewService
    {
        private readonly List<Review> _reviews;
        private readonly PersonService _personService;
        private readonly HotelService _hotelService;

        public ReviewService(List<Review> reviews, PersonService personService, HotelService hotelService)
        {
            _reviews = reviews;
            _personService = personService;
            _hotelService = hotelService;
        }

        public List<Review> GetAllReviews() => _reviews;

        public List<Review> GetReviewsByHotelId(int hotelId)
        {
            return _reviews.Where(r => r.Hotel.Id == hotelId).ToList();
        }

        public List<Review> GetReviewsByPersonId(int personId)
        {
            return _reviews.Where(r => r.Person.Id == personId).ToList();
        }

        public Review? GetReviewById(int id)
        {
            return _reviews.FirstOrDefault(r => r.Id == id);
        }

        public Review? CreateReview(int personId, int hotelId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                return null;

            var person = _personService.FindPersonById(personId);
            if (person == null)
                return null;

            var hotel = _hotelService.GetHotelById(hotelId);
            if (hotel == null)
                return null;

            var review = new Review
            {
                Id = _reviews.Count > 0 ? _reviews.Max(r => r.Id) + 1 : 1,
                Rating = rating,
                Comment = comment,
                Person = person,
                Hotel = hotel
            };

            _reviews.Add(review);

            // Loyalty points ekle
            _personService.AddLoyaltyPoints(personId, 5); // Her review için 5 puan

            return review;
        }

        public double GetAverageRatingForHotel(int hotelId)
        {
            var hotelReviews = GetReviewsByHotelId(hotelId);
            return hotelReviews.Any() ? hotelReviews.Average(r => r.Rating) : 0;
        }

        public bool DeleteReview(int reviewId)
        {
            var review = _reviews.FirstOrDefault(r => r.Id == reviewId);
            if (review == null)
                return false;

            _reviews.Remove(review);
            return true;
        }
    }
}
