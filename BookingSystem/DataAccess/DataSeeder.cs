using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BookingSystem.Models;

namespace BookingSystem.DataAccess
{
    public static class DataSeeder
    {
        public static void Seed()
        {
            SeedUsers();
            SeedAccommodations();
            SeedReservations();
            SeedReviews();
        }

        private static void SeedUsers()
        {
            if (JsonStore.ReadAll<User>("users.json").Any())
            {
                return;
            }

            var users = new List<User>
            {
                new User { Username = "TijanaBejat", Password = "Bejat@123", FirstName = "Tijana", LastName = "Bejat", Email = "bejatt.2004@gmail.com", DateOfBirth = "01/12/2004", Gender = Gender.Female, Role = Role.Admin, IsDeleted = false },
                new User { Username = "marko.markovic", Password = "Host@123", FirstName = "Marko", LastName = "Markovic", Email = "marko.markovic@example.com", DateOfBirth = "15/04/1988", Gender = Gender.Male, Role = Role.Host, IsDeleted = false },
                new User { Username = "jelena.jovanovic", Password = "Host@123", FirstName = "Jelena", LastName = "Jovanovic", Email = "jelena.jovanovic@example.com", DateOfBirth = "22/09/1990", Gender = Gender.Female, Role = Role.Host, IsDeleted = false },
                new User { Username = "nikola.nikolic", Password = "Guest@123", FirstName = "Nikola", LastName = "Nikolic", Email = "nikola.nikolic@example.com", DateOfBirth = "10/06/1995", Gender = Gender.Male, Role = Role.Guest, IsDeleted = false },
                new User { Username = "ana.anic", Password = "Guest@123", FirstName = "Ana", LastName = "Anic", Email = "ana.anic@example.com", DateOfBirth = "03/02/2000", Gender = Gender.Female, Role = Role.Guest, IsDeleted = false }
            };
            JsonStore.WriteAll("users.json", users);
        }

        private static void SeedAccommodations()
        {
            if (JsonStore.ReadAll<Accommodation>("accommodations.json").Any())
            {
                return;
            }

            var accommodations = new List<Accommodation>
            {
                new Accommodation { Id = 1, Name = "Grand Hotel Central", Type = AccommodationType.Hotel, Description = "Central city hotel with spacious rooms.", Address = "Knez Mihailova 10", City = "Belgrade", PricePerNight = 120.00m, MaxGuests = 2, Image = "hotel_central.jpg", PostingDate = "10/01/2025", HostUsername = "marko.markovic", IsAvailable = true, IsDeleted = false },
                new Accommodation { Id = 2, Name = "Sunny Apartment", Type = AccommodationType.Apartment, Description = "Bright apartment near the river.", Address = "Bulevar Oslobodjenja 25", City = "Novi Sad", PricePerNight = 65.50m, MaxGuests = 4, Image = "apartment_sunny.jpg", PostingDate = "05/02/2025", HostUsername = "jelena.jovanovic", IsAvailable = true, IsDeleted = false },
                new Accommodation { Id = 3, Name = "Backpackers Hostel", Type = AccommodationType.Hostel, Description = "Budget friendly hostel for travelers.", Address = "Cara Dusana 4", City = "Nis", PricePerNight = 20.00m, MaxGuests = 6, Image = "hostel_backpackers.jpg", PostingDate = "20/02/2025", HostUsername = "marko.markovic", IsAvailable = false, IsDeleted = false },
                new Accommodation { Id = 4, Name = "Mountain Lodge Apartment", Type = AccommodationType.Apartment, Description = "Cozy apartment with a mountain view.", Address = "Kopaonik bb", City = "Kopaonik", PricePerNight = 90.00m, MaxGuests = 3, Image = "apartment_mountain.jpg", PostingDate = "01/03/2025", HostUsername = "jelena.jovanovic", IsAvailable = true, IsDeleted = false }
            };
            JsonStore.WriteAll("accommodations.json", accommodations);
        }

        private static void SeedReservations()
        {
            if (JsonStore.ReadAll<Reservation>("reservations.json").Any())
            {
                return;
            }

            Reservation Make(int id, string guest, int accId, int inOffset, int outOffset, int guests, decimal pricePerNight, ReservationStatus status)
            {
                DateTime checkIn = DateTime.Today.AddDays(inOffset);
                DateTime checkOut = DateTime.Today.AddDays(outOffset);
                int nights = (checkOut - checkIn).Days;
                return new Reservation
                {
                    Id = id,
                    GuestUsername = guest,
                    AccommodationId = accId,
                    CheckInDate = checkIn.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    CheckOutDate = checkOut.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                    NumberOfGuests = guests,
                    TotalPrice = nights * pricePerNight,
                    Status = status,
                    IsDeleted = false
                };
            }

            var reservations = new List<Reservation>
            {
                Make(1, "ana.anic", 1, -40, -37, 2, 120.00m, ReservationStatus.Completed),
                Make(2, "nikola.nikolic", 2, -35, -32, 3, 65.50m, ReservationStatus.Completed),
                Make(3, "ana.anic", 4, -30, -27, 2, 90.00m, ReservationStatus.Completed),
                Make(4, "ana.anic", 2, 30, 34, 3, 65.50m, ReservationStatus.Approved),
                Make(5, "nikola.nikolic", 1, 15, 18, 2, 120.00m, ReservationStatus.Created),
                Make(6, "nikola.nikolic", 4, -60, -58, 2, 90.00m, ReservationStatus.Cancelled),
                Make(7, "ana.anic", 1, 0, 2, 1, 120.00m, ReservationStatus.Created)
            };
            JsonStore.WriteAll("reservations.json", reservations);
        }

        private static void SeedReviews()
        {
            if (JsonStore.ReadAll<Review>("reviews.json").Any())
            {
                return;
            }

            var reviews = new List<Review>
            {
                new Review { Id = 1, AccommodationId = 1, ReviewerUsername = "ana.anic", Title = "Great stay", Content = "Clean rooms and friendly staff.", Rating = 5, Image = "", Status = ReviewStatus.Approved, IsDeleted = false },
                new Review { Id = 2, AccommodationId = 2, ReviewerUsername = "nikola.nikolic", Title = "Nice apartment", Content = "Good location near the river.", Rating = 4, Image = "", Status = ReviewStatus.Created, IsDeleted = false },
                new Review { Id = 3, AccommodationId = 4, ReviewerUsername = "ana.anic", Title = "Not as expected", Content = "The photos looked better than reality.", Rating = 2, Image = "", Status = ReviewStatus.Rejected, IsDeleted = false }
            };
            JsonStore.WriteAll("reviews.json", reviews);
        }
    }
}
