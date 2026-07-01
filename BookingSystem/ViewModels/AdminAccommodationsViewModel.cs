using System.Collections.Generic;
using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class AdminAccommodationsViewModel
    {
        public List<Accommodation> Items { get; set; } = new List<Accommodation>();
        public bool? AvailabilityFilter { get; set; }
        public string SortBy { get; set; }   // "name" | "price" | "date"
        public string SortDir { get; set; }  // "asc" | "desc"
    }
}
