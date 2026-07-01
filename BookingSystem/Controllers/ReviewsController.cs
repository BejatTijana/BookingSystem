using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookingSystem.DataAccess;
using BookingSystem.Helpers;
using BookingSystem.Models;
using BookingSystem.ViewModels;

namespace BookingSystem.Controllers
{
    public class ReviewsController : Controller
    {
        // GET: Reviews/Create/5
        [HttpGet]
        public ActionResult Create(int accommodationId)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Guest)
            {
                TempData["Error"] = "Only guests can write reviews.";
                return RedirectToAction("Index", "Home");
            }

            var acc = AccommodationRepository.GetById(accommodationId);
            if (acc == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!HasCompleted(username, accommodationId))
            {
                TempData["Error"] = "You can only review accommodations you have completed a stay at.";
                return RedirectToAction("Index", "Home");
            }

            var existing = FindOwnReview(username, accommodationId);
            if (existing != null)
            {
                return RedirectToAction("Edit", new { id = existing.Id });
            }

            return View(new ReviewFormViewModel
            {
                AccommodationId = acc.Id,
                AccommodationName = acc.Name,
                Rating = 5
            });
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ReviewFormViewModel model, HttpPostedFileBase imageFile)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Guest)
            {
                TempData["Error"] = "Only guests can write reviews.";
                return RedirectToAction("Index", "Home");
            }

            var acc = AccommodationRepository.GetById(model.AccommodationId);
            if (acc == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!HasCompleted(username, model.AccommodationId))
            {
                TempData["Error"] = "You can only review accommodations you have completed a stay at.";
                return RedirectToAction("Index", "Home");
            }

            var existing = FindOwnReview(username, model.AccommodationId);
            if (existing != null)
            {
                return RedirectToAction("Edit", new { id = existing.Id });
            }

            string imgError;
            if (!ImageHelper.Validate(imageFile, false, out imgError))
            {
                ModelState.AddModelError("", imgError);
            }

            if (!ModelState.IsValid)
            {
                model.AccommodationName = acc.Name;
                return View(model);
            }

            string image = "";
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string savedName;
                string saveError;
                if (!ImageHelper.Save(imageFile, Server.MapPath("~/Content/images/"), out savedName, out saveError))
                {
                    ModelState.AddModelError("", saveError);
                    model.AccommodationName = acc.Name;
                    return View(model);
                }
                image = savedName;
            }

            ReviewRepository.Add(new Review
            {
                AccommodationId = model.AccommodationId,
                ReviewerUsername = username,
                Title = model.Title,
                Content = model.Content,
                Rating = model.Rating,
                Image = image,
                Status = ReviewStatus.Created,
                IsDeleted = false
            });

            TempData["Success"] = "Review submitted - pending approval.";
            return RedirectToAction("Index", "Reservations");
        }

        // GET: Reviews/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Guest)
            {
                TempData["Error"] = "Only guests can edit reviews.";
                return RedirectToAction("Index", "Home");
            }

            var review = ReviewRepository.GetById(id);
            if (review == null || review.ReviewerUsername != username)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction("Index", "Home");
            }

            var acc = AccommodationRepository.GetById(review.AccommodationId);
            return View(new ReviewFormViewModel
            {
                Id = review.Id,
                AccommodationId = review.AccommodationId,
                AccommodationName = acc != null ? acc.Name : "(unavailable)",
                Title = review.Title,
                Content = review.Content,
                Rating = review.Rating,
                ExistingImage = review.Image
            });
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ReviewFormViewModel model, HttpPostedFileBase imageFile)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Guest)
            {
                TempData["Error"] = "Only guests can edit reviews.";
                return RedirectToAction("Index", "Home");
            }

            var review = model.Id.HasValue ? ReviewRepository.GetById(model.Id.Value) : null;
            if (review == null || review.ReviewerUsername != username)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction("Index", "Home");
            }

            string imgError;
            if (!ImageHelper.Validate(imageFile, false, out imgError))
            {
                ModelState.AddModelError("", imgError);
            }

            if (!ModelState.IsValid)
            {
                var accInvalid = AccommodationRepository.GetById(review.AccommodationId);
                model.AccommodationName = accInvalid != null ? accInvalid.Name : "(unavailable)";
                model.ExistingImage = review.Image;
                return View(model);
            }

            review.Title = model.Title;
            review.Content = model.Content;
            review.Rating = model.Rating;
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string savedName;
                string saveError;
                if (!ImageHelper.Save(imageFile, Server.MapPath("~/Content/images/"), out savedName, out saveError))
                {
                    ModelState.AddModelError("", saveError);
                    var accSave = AccommodationRepository.GetById(review.AccommodationId);
                    model.AccommodationName = accSave != null ? accSave.Name : "(unavailable)";
                    model.ExistingImage = review.Image;
                    return View(model);
                }
                review.Image = savedName;
            }
            review.Status = ReviewStatus.Created;
            ReviewRepository.Update(review);

            TempData["Success"] = "Review updated - pending approval.";
            return RedirectToAction("Index", "Reservations");
        }

        // POST: Reviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }
            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role != Role.Guest)
            {
                TempData["Error"] = "Only guests can delete reviews.";
                return RedirectToAction("Index", "Home");
            }

            var review = ReviewRepository.GetById(id);
            if (review == null || review.ReviewerUsername != username)
            {
                TempData["Error"] = "Review not found.";
                return RedirectToAction("Index", "Reservations");
            }

            ReviewRepository.Delete(id);
            TempData["Success"] = "Review deleted.";
            return RedirectToAction("Index", "Reservations");
        }

        private static bool HasCompleted(string username, int accommodationId)
        {
            return ReservationRepository.GetAll().Any(x =>
                x.GuestUsername == username &&
                x.AccommodationId == accommodationId &&
                x.Status == ReservationStatus.Completed);
        }

        private static Review FindOwnReview(string username, int accommodationId)
        {
            return ReviewRepository.GetAll().FirstOrDefault(x =>
                x.ReviewerUsername == username &&
                x.AccommodationId == accommodationId);
        }
    }
}
