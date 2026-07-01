using System.Collections.Generic;
using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class AccommodationDetailsViewModel
    {
        public Accommodation Accommodation { get; set; }
        public CreateReservationViewModel Reservation { get; set; }
        public bool CanReserve { get; set; }
        public List<Review> ApprovedReviews { get; set; } = new List<Review>();
        public double? AverageRating { get; set; }
    }
}
