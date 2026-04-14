using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IRepository<Product> _productRepository;

    public DeleteProductCommandHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.Query()
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product == null)
            return Result<bool>.FailureResult("Product not found");

        product.Delete();
        _productRepository.Update(product);

        return Result<bool>.SuccessResult(true, "Product deleted successfully");
    }
}