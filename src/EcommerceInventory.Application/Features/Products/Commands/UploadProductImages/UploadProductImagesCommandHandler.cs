using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Commands.UploadProductImages;

public class UploadProductImagesCommandHandler
    : IRequestHandler<UploadProductImagesCommand, List<string>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICloudinaryService _cloudinary;

    public UploadProductImagesCommandHandler(IUnitOfWork uow,
                                              ICloudinaryService cloudinary)
    {
        _uow       = uow;
        _cloudinary = cloudinary;
    }

    public async Task<List<string>> Handle(
        UploadProductImagesCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _uow.Products.Query()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId,
                                 cancellationToken);

        if (product == null)
            throw new NotFoundException("Product", request.ProductId);

        var hasPrimary = product.Images.Any(i => i.IsPrimary);
        var nextOrder  = product.Images.Any()
            ? product.Images.Max(i => i.DisplayOrder) + 1
            : 0;

        var uploadTasks = request.Files.Select(f =>
            _cloudinary.UploadImageAsync(
                f.FileStream, f.FileName, f.ContentType, "products"));

        var results = await Task.WhenAll(uploadTasks);
        var imageUrls = new List<string>();

        for (int i = 0; i < results.Length; i++)
        {
            var image = ProductImage.Create(
                productId:    request.ProductId,
                cloudinaryId: results[i].PublicId,
                imageUrl:     results[i].SecureUrl,
                isPrimary:    !hasPrimary && i == 0,
                displayOrder: nextOrder + i);

            await _uow.ProductImages.AddAsync(image, cancellationToken);
            imageUrls.Add(results[i].SecureUrl);
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return imageUrls;
    }
}
