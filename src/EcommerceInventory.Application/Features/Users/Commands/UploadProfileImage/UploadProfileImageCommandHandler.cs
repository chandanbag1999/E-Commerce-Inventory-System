using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.UploadProfileImage;

public class UploadProfileImageCommandHandler
    : IRequestHandler<UploadProfileImageCommand, string>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICloudinaryService _cloudinary;
    private readonly ICurrentUserService _currentUser;

    public UploadProfileImageCommandHandler(IUnitOfWork uow,
                                             ICloudinaryService cloudinary,
                                             ICurrentUserService currentUser)
    {
        _uow         = uow;
        _cloudinary  = cloudinary;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(
        UploadProfileImageCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _uow.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.UserId);

        if (_currentUser.UserId != request.UserId &&
            !_currentUser.HasPermission("Users.Edit"))
            throw new UnauthorizedException(
                "You can only update your own profile image.");

        var oldCloudinaryId = user.CloudinaryProfileId;

        var result = await _cloudinary.UploadImageAsync(
            request.FileStream, request.FileName,
            request.ContentType, "profiles");

        if (!string.IsNullOrEmpty(oldCloudinaryId))
        {
            _ = Task.Run(async () =>
            {
                try { await _cloudinary.DeleteImageAsync(oldCloudinaryId); }
                catch { }
            }, CancellationToken.None);
        }

        user.SetProfileImage(result.SecureUrl, result.PublicId);
        await _uow.SaveChangesAsync(cancellationToken);

        return result.SecureUrl;
    }
}
