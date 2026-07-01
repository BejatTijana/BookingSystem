using System.ComponentModel.DataAnnotations;
using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class AccommodationFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [Display(Name = "Type")]
        public AccommodationType Type { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required(ErrorMessage = "Price per night is required.")]
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0.")]
        [Display(Name = "Price per night")]
        public decimal? PricePerNight { get; set; }

        [Required(ErrorMessage = "Max guests is required.")]
        [Range(1, 1000, ErrorMessage = "Max guests must be at least 1.")]
        [Display(Name = "Max guests")]
        public int? MaxGuests { get; set; }

        [Display(Name = "Available")]
        public bool IsAvailable { get; set; }

        // Edit only: existing image filename for preview + hidden round-trip.
        public string ExistingImage { get; set; }
    }
}
