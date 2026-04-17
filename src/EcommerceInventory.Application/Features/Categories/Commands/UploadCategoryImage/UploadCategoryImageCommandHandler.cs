using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;

namespace EcommerceInventory.Application.Features.Categories.Commands.UploadCategoryImage;

public class UploadCategoryImageCommandHandler
    : IRequestHandler<UploadCategoryImageCommand, string>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICloudinaryService _cloudinary;

    public UploadCategoryImageCommandHandler(IUnitOfWork uow,
                                              ICloudinaryService cloudinary)
    {
        _uow       = uow;
        _cloudinary = cloudinary;
    }

    public async Task<string> Handle(
        UploadCategoryImageCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _uow.Categories.GetByIdAsync(
            request.CategoryId, cancellationToken);

        if (category == null)
            throw new NotFoundException("Category", request.CategoryId);

        var oldId = category.CloudinaryId;

        var result = await _cloudinary.UploadImageAsync(
            request.FileStream, request.FileName,
            request.ContentType, "categories");

        if (!string.IsNullOrEmpty(oldId))
        {
            _ = Task.Run(async () =>
            {
                try { await _cloudinary.DeleteImageAsync(oldId); }
                catch { }
            }, CancellationToken.None);
        }

        category.SetImage(result.SecureUrl, result.PublicId);
        await _uow.SaveChangesAsync(cancellationToken);

        return result.SecureUrl;
    }
}
