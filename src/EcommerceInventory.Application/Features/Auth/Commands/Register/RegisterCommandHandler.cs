using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Auth.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Enums;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IDateTimeService _dateTimeService;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<User> userRepository,
        ITokenService tokenService,
        IEmailService emailService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _dateTimeService = dateTimeService;
    }

    public async Task<Result<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.Query().AnyAsync(u => u.Email == request.Email.ToLower(), cancellationToken))
        {
            return Result<RegisterResponseDto>.FailureResult("Email is already registered");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user entity
        var user = User.Create(
            request.FullName.Trim(),
            request.Email.Trim().ToLower(),
            passwordHash,
            request.Phone?.Trim());

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Generate and send email verification token
        // This will be implemented when we create the EmailVerificationToken service

        var response = new RegisterResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Status = user.Status.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt
        };

        return Result<RegisterResponseDto>.SuccessResult(response, "Registration successful. Please check your email to verify your account.");
    }
}
