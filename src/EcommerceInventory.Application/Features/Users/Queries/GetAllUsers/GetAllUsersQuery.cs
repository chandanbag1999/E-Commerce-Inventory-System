using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Users.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQuery : IRequest<PagedResult<UserListDto>>
{
    public int     PageNumber  { get; set; } = 1;
    public int     PageSize    { get; set; } = 20;
    public string? SearchTerm  { get; set; }
    public string? Status      { get; set; }
    public string? SortBy      { get; set; } = "CreatedAt";
    public bool    SortDesc    { get; set; } = true;
}
