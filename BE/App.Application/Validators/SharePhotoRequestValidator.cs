using FluentValidation;
using App.Application.Dto;

namespace App.Application.Validation;

public class SharePhotoRequestValidator : AbstractValidator<SharePhotoRequest>
{
    public SharePhotoRequestValidator()
    {
        RuleFor(x => x.TargetEmail)
            .NotEmpty().WithMessage("Target email is required.")
            .EmailAddress().WithMessage("Target email format is invalid.");
    }
}
