using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Users.DTOs;
using EcommerceInventory.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler
    : IRequestHandler<GetAllUsersQuery, PagedResult<UserListDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllUsersQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<UserListDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _uow.Users.Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<UserStatus>(request.Status, out var status))
        {
            query = query.Where(u => u.Status == status);
        }

        query = request.SortBy?.ToLower() switch
        {
            "fullname" => request.SortDesc
                ? query.OrderByDescending(u => u.FullName)
                : query.OrderBy(u => u.FullName),
            "email" => request.SortDesc
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "status" => request.SortDesc
                ? query.OrderByDescending(u => u.Status)
                : query.OrderBy(u => u.Status),
            _ => request.SortDesc
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(u => new UserListDto
        {
            Id              = u.Id,
            FullName        = u.FullName,
            Email           = u.Email,
            Phone           = u.Phone,
            ProfileImageUrl = u.ProfileImageUrl,
            Status          = u.Status.ToString(),
            IsEmailVerified = u.IsEmailVerified,
            Roles           = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt       = u.CreatedAt
        }).ToList();

        return PagedResult<UserListDto>.Create(dtos, total,
            request.PageNumber, request.PageSize);
    }
}
