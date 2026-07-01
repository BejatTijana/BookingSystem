using System.Collections.Generic;
using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class AdminReviewsViewModel
    {
        public List<AdminReviewItemViewModel> Items { get; set; } = new List<AdminReviewItemViewModel>();
        public ReviewStatus? StatusFilter { get; set; }
    }
}
