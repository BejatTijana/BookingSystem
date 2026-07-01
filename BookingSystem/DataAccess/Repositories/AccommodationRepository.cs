using System.Collections.Generic;
using System.Linq;
using BookingSystem.Models;

namespace BookingSystem.DataAccess
{
    public static class AccommodationRepository
    {
        private const string FileName = "accommodations.json";

        public static List<Accommodation> GetAll()
        {
            return JsonStore.ReadAll<Accommodation>(FileName).Where(x => !x.IsDeleted).ToList();
        }

        public static Accommodation GetById(int id)
        {
            return JsonStore.ReadAll<Accommodation>(FileName).FirstOrDefault(x => !x.IsDeleted && x.Id == id);
        }

        public static void Add(Accommodation accommodation)
        {
            var all = JsonStore.ReadAll<Accommodation>(FileName);
            accommodation.Id = all.Any() ? all.Max(x => x.Id) + 1 : 1;
            all.Add(accommodation);
            JsonStore.WriteAll(FileName, all);
        }

        public static void Update(Accommodation accommodation)
        {
            var all = JsonStore.ReadAll<Accommodation>(FileName);
            int index = all.FindIndex(x => x.Id == accommodation.Id);
            if (index >= 0)
            {
                all[index] = accommodation;
                JsonStore.WriteAll(FileName, all);
            }
        }

        public static void Delete(int id)
        {
            var all = JsonStore.ReadAll<Accommodation>(FileName);
            var existing = all.FirstOrDefault(x => x.Id == id);
            if (existing != null)
            {
                existing.IsDeleted = true;
                JsonStore.WriteAll(FileName, all);
            }
        }
    }
}
