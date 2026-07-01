using System.Collections.Generic;
using System.Linq;
using BookingSystem.Models;

namespace BookingSystem.DataAccess
{
    public static class ReservationRepository
    {
        private const string FileName = "reservations.json";

        public static List<Reservation> GetAll()
        {
            return JsonStore.ReadAll<Reservation>(FileName).Where(x => !x.IsDeleted).ToList();
        }

        public static Reservation GetById(int id)
        {
            return JsonStore.ReadAll<Reservation>(FileName).FirstOrDefault(x => !x.IsDeleted && x.Id == id);
        }

        public static void Add(Reservation reservation)
        {
            var all = JsonStore.ReadAll<Reservation>(FileName);
            reservation.Id = all.Any() ? all.Max(x => x.Id) + 1 : 1;
            all.Add(reservation);
            JsonStore.WriteAll(FileName, all);
        }

        public static void Update(Reservation reservation)
        {
            var all = JsonStore.ReadAll<Reservation>(FileName);
            int index = all.FindIndex(x => x.Id == reservation.Id);
            if (index >= 0)
            {
                all[index] = reservation;
                JsonStore.WriteAll(FileName, all);
            }
        }

        public static void Delete(int id)
        {
            var all = JsonStore.ReadAll<Reservation>(FileName);
            var existing = all.FirstOrDefault(x => x.Id == id);
            if (existing != null)
            {
                existing.IsDeleted = true;
                JsonStore.WriteAll(FileName, all);
            }
        }
    }
}
