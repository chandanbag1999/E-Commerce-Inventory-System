using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly IRepository<Category> _categoryRepository;

    public DeleteCategoryCommandHandler(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken ct)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (category == null)
            return Result<bool>.FailureResult("Category not found");

        if (category.Children.Any(c => c.IsActive))
            return Result<bool>.FailureResult("Cannot delete category with active children");

        category.Delete();
        _categoryRepository.Update(category);

        return Result<bool>.SuccessResult(true, "Category deleted successfully");
    }
}