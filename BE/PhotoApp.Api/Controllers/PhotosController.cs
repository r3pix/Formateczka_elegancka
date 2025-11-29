using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using App.Application.Dto;
using App.Domain.Models;
using App.Infrastructure.Services;
using System.Security.Claims;

namespace App.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PhotosController : ControllerBase
    {
        private readonly IPhotoService _photoService;
        private readonly IFileStorage _fileStorage;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhotosController(
            IPhotoService photoService,
            IFileStorage fileStorage,
            UserManager<ApplicationUser> userManager)
        {
            _photoService = photoService;
            _fileStorage = fileStorage;
            _userManager = userManager;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IEnumerable<PhotoDto>> GetMyPhotos()
        {
            var photos = await _photoService.GetOwnPhotosAsync(CurrentUserId);
            return photos.Select(p => new PhotoDto(
                p.Id,
                p.Title ?? string.Empty,
                p.OriginalFileName,
                p.UploadedAt,
                IsOwner: true
            ));
        }

        [HttpGet("shared-with-me")]
        public async Task<IEnumerable<PhotoDto>> GetSharedWithMe()
        {
            var photos = await _photoService.GetSharedWithMeAsync(CurrentUserId);
            return photos.Select(p => new PhotoDto(
                p.Id,
                p.Title ?? string.Empty,
                p.OriginalFileName,
                p.UploadedAt,
                IsOwner: false
            ));
        }

        [HttpPost("upload")]
        [RequestSizeLimit(10_000_000)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadPhotoRequest uploadPhotoRequest)
        {
            if (uploadPhotoRequest.File == null)
                return BadRequest("File is required");

            var photo = await _photoService.UploadAsync(CurrentUserId, uploadPhotoRequest.File, uploadPhotoRequest.Title);
            return Ok(new { photo.Id });
        }

        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download(Guid id)
        {
            var photo = await _photoService.GetAccessiblePhotoAsync(CurrentUserId, id);
            if (photo == null)
                return NotFound();

            var stream = await _fileStorage.OpenReadAsync(photo.FileName);
            return File(stream, photo.ContentType, photo.OriginalFileName);
        }

        [HttpPost("{id:guid}/share")]
        public async Task<IActionResult> Share(Guid id, [FromBody] SharePhotoRequest request)
        {
            var targetUser = await _userManager.FindByEmailAsync(request.TargetEmail);
            if (targetUser == null)
                return BadRequest("Target user not found");

            await _photoService.ShareAsync(CurrentUserId, id, targetUser.Id);
            return Ok();
        }
    }
}
