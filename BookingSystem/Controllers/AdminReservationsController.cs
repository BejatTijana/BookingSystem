using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.ViewModels;

namespace BookingSystem.Controllers
{
    public class AdminReservationsController : Controller
    {
        private const string DateFormat = "dd/MM/yyyy";

        // GET: AdminReservations
        [HttpGet]
        public ActionResult Index(ReservationStatus? status)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            IEnumerable<Reservation> all = ReservationRepository.GetAll();
            if (status.HasValue)
            {
                all = all.Where(r => r.Status == status.Value);
            }

            var items = all
                .Select(r => new AdminReservationItemViewModel
                {
                    Reservation = r,
                    AccommodationName = AccommodationRepository.GetById(r.AccommodationId)?.Name ?? "(unavailable)",
                    CanApprove = CanApprove(r),
                    CanCancel = CanCancel(r)
                })
                .ToList();

            return View(new AdminReservationsViewModel { Items = items, StatusFilter = status });
        }

        // POST: AdminReservations/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var r = ReservationRepository.GetById(id);
            if (r == null || r.Status != ReservationStatus.Created)
            {
                TempData["Error"] = "Only created reservations can be approved.";
                return RedirectToAction("Index");
            }

            if (HasApprovedOverlap(r.AccommodationId, r.Id, r.CheckInDate, r.CheckOutDate))
            {
                TempData["Error"] = "Cannot approve: this reservation overlaps an existing approved reservation.";
                return RedirectToAction("Index");
            }

            r.Status = ReservationStatus.Approved;
            ReservationRepository.Update(r);
            TempData["Success"] = "Reservation approved.";
            return RedirectToAction("Index");
        }

        // POST: AdminReservations/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var r = ReservationRepository.GetById(id);
            if (r == null || (r.Status != ReservationStatus.Created && r.Status != ReservationStatus.Approved))
            {
                TempData["Error"] = "Only created or approved reservations can be cancelled.";
                return RedirectToAction("Index");
            }

            if (!IsAtLeast24hBeforeCheckIn(r))
            {
                TempData["Error"] = "Reservation can no longer be cancelled (less than 24 hours before check-in).";
                return RedirectToAction("Index");
            }

            r.Status = ReservationStatus.Cancelled;
            ReservationRepository.Update(r);
            TempData["Success"] = "Reservation cancelled.";
            return RedirectToAction("Index");
        }

        private ActionResult RequireAdmin(out string username)
        {
            username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Admin)
            {
                TempData["Error"] = "Only administrators can access this page.";
                return RedirectToAction("Index", "Home");
            }
            return null;
        }

        private static bool CanApprove(Reservation r)
        {
            return r.Status == ReservationStatus.Created;
        }

        private static bool CanCancel(Reservation r)
        {
            if (r.Status != ReservationStatus.Created && r.Status != ReservationStatus.Approved)
            {
                return false;
            }
            return IsAtLeast24hBeforeCheckIn(r);
        }

        private static bool IsAtLeast24hBeforeCheckIn(Reservation r)
        {
            DateTime checkIn;
            if (!DateTime.TryParseExact(r.CheckInDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out checkIn))
            {
                return false;
            }
            return checkIn >= DateTime.Now.AddHours(24);
        }

        private static bool HasApprovedOverlap(int accommodationId, int excludeReservationId, string newCheckIn, string newCheckOut)
        {
            DateTime newIn, newOut;
            if (!DateTime.TryParseExact(newCheckIn, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out newIn)
                || !DateTime.TryParseExact(newCheckOut, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out newOut))
            {
                return false;
            }

            foreach (var r in ReservationRepository.GetAll()
                .Where(x => x.AccommodationId == accommodationId
                    && x.Id != excludeReservationId
                    && x.Status == ReservationStatus.Approved))
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
