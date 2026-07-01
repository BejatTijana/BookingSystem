using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace BookingSystem.Models
{
    public class Accommodation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AccommodationType Type { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public string Image { get; set; }
        public string PostingDate { get; set; }
        public string HostUsername { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsDeleted { get; set; }
        [ScriptIgnore]
        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
