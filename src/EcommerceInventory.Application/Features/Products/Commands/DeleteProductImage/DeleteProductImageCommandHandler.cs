using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommandHandler
    : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICloudinaryService _cloudinary;

    public DeleteProductImageCommandHandler(IUnitOfWork uow,
                                             ICloudinaryService cloudinary)
    {
        _uow       = uow;
        _cloudinary = cloudinary;
    }

    public async Task<bool> Handle(
        DeleteProductImageCommand request,
        CancellationToken cancellationToken)
    {
        var image = await _uow.ProductImages.Query()
            .FirstOrDefaultAsync(
                i => i.Id        == request.ImageId
                  && i.ProductId == request.ProductId,
                cancellationToken);

        if (image == null)
            throw new NotFoundException("Product image not found.");

        _ = Task.Run(async () =>
        {
            try { await _cloudinary.DeleteImageAsync(image.CloudinaryId); }
            catch { }
        }, CancellationToken.None);

        _uow.ProductImages.Remove(image);
        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}
