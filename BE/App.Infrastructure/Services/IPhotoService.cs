using Microsoft.AspNetCore.Http;
using App.Domain.Models;

namespace App.Infrastructure.Services
{
    public interface IPhotoService
    {
        Task<Photo> UploadAsync(Guid ownerId, IFormFile file, string? title);
        Task<Photo?> GetAccessiblePhotoAsync(Guid currentUserId, Guid photoId);
        Task<IEnumerable<Photo>> GetOwnPhotosAsync(Guid ownerId);
        Task<IEnumerable<Photo>> GetSharedWithMeAsync(Guid userId);
        Task ShareAsync(Guid ownerId, Guid photoId, Guid targetUserId);
    }
}
