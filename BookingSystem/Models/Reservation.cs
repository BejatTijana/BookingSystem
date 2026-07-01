namespace BookingSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string GuestUsername { get; set; }
        public int AccommodationId { get; set; }
        public string CheckInDate { get; set; }
        public string CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; }
        public bool IsDeleted { get; set; }
    }
}
