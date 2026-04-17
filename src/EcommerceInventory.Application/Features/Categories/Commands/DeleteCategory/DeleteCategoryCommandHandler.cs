using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _uow.Categories.GetByIdAsync(
            request.Id, cancellationToken);

        if (category == null)
            throw new NotFoundException("Category", request.Id);

        var hasProducts = await _uow.Products.Query()
            .AnyAsync(p => p.CategoryId == request.Id, cancellationToken);

        if (hasProducts)
            throw new BusinessRuleViolationException(
                "Cannot delete category with existing products.");

        category.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);
        return true;
    }
}
