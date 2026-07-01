using System.Collections.Generic;
using System.Linq;
using BookingSystem.Models;

namespace BookingSystem.DataAccess
{
    public static class UserRepository
    {
        private const string FileName = "users.json";

        public static List<User> GetAll()
        {
            return JsonStore.ReadAll<User>(FileName).Where(x => !x.IsDeleted).ToList();
        }

        public static User GetByUsername(string username)
        {
            return JsonStore.ReadAll<User>(FileName).FirstOrDefault(x => !x.IsDeleted && x.Username == username);
        }

        public static bool ExistsUsername(string username)
        {
            return JsonStore.ReadAll<User>(FileName).Any(x => x.Username == username);
        }

        public static void Add(User user)
        {
            var all = JsonStore.ReadAll<User>(FileName);
            all.Add(user);
            JsonStore.WriteAll(FileName, all);
        }

        public static void Update(User user)
        {
            var all = JsonStore.ReadAll<User>(FileName);
            int index = all.FindIndex(x => x.Username == user.Username);
            if (index >= 0)
            {
                all[index] = user;
                JsonStore.WriteAll(FileName, all);
            }
        }

        public static void Delete(string username)
        {
            var all = JsonStore.ReadAll<User>(FileName);
            var existing = all.FirstOrDefault(x => x.Username == username);
            if (existing != null)
            {
                existing.IsDeleted = true;
                JsonStore.WriteAll(FileName, all);
            }
        }
    }
}
