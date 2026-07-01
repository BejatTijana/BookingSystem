namespace BookingSystem.Models
{
    public enum Role
    {
        Guest,
        Host,
        Admin
    }

    public enum Gender
    {
        Male,
        Female
    }

    public enum AccommodationType
    {
        Hotel,
        Apartment,
        Hostel
    }

    public enum ReservationStatus
    {
        Created,
        Approved,
        Cancelled,
        Completed
    }

    public enum ReviewStatus
    {
        Created,
        Approved,
        Rejected
    }
}
