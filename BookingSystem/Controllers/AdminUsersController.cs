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
    public class AdminUsersController : Controller
    {
        private const string DateFormat = "dd/MM/yyyy";

        // GET: AdminUsers
        [HttpGet]
        public ActionResult Index(string firstName, string lastName, DateTime? bornFrom, DateTime? bornTo, Role? role, string sortBy, string sortDir)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            IEnumerable<User> users = UserRepository.GetAll().Where(u => u.Role != Role.Admin);

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                users = users.Where(u => !string.IsNullOrEmpty(u.FirstName) &&
                    u.FirstName.IndexOf(firstName, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                users = users.Where(u => !string.IsNullOrEmpty(u.LastName) &&
                    u.LastName.IndexOf(lastName, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (bornFrom.HasValue || bornTo.HasValue)
            {
                users = users.Where(u =>
                {
                    DateTime dob;
                    if (!DateTime.TryParseExact(u.DateOfBirth, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dob))
                    {
                        return false;
                    }
                    if (bornFrom.HasValue && dob.Date < bornFrom.Value.Date) return false;
                    if (bornTo.HasValue && dob.Date > bornTo.Value.Date) return false;
                    return true;
                });
            }
            if (role.HasValue)
            {
                users = users.Where(u => u.Role == role.Value);
            }

            var sorted = ApplySort(users, sortBy, sortDir).ToList();

            return View(new AdminUsersViewModel
            {
                Items = sorted,
                SearchFirstName = firstName,
                SearchLastName = lastName,
                BornFrom = bornFrom,
                BornTo = bornTo,
                RoleFilter = role,
                SortBy = sortBy,
                SortDir = sortDir
            });
        }

        // GET: AdminUsers/CreateHost
        [HttpGet]
        public ActionResult CreateHost()
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            return View(new AddHostViewModel());
        }

        // POST: AdminUsers/CreateHost
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateHost(AddHostViewModel model)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            if (model.DateOfBirth.HasValue && model.DateOfBirth.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError("DateOfBirth", "Date of birth cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (UserRepository.ExistsUsername(model.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                DateOfBirth = model.DateOfBirth.Value.ToString(DateFormat, CultureInfo.InvariantCulture),
                Gender = model.Gender,
                Role = Role.Host,
                IsDeleted = false
            };
            UserRepository.Add(user);

            TempData["Success"] = "Host added.";
            return RedirectToAction("Index");
        }

        // GET: AdminUsers/Edit/5
        [HttpGet]
        public ActionResult Edit(string username)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role == Role.Admin)
            {
                TempData["Error"] = "User not found or cannot be edited.";
                return RedirectToAction("Index");
            }

            DateTime parsed;
            DateTime? dob = DateTime.TryParseExact(user.DateOfBirth, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed)
                ? parsed
                : (DateTime?)null;

            return View(new EditUserViewModel
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

        // POST: AdminUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string username, EditUserViewModel model)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role == Role.Admin)
            {
                TempData["Error"] = "User not found or cannot be edited.";
                return RedirectToAction("Index");
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
                // ponovo postavi read-only polja jer ih forma ne šalje nazad
                model.Username = user.Username;
                model.Role = user.Role;
                return View(model);
            }

            // Username i Role se nikad ne mijenjaju ovdje — uzimamo iz baze, ne iz forme
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

            TempData["Success"] = "User updated.";
            return RedirectToAction("Index");
        }

        // POST: AdminUsers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string username)
        {
            string currentUsername;
            var gate = RequireAdmin(out currentUsername);
            if (gate != null) return gate;

            var user = UserRepository.GetByUsername(username);
            if (user == null || user.Role == Role.Admin)
            {
                TempData["Error"] = "User not found or cannot be deleted.";
                return RedirectToAction("Index");
            }
            if (username == currentUsername)
            {
                TempData["Error"] = "Cannot delete your own account.";
                return RedirectToAction("Index");
            }

            int cancelled = 0;
            if (user.Role == Role.Guest)
            {
                var active = ReservationRepository.GetAll()
                    .Where(r => r.GuestUsername == username
                        && (r.Status == ReservationStatus.Created || r.Status == ReservationStatus.Approved))
                    .ToList();
                foreach (var r in active)
                {
                    r.Status = ReservationStatus.Cancelled;
                    ReservationRepository.Update(r);
                    cancelled++;
                }
            }

            UserRepository.Delete(username);

            TempData["Success"] = cancelled > 0
                ? string.Format("User deleted. {0} reservation{1} cancelled.", cancelled, cancelled == 1 ? "" : "s")
                : "User deleted.";
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

        private static IEnumerable<User> ApplySort(IEnumerable<User> items, string sortBy, string sortDir)
        {
            if (string.IsNullOrEmpty(sortBy)) return items;
            bool desc = sortDir == "desc";
            switch (sortBy)
            {
                case "dob":
                    return desc ? items.OrderByDescending(u => ParseDob(u.DateOfBirth))
                                : items.OrderBy(u => ParseDob(u.DateOfBirth));
                case "role":
                    return desc ? items.OrderByDescending(u => u.Role)
                                : items.OrderBy(u => u.Role);
                case "firstName":
                    return desc ? items.OrderByDescending(u => u.FirstName)
                                : items.OrderBy(u => u.FirstName);
                default:
                    return items;
            }
        }

        private static DateTime ParseDob(string s)
        {
            DateTime d;
            return DateTime.TryParseExact(s, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                ? d
                : DateTime.MinValue;
        }
    }
}
