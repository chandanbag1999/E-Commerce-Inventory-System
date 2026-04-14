using EcommerceInventory.Application.Features.Users.Commands.UploadProfileImage;
using FluentValidation;

namespace EcommerceInventory.Application.Features.Users.Commands.UploadProfileImage;

public class UploadProfileImageCommandValidator : AbstractValidator<UploadProfileImageCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const int MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

    public UploadProfileImageCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.File)
            .NotNull().WithMessage("File is required")
            .Must(file => AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
            .WithMessage("Only JPG, JPEG, and PNG images are allowed")
            .Must(file => file.Length <= MaxFileSizeInBytes)
            .WithMessage("File size must not exceed 5MB");
    }
}
