using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class ProfileViewModel
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public Role Role { get; set; }
    }
}
