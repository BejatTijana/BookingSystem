using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class ReservationListItemViewModel
    {
        public Reservation Reservation { get; set; }
        public string AccommodationName { get; set; }
        public bool CanCancel { get; set; }
        public int? ReviewId { get; set; }
    }
}
