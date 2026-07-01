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
    public class HostAccommodationsController : Controller
    {
        private const string DateFormat = "dd/MM/yyyy";

        // GET: HostAccommodations
        [HttpGet]
        public ActionResult Index(bool? available, string sortBy, string sortDir)
        {
            string username;
            var gate = RequireHost(out username);
            if (gate != null) return gate;

            IEnumerable<Accommodation> mine = AccommodationRepository.GetAll()
                .Where(x => x.HostUsername == username);

            if (available.HasValue)
            {
                mine = mine.Where(x => x.IsAvailable == available.Value);
            }

            if (string.IsNullOrEmpty(sortBy)) sortBy = "name";
            if (sortDir != "desc") sortDir = "asc";

            var sorted = ApplySort(mine, sortBy, sortDir).ToList();

            return View(new HostAccommodationsViewModel
            {
                Items = sorted,
                AvailabilityFilter = available,
                SortBy = sortBy,
                SortDir = sortDir
            });
        }

        // GET: HostAccommodations/Create
        [HttpGet]
        public ActionResult Create()
        {
            string username;
            var gate = RequireHost(out username);
            if (gate != null) return gate;

            return View(new AccommodationFormViewModel { IsAvailable = true });
        }

        // POST: HostAccommodations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AccommodationFormViewModel model, HttpPostedFileBase imageFile)
        {
            string username;
            var gate = RequireHost(out username);
            if (gate != null) return gate;

            string imgError;
            if (!ImageHelper.Validate(imageFile, true, out imgError))
            {
                ModelState.AddModelError("", imgError);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string savedName;
            string saveError;
            if (!ImageHelper.Save(imageFile, Server.MapPath("~/Content/images/"), out savedName, out saveError))
            {
                ModelState.AddModelError("", saveError);
                return View(model);
            }

            var acc = new Accommodation
            {
                Name = model.Name,
                Type = model.Type,
                Description = model.Description,
                Address = model.Address,
                City = model.City,
                PricePerNight = model.PricePerNight.Value,
                MaxGuests = model.MaxGuests.Value,
                Image = savedName,
                PostingDate = DateTime.Today.ToString(DateFormat, CultureInfo.InvariantCulture),
                HostUsername = username,
                IsAvailable = model.IsAvailable,
                IsDeleted = false
            };
            AccommodationRepository.Add(acc);

            TempData["Success"] = "Accommodation created.";
            return RedirectToAction("Index");
        }

        // GET: HostAccommodations/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            string username;
            var gate = RequireHost(out username);
            if (gate != null) return gate;

            var acc = AccommodationRepository.GetById(id);
            if (acc == null || acc.HostUsername != username)
            {
                TempData["Error"] = "You can only edit your own accommodations.";
                return RedirectToAction("Index");
            }

            var vm = new AccommodationFormViewModel
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
                ExistingImage = acc.Image
            };
            return View(vm);
        }

        // POST: HostAccommodations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AccommodationFormViewModel model, HttpPostedFileBase imageFile)
        {
            string username;
            var gate = RequireHost(out username);
            if (gate != null) return gate;

            var acc = AccommodationRepository.GetById(id);
            if (acc == null || acc.HostUsername != username)
            {
                TempData["Error"] = "You can only edit your own accommodations.";
                return RedirectToAction("Index");
            }

            // Interpretacija II: ako je objekat nedostupan, jedino što se može promijeniti je IsAvailable
            // — ostala polja iz forme se ignorišu, čak i ako je neko zaobišao disabled atribute
            if (!acc.IsAvailable)
            {
                acc.IsAvailable = model.IsAvailable;
                AccommodationRepository.Update(acc);
                TempData["Success"] = acc.IsAvailable
                    ? "Accommodation is now available."
                    : "Accommodation remains unavailable.";
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
                model.ExistingImage = acc.Image;
                return View(model);
            }

            acc.Name = model.Name;
            acc.Type = model.Type;
            acc.Description = model.Description;
            acc.Address = model.Address;
            acc.City = model.City;
            acc.PricePerNight = model.PricePerNight.Value;
            acc.MaxGuests = model.MaxGuests.Value;
            acc.IsAvailable = model.IsAvailable;
            // PostingDate, HostUsername, Id i IsDeleted se nikad ne mijenjaju ovdje — anti-tamper

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string savedName;
                string saveError;
                if (!ImageHelper.Save(imageFile, Server.MapPath("~/Content/images/"), out savedName, out saveError))
                {
                    ModelState.AddModelError("", saveError);
                    model.Id = acc.Id;
                    model.ExistingImage = acc.Image;
                    return View(model);
                }
                acc.Image = savedName;
            }

            AccommodationRepository.Update(acc);
            TempData["Success"] = "Accommodation updated.";
            return RedirectToAction("Index");
        }

        // POST: HostAccommodations/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            string username;
            var gate = RequireHost(out username);
            if (gate != null) return gate;

            var acc = AccommodationRepository.GetById(id);
            if (acc == null || acc.HostUsername != username)
            {
                TempData["Error"] = "You can only delete your own accommodations.";
                return RedirectToAction("Index");
            }
            if (!acc.IsAvailable)
            {
                TempData["Error"] = "Unavailable accommodations cannot be deleted.";
                return RedirectToAction("Index");
            }

            AccommodationRepository.Delete(id);
            TempData["Success"] = "Accommodation deleted.";
            return RedirectToAction("Index");
        }

        private ActionResult RequireHost(out string username)
        {
            username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Host)
            {
                TempData["Error"] = "Only hosts can manage accommodations.";
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
