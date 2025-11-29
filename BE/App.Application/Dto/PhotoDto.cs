namespace App.Application.Dto
{
    public record PhotoDto(
    Guid Id,
    string Title,
    string OriginalFileName,
    DateTime UploadedAt,
    bool IsOwner);
}
