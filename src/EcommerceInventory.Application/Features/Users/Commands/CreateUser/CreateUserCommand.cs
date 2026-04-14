using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Users.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<Result<UserResponseDto>>
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;
    private readonly ITokenService _tokenService;

    public CreateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<User> userRepository,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<UserResponseDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower(), cancellationToken);

        if (existingUser != null)
        {
            throw new BusinessRuleViolationException($"User with email '{request.Email}' already exists");
        }

        // Hash password
        var passwordHash = _tokenService.HashPassword(request.Password);

        // Create user using domain factory
        var user = User.Create(
            request.FullName,
            request.Email,
            passwordHash,
            request.Phone
        );

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to response DTO
        var response = new UserResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            ProfileImageUrl = user.ProfileImageUrl,
            Status = user.Status.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            LastLoginAt = user.LastLoginAt,
            Roles = new List<string>(), // No roles assigned yet
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return Result<UserResponseDto>.SuccessResult(response, "User created successfully");
    }
}
