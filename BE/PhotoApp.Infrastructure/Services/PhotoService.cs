using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using App.Domain.Models;

namespace App.Infrastructure.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly AppDbContext _db;
        private readonly IFileStorage _fileStorage;

        public PhotoService(AppDbContext db, IFileStorage fileStorage)
        {
            _db = db;
            _fileStorage = fileStorage;
        }

        public async Task<Photo> UploadAsync(Guid ownerId, IFormFile file, string? title)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Empty file");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                throw new ArgumentException("Invalid image type");

            await using var stream = file.OpenReadStream();
            var storedFileName = await _fileStorage.SaveAsync(stream, ext);

            var photo = new Photo
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                FileName = storedFileName,
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                UploadedAt = DateTime.UtcNow,
                Title = title
            };

            _db.Photos.Add(photo);
            await _db.SaveChangesAsync();

            return photo;
        }

        public async Task<Photo?> GetAccessiblePhotoAsync(Guid currentUserId, Guid photoId)
        {
            var photo = await _db.Photos
                .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo == null)
                return null;

            if (photo.OwnerId == currentUserId)
                return photo;

            var shared = await _db.PhotoShares.AnyAsync(ps =>
                ps.PhotoId == photoId && ps.SharedWithUserId == currentUserId);

            return shared ? photo : null;
        }

        public async Task<IEnumerable<Photo>> GetOwnPhotosAsync(Guid ownerId)
        {
            return await _db.Photos
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Photo>> GetSharedWithMeAsync(Guid userId)
        {
            return await _db.PhotoShares
                .Include(ps => ps.Photo)
                .Where(ps => ps.SharedWithUserId == userId)
                .Select(ps => ps.Photo)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();
        }

        public async Task ShareAsync(Guid ownerId, Guid photoId, Guid targetUserId)
        {
            var photo = await _db.Photos.FirstOrDefaultAsync(p => p.Id == photoId);
            if (photo == null || photo.OwnerId != ownerId)
                throw new InvalidOperationException("Photo not found or not owned by user");

            var exists = await _db.PhotoShares.AnyAsync(ps =>
                ps.PhotoId == photoId && ps.SharedWithUserId == targetUserId);
            if (exists)
                return;

            var share = new PhotoShare
            {
                Id = Guid.NewGuid(),
                PhotoId = photoId,
                SharedWithUserId = targetUserId,
                SharedAt = DateTime.UtcNow
            };

            _db.PhotoShares.Add(share);
            await _db.SaveChangesAsync();
        }
    }
}
