using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Users.DTOs;
using EcommerceInventory.Domain.Entities;
using EcommerceInventory.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Users.Commands.UploadProfileImage;

public record UploadProfileImageCommand : IRequest<Result<UserResponseDto>>
{
    public Guid UserId { get; set; }
    public IFormFile File { get; set; } = null!;
}

public class UploadProfileImageCommandHandler : IRequestHandler<UploadProfileImageCommand, Result<UserResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<User> _userRepository;
    private readonly ICloudinaryService _cloudinaryService;

    public UploadProfileImageCommandHandler(
        IUnitOfWork unitOfWork,
        IRepository<User> userRepository,
        ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<Result<UserResponseDto>> Handle(UploadProfileImageCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // Delete old profile image if exists
        if (!string.IsNullOrEmpty(user.CloudinaryProfileId))
        {
            await _cloudinaryService.DeleteImageAsync(user.CloudinaryProfileId);
        }

        // Upload new image
        var (imageUrl, cloudinaryId) = await _cloudinaryService.UploadImageAsync(request.File, "users");

        user.SetProfileImage(imageUrl, cloudinaryId);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

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
            Roles = roles,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        return Result<UserResponseDto>.SuccessResult(response, "Profile image uploaded successfully");
    }
}
