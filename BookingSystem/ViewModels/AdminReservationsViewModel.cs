using System.Collections.Generic;
using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class AdminReservationsViewModel
    {
        public List<AdminReservationItemViewModel> Items { get; set; } = new List<AdminReservationItemViewModel>();
        public ReservationStatus? StatusFilter { get; set; }
    }
}
