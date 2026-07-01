using System;
using System.Collections.Generic;
using BookingSystem.Models;

namespace BookingSystem.ViewModels
{
    public class AdminUsersViewModel
    {
        public List<User> Items { get; set; } = new List<User>();
        public string SearchFirstName { get; set; }
        public string SearchLastName { get; set; }
        public DateTime? BornFrom { get; set; }
        public DateTime? BornTo { get; set; }
        public Role? RoleFilter { get; set; }
        public string SortBy { get; set; }   // "firstName" | "dob" | "role"
        public string SortDir { get; set; }  // "asc" | "desc"
    }
}
