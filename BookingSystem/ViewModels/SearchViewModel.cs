using System.Collections.Generic;
using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class SearchViewModel
    {
        public string SearchTerm { get; set; }
        public string City { get; set; }
        public AccommodationType? Type { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; }
        public string SortDir { get; set; }
        public List<Accommodation> Results { get; set; } = new List<Accommodation>();
    }
}
