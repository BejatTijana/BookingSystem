namespace BookingSystem.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int AccommodationId { get; set; }
        public string ReviewerUsername { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string Image { get; set; }
        public ReviewStatus Status { get; set; }
        public bool IsDeleted { get; set; }
    }
}
