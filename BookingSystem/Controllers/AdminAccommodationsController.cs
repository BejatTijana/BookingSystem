using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookingSystem.DataAccess;
using BookingSystem.Helpers;
using BookingSystem.Models;
using BookingSystem.ViewModels;

namespace BookingSystem.Controllers
{
    public class AdminAccommodationsController : Controller
    {
        // ApplySort i ParsePostingDate su namjerno duplirani iz HostAccommodationsController — refaktor u shared helper je odgođen
        private const string DateFormat = "dd/MM/yyyy";

        // GET: AdminAccommodations
        [HttpGet]
        public ActionResult Index(bool? available, string sortBy, string sortDir)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            IEnumerable<Accommodation> items = AccommodationRepository.GetAll();

            if (available.HasValue)
            {
                items = items.Where(x => x.IsAvailable == available.Value);
            }

            if (string.IsNullOrEmpty(sortBy)) sortBy = "name";
            if (sortDir != "desc") sortDir = "asc";

            var sorted = ApplySort(items, sortBy, sortDir).ToList();

            return View(new AdminAccommodationsViewModel
            {
                Items = sorted,
                AvailabilityFilter = available,
                SortBy = sortBy,
                SortDir = sortDir
            });
        }

        // GET: AdminAccommodations/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var acc = AccommodationRepository.GetById(id);
            if (acc == null)
            {
                TempData["Error"] = "Accommodation not found.";
                return RedirectToAction("Index");
            }

            return View(new AdminAccommodationFormViewModel
            {
                Id = acc.Id,
                Name = acc.Name,
                Type = acc.Type,
                Description = acc.Description,
                Address = acc.Address,
                City = acc.City,
                PricePerNight = acc.PricePerNight,
                MaxGuests = acc.MaxGuests,
                IsAvailable = acc.IsAvailable,
                HostUsername = acc.HostUsername,
                PostingDate = acc.PostingDate,
                ExistingImage = acc.Image
            });
        }

        // POST: AdminAccommodations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AdminAccommodationFormViewModel model, HttpPostedFileBase imageFile)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var acc = AccommodationRepository.GetById(id);
            if (acc == null)
            {
                TempData["Error"] = "Accommodation not found.";
                return RedirectToAction("Index");
            }

            string imgError;
            if (!ImageHelper.Validate(imageFile, false, out imgError))
            {
                ModelState.AddModelError("", imgError);
            }

            if (!ModelState.IsValid)
            {
                model.Id = acc.Id;
                model.HostUsername = acc.HostUsername;
                model.PostingDate = acc.PostingDate;
                model.ExistingImage = acc.Image;
                return View(model);
            }

            // HostUsername, PostingDate, Id i IsDeleted se nikad ne mijenjaju — anti-tamper
            acc.Name = model.Name;
            acc.Type = model.Type;
            acc.Description = model.Description;
            acc.Address = model.Address;
            acc.City = model.City;
            acc.PricePerNight = model.PricePerNight.Value;
            acc.MaxGuests = model.MaxGuests.Value;
            acc.IsAvailable = model.IsAvailable;

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string savedName;
                string saveError;
                if (!ImageHelper.Save(imageFile, Server.MapPath("~/Content/images/"), out savedName, out saveError))
                {
                    ModelState.AddModelError("", saveError);
                    model.Id = acc.Id;
                    model.HostUsername = acc.HostUsername;
                    model.PostingDate = acc.PostingDate;
                    model.ExistingImage = acc.Image;
                    return View(model);
                }
                acc.Image = savedName;
            }

            AccommodationRepository.Update(acc);

            TempData["Success"] = "Accommodation updated.";
            return RedirectToAction("Index");
        }

        // POST: AdminAccommodations/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var acc = AccommodationRepository.GetById(id);
            if (acc == null)
            {
                TempData["Error"] = "Accommodation not found.";
                return RedirectToAction("Index");
            }

            bool hasBlockingReservation = ReservationRepository.GetAll().Any(r =>
                r.AccommodationId == id
                && (r.Status == ReservationStatus.Created || r.Status == ReservationStatus.Approved));
            if (hasBlockingReservation)
            {
                TempData["Error"] = "Cannot delete accommodation with created or approved reservations.";
                return RedirectToAction("Index");
            }

            AccommodationRepository.Delete(id);

            TempData["Success"] = "Accommodation deleted.";
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

        private static IEnumerable<Accommodation> ApplySort(IEnumerable<Accommodation> items, string sortBy, string sortDir)
        {
            bool desc = sortDir == "desc";
            switch (sortBy)
            {
                case "price":
                    return desc ? items.OrderByDescending(x => x.PricePerNight) : items.OrderBy(x => x.PricePerNight);
                case "date":
                    return desc
                        ? items.OrderByDescending(x => ParsePostingDate(x.PostingDate))
                        : items.OrderBy(x => ParsePostingDate(x.PostingDate));
                case "name":
                default:
                    return desc ? items.OrderByDescending(x => x.Name) : items.OrderBy(x => x.Name);
            }
        }

        private static DateTime ParsePostingDate(string s)
        {
            DateTime d;
            return DateTime.TryParseExact(s, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                ? d
                : DateTime.MinValue;
        }
    }
}
