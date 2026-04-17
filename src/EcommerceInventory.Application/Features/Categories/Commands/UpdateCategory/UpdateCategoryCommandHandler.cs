using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Features.Categories.DTOs;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _uow.Categories.GetByIdAsync(
            request.Id, cancellationToken);

        if (category == null)
            throw new NotFoundException("Category", request.Id);

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
                throw new DomainException("Category cannot be its own parent.");

            var parentExists = await _uow.Categories.ExistsAsync(
                request.ParentId.Value, cancellationToken);
            if (!parentExists)
                throw new NotFoundException("Parent category", request.ParentId.Value);
        }

        category.Update(request.Name, request.Description, request.ParentId);
        category.SetSortOrder(request.SortOrder);

        await _uow.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id          = category.Id,
            Name        = category.Name,
            Slug        = category.Slug,
            Description = category.Description,
            ImageUrl    = category.ImageUrl,
            ParentId    = category.ParentId,
            IsActive    = category.IsActive,
            SortOrder   = category.SortOrder,
            CreatedAt   = category.CreatedAt,
            UpdatedAt   = category.UpdatedAt
        };
    }
}
