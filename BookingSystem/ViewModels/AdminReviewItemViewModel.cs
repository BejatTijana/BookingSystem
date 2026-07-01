using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class AdminReviewItemViewModel
    {
        public Review Review { get; set; }
        public string AccommodationName { get; set; }
        public bool CanModerate { get; set; }
    }
}
