namespace App.Domain.Models
{
    public class PhotoShare
    {
        public Guid Id { get; set; }

        public Guid PhotoId { get; set; }
        public Photo Photo { get; set; } = default!;

        public Guid SharedWithUserId { get; set; }
        public ApplicationUser SharedWithUser { get; set; } = default!;

        public DateTime SharedAt { get; set; }
    }
}
