using System;
using System.Globalization;
using System.Web.Mvc;
using BookingSystem.DataAccess;
using BookingSystem.Models;
using BookingSystem.ViewModels;

namespace BookingSystem.Controllers
{
    public class ProfileController : Controller
    {
        private const string DateFormat = "dd/MM/yyyy";

        // GET: Profile
        [HttpGet]
        public ActionResult Index()
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = UserRepository.GetByUsername(username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(new ProfileViewModel
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Role = user.Role
            });
        }

        // GET: Profile/Edit
        [HttpGet]
        public ActionResult Edit()
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = UserRepository.GetByUsername(username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            DateTime parsed;
            DateTime? dob = DateTime.TryParseExact(user.DateOfBirth, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
                ? parsed
                : (DateTime?)null;

            return View(new EditProfileViewModel
            {
                Username = user.Username,
                Role = user.Role,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = dob,
                Gender = user.Gender,
                NewPassword = null
            });
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProfileViewModel model)
        {
            var username = Session["Username"] as string;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = UserRepository.GetByUsername(username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (model.DateOfBirth.HasValue && model.DateOfBirth.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future.");
            }
            if (!string.IsNullOrEmpty(model.NewPassword) && model.NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "Password must be at least 6 characters.");
            }

            if (!ModelState.IsValid)
            {
                model.Username = user.Username;
                model.Role = user.Role;
                return View(model);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.DateOfBirth = model.DateOfBirth.Value.ToString(DateFormat, CultureInfo.InvariantCulture);
            user.Gender = model.Gender;
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.Password = model.NewPassword;
            }

            UserRepository.Update(user);

            TempData["Success"] = "Profile updated.";
            return RedirectToAction("Index");
        }
    }
}
