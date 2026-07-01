using System.Collections.Generic;
using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class MyReservationsViewModel
    {
        public List<ReservationListItemViewModel> Items { get; set; } = new List<ReservationListItemViewModel>();
        public ReservationStatus? StatusFilter { get; set; }
    }
}
