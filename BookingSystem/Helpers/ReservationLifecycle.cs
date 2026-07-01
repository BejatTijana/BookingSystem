using System;
using System.Globalization;
using BookingSystem.DataAccess;
using BookingSystem.Models;

namespace BookingSystem.Helpers
{
    public static class ReservationLifecycle
    {
        private const string DateFormat = "dd/MM/yyyy";

        // pri pokretanju: Approved rezervacije čiji je checkout u prošlosti  postavljamo na Completed da gosti mogu ostaviti recenziju
        public static void EnsureCompletedForPastCheckouts()
        {
            foreach (var r in ReservationRepository.GetAll())
            {
                if (r.Status != ReservationStatus.Approved)
                {
                    continue;
                }
                DateTime checkOut;
                if (!DateTime.TryParseExact(r.CheckOutDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out checkOut))
                {
                    continue;
                }
                if (checkOut.Date < DateTime.Today)
                {
                    r.Status = ReservationStatus.Completed;
                    ReservationRepository.Update(r);
                }
            }
        }
    }
}
