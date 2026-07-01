using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.ViewModels;

namespace BookingSystem.Controllers
{
    public class AccommodationsController : Controller
    {
        private const string DateFormat = "dd/MM/yyyy";

        // GET: Accommodations/Details/5
        [HttpGet]
        public ActionResult Details(int id)
        {
            var acc = AccommodationRepository.GetById(id);
            if (acc == null || !acc.IsAvailable)
            {
                return RedirectToAction("Index", "Home");
            }

            var approved = ReviewRepository.GetAll()
                .Where(x => x.AccommodationId == acc.Id && x.Status == ReviewStatus.Approved)
                .ToList();

            var vm = new AccommodationDetailsViewModel
            {
                Accommodation = acc,
                Reservation = new CreateReservationViewModel { AccommodationId = acc.Id, NumberOfGuests = 1 },
                CanReserve = CanCurrentUserReserve(acc),
                ApprovedReviews = approved,
                AverageRating = approved.Any() ? (double?)approved.Average(x => x.Rating) : null
            };
            return View(vm);
        }

        // POST: Accommodations/Reserve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reserve([Bind(Prefix = "Reservation")] CreateReservationViewModel model)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Guest)
            {
                TempData["Error"] = "Only guests can create reservations.";
                return RedirectToAction("Index", "Home");
            }

            var acc = AccommodationRepository.GetById(model.AccommodationId);
            if (acc == null || !acc.IsAvailable)
            {
                TempData["Error"] = "Accommodation is not available.";
                return RedirectToAction("Index", "Home");
            }

            if (model.NumberOfGuests < 1 || model.NumberOfGuests > acc.MaxGuests)
            {
                ModelState.AddModelError("Reservation.NumberOfGuests",
                    "Number of guests must be between 1 and " + acc.MaxGuests + ".");
            }
            if (model.CheckInDate.HasValue && model.CheckInDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError("Reservation.CheckInDate", "Check-in date cannot be in the past.");
            }
            if (model.CheckInDate.HasValue && model.CheckOutDate.HasValue
                && model.CheckOutDate.Value.Date <= model.CheckInDate.Value.Date)
            {
                ModelState.AddModelError("Reservation.CheckOutDate", "Check-out date must be after check-in date.");
            }

            if (ModelState.IsValid && model.CheckInDate.HasValue && model.CheckOutDate.HasValue
                && HasApprovedOverlap(acc.Id, model.CheckInDate.Value, model.CheckOutDate.Value))
            {
                ModelState.AddModelError("", "Selected dates overlap an existing approved reservation.");
            }

            if (!ModelState.IsValid)
            {
                return View("Details", new AccommodationDetailsViewModel
                {
                    Accommodation = acc,
                    Reservation = model,
                    CanReserve = true
                });
            }

            int nights = (model.CheckOutDate.Value.Date - model.CheckInDate.Value.Date).Days;
            ReservationRepository.Add(new Reservation
            {
                GuestUsername = username,
                AccommodationId = acc.Id,
                CheckInDate = model.CheckInDate.Value.ToString(DateFormat, CultureInfo.InvariantCulture),
                CheckOutDate = model.CheckOutDate.Value.ToString(DateFormat, CultureInfo.InvariantCulture),
                NumberOfGuests = model.NumberOfGuests,
                TotalPrice = nights * acc.PricePerNight,
                Status = ReservationStatus.Created,
                IsDeleted = false
            });

            TempData["Success"] = "Reservation created - pending approval.";
            return RedirectToAction("Index", "Home");
        }

        private bool CanCurrentUserReserve(Accommodation acc)
        {
            if (!acc.IsAvailable)
            {
                return false;
            }
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }
            var user = UserRepository.GetByUsername(username);
            return user != null && user.Role == Role.Guest;
        }

        private static bool HasApprovedOverlap(int accommodationId, DateTime newIn, DateTime newOut)
        {
            foreach (var r in ReservationRepository.GetAll()
                .Where(x => x.AccommodationId == accommodationId && x.Status == ReservationStatus.Approved))
            {
                DateTime existIn, existOut;
                if (DateTime.TryParseExact(r.CheckInDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out existIn)
                    && DateTime.TryParseExact(r.CheckOutDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out existOut)
                    && newIn.Date < existOut.Date && newOut.Date > existIn.Date)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
