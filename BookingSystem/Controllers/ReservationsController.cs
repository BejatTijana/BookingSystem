using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.ViewModels;

namespace BookingSystem.Controllers
{
    public class ReservationsController : Controller
    {
        private const string DateFormat = "dd/MM/yyyy";

        // GET: Reservations
        [HttpGet]
        public ActionResult Index(ReservationStatus? status)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Guest)
            {
                TempData["Error"] = "Only guests can view reservations.";
                return RedirectToAction("Index", "Home");
            }

            var mine = ReservationRepository.GetAll().Where(r => r.GuestUsername == username);
            if (status.HasValue)
            {
                mine = mine.Where(r => r.Status == status.Value);
            }

            var myReviews = ReviewRepository.GetAll().Where(x => x.ReviewerUsername == username).ToList();

            var items = mine
                .Select(r => new ReservationListItemViewModel
                {
                    Reservation = r,
                    AccommodationName = AccommodationRepository.GetById(r.AccommodationId)?.Name ?? "(unavailable)",
                    CanCancel = CanCancel(r),
                    ReviewId = r.Status == ReservationStatus.Completed
                        ? (int?)myReviews.FirstOrDefault(x => x.AccommodationId == r.AccommodationId)?.Id
                        : null
                })
                .ToList();

            return View(new MyReservationsViewModel { Items = items, StatusFilter = status });
        }

        // POST: Reservations/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Guest)
            {
                TempData["Error"] = "Only guests can cancel reservations.";
                return RedirectToAction("Index", "Home");
            }

            var reservation = ReservationRepository.GetById(id);
            if (reservation == null || reservation.GuestUsername != username || !CanCancel(reservation))
            {
                TempData["Error"] = "Reservation can no longer be cancelled.";
                return RedirectToAction("Index");
            }

            reservation.Status = ReservationStatus.Cancelled;
            ReservationRepository.Update(reservation);
            TempData["Success"] = "Reservation cancelled.";
            return RedirectToAction("Index");
        }

        private static bool CanCancel(Reservation reservation)
        {
            if (reservation.Status != ReservationStatus.Created && reservation.Status != ReservationStatus.Approved)
            {
                return false;
            }

            DateTime checkIn;
            if (!DateTime.TryParseExact(reservation.CheckInDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out checkIn))
            {
                return false;
            }

            return checkIn >= DateTime.Now.AddHours(24);
        }
    }
}
