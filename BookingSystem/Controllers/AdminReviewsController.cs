using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.ViewModels;

namespace BookingSystem.Controllers
{
    public class AdminReviewsController : Controller
    {
        // GET: AdminReviews
        [HttpGet]
        public ActionResult Index(ReviewStatus? status)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            IEnumerable<Review> all = ReviewRepository.GetAll();
            if (status.HasValue)
            {
                all = all.Where(r => r.Status == status.Value);
            }

            var items = all
                .Select(r => new AdminReviewItemViewModel
                {
                    Review = r,
                    AccommodationName = AccommodationRepository.GetById(r.AccommodationId)?.Name ?? "(unavailable)",
                    CanModerate = CanModerate(r)
                })
                .ToList();

            return View(new AdminReviewsViewModel { Items = items, StatusFilter = status });
        }

        // POST: AdminReviews/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var r = ReviewRepository.GetById(id);
            if (r == null || r.Status != ReviewStatus.Created)
            {
                TempData["Error"] = "Only created reviews can be approved.";
                return RedirectToAction("Index");
            }

            r.Status = ReviewStatus.Approved;
            ReviewRepository.Update(r);
            TempData["Success"] = "Review approved.";
            return RedirectToAction("Index");
        }

        // POST: AdminReviews/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var r = ReviewRepository.GetById(id);
            if (r == null || r.Status != ReviewStatus.Created)
            {
                TempData["Error"] = "Only created reviews can be rejected.";
                return RedirectToAction("Index");
            }

            r.Status = ReviewStatus.Rejected;
            ReviewRepository.Update(r);
            TempData["Success"] = "Review rejected.";
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

        private static bool CanModerate(Review r)
        {
            return r.Status == ReviewStatus.Created;
        }
    }
}
