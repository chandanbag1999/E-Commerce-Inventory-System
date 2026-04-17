using EcommerceInventory.Application.Features.Auth.DTOs;
using MediatR;

namespace EcommerceInventory.Application.Features.Auth.Queries.GetMe;

public class GetMeQuery : IRequest<UserInfoDto>
{
    public Guid UserId { get; set; }
}