using System.ComponentModel.DataAnnotations;

namespace BookingSystem.ViewModels
{
    public class ReviewFormViewModel
    {
        public int? Id { get; set; }
        public int AccommodationId { get; set; }
        public string AccommodationName { get; set; }

        public string ExistingImage { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Content")]
        public string Content { get; set; }

        [Range(1, 5)]
        [Display(Name = "Rating")]
        public int Rating { get; set; }
    }
}
