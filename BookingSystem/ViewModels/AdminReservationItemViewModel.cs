using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class AdminReservationItemViewModel
    {
        public Reservation Reservation { get; set; }
        public string AccommodationName { get; set; }
        public bool CanApprove { get; set; }
        public bool CanCancel { get; set; }
    }
}
