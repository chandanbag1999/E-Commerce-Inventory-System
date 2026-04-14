using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Users.DTOs;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Queries.SearchUsers;

public record SearchUsersQuery : IRequest<Result<PagedResult<UserListDto>>>
{
    public string SearchTerm { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, Result<PagedResult<UserListDto>>>
{
    private readonly IRepository<User> _userRepository;

    public SearchUsersQueryHandler(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<UserListDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var searchTerm = request.SearchTerm.ToLower();

        var query = _userRepository.Query()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .Where(u => u.FullName.ToLower().Contains(searchTerm) ||
                        u.Email.ToLower().Contains(searchTerm));

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(u => u.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userDtos = users.Select(u => new UserListDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Phone = u.Phone,
            ProfileImageUrl = u.ProfileImageUrl,
            Status = u.Status.ToString(),
            IsEmailVerified = u.IsEmailVerified,
            LastLoginAt = u.LastLoginAt,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt = u.CreatedAt
        }).ToList();

        var pagedResult = new PagedResult<UserListDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResult<UserListDto>>.SuccessResult(pagedResult);
    }
}
