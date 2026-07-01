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
    public class HomeController : Controller
    {
        // GET: Home
        [HttpGet]
        public ActionResult Index(SearchViewModel model)
        {
            if (model == null)
            {
                model = new SearchViewModel();
            }

            var items = AccommodationRepository.GetAll().Where(a => a.IsAvailable);

            if (!string.IsNullOrWhiteSpace(model.SearchTerm))
            {
                string term = model.SearchTerm.Trim().ToLowerInvariant();
                items = items.Where(a => a.Name != null && a.Name.ToLowerInvariant().Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(model.City))
            {
                string city = model.City.Trim().ToLowerInvariant();
                items = items.Where(a => a.City != null && a.City.ToLowerInvariant().Contains(city));
            }
            if (model.Type.HasValue)
            {
                items = items.Where(a => a.Type == model.Type.Value);
            }
            if (model.MinPrice.HasValue)
            {
                items = items.Where(a => a.PricePerNight >= model.MinPrice.Value);
            }
            if (model.MaxPrice.HasValue)
            {
                items = items.Where(a => a.PricePerNight <= model.MaxPrice.Value);
            }

            items = ApplySort(items, model.SortBy, model.SortDir);

            model.Results = items.ToList();
            return View(model);
        }

        private static IEnumerable<Accommodation> ApplySort(IEnumerable<Accommodation> items, string sortBy, string sortDir)
        {
            bool desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
            switch ((sortBy ?? string.Empty).ToLowerInvariant())
            {
                case "price":
                    return desc ? items.OrderByDescending(a => a.PricePerNight) : items.OrderBy(a => a.PricePerNight);
                case "date":
                    return desc
                        ? items.OrderByDescending(a => ParseDate(a.PostingDate))
                        : items.OrderBy(a => ParseDate(a.PostingDate));
                case "name":
                    return desc ? items.OrderByDescending(a => a.Name) : items.OrderBy(a => a.Name);
                default:
                    return items;
            }
        }

        private static DateTime ParseDate(string value)
        {
            DateTime dt;
            return DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)
                ? dt
                : DateTime.MinValue;
        }
    }
}
