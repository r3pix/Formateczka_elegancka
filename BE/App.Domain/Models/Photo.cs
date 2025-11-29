namespace App.Domain.Models
{
    public class Photo
    {
        public Guid Id { get; set; }

        public Guid OwnerId { get; set; }
        public ApplicationUser Owner { get; set; } = default!;

        public string FileName { get; set; } = default!;
        public string OriginalFileName { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public DateTime UploadedAt { get; set; }
        public string? Title { get; set; }
    }
}
