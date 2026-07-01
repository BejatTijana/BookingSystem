using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace BookingSystem.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public Role Role { get; set; }
        public bool IsDeleted { get; set; }
        [ScriptIgnore]
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        [ScriptIgnore]
        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
    }
}
