using System.Collections.Generic;
using System.Linq;
using BookingSystem.Models;

namespace BookingSystem.DataAccess
{
    public static class ReviewRepository
    {
        private const string FileName = "reviews.json";

        public static List<Review> GetAll()
        {
            return JsonStore.ReadAll<Review>(FileName).Where(x => !x.IsDeleted).ToList();
        }

        public static Review GetById(int id)
        {
            return JsonStore.ReadAll<Review>(FileName).FirstOrDefault(x => !x.IsDeleted && x.Id == id);
        }

        public static void Add(Review review)
        {
            var all = JsonStore.ReadAll<Review>(FileName);
            review.Id = all.Any() ? all.Max(x => x.Id) + 1 : 1;
            all.Add(review);
            JsonStore.WriteAll(FileName, all);
        }

        public static void Update(Review review)
        {
            var all = JsonStore.ReadAll<Review>(FileName);
            int index = all.FindIndex(x => x.Id == review.Id);
            if (index >= 0)
            {
                all[index] = review;
                JsonStore.WriteAll(FileName, all);
            }
        }

        public static void Delete(int id)
        {
            var all = JsonStore.ReadAll<Review>(FileName);
            var existing = all.FirstOrDefault(x => x.Id == id);
            if (existing != null)
            {
                existing.IsDeleted = true;
                JsonStore.WriteAll(FileName, all);
            }
        }
    }
}
