using System;
using System.ComponentModel.DataAnnotations;

namespace BookingSystem.ViewModels
{
    public class CreateReservationViewModel
    {
        public int AccommodationId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Check-in date")]
        public DateTime? CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Check-out date")]
        public DateTime? CheckOutDate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of guests must be at least 1.")]
        [Display(Name = "Number of guests")]
        public int NumberOfGuests { get; set; }
    }
}
