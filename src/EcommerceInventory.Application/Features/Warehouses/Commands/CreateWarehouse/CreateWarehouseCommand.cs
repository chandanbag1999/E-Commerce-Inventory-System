using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.CreateWarehouse;

public record CreateWarehouseCommand : IRequest<Result<WarehouseDto>>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }
    public Guid? ManagerId { get; set; }
    public string? Phone { get; set; }
}

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Warehouse code is required")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWarehouseCommandHandler(
        IRepository<Warehouse> warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WarehouseDto>> Handle(CreateWarehouseCommand request, CancellationToken ct)
    {
        // Check if code already exists
        var exists = await _warehouseRepository.Query()
            .AnyAsync(w => w.Code == request.Code.ToUpper() && !w.DeletedAt.HasValue, ct);

        if (exists)
            return Result<WarehouseDto>.FailureResult("Warehouse code already exists");

        // If managerId provided, verify it exists
        if (request.ManagerId.HasValue)
        {
            // We'll check via repository - this is a simple existence check
            // In a full implementation, you'd inject IUserRepository or similar
        }

        // Build Address value object if address fields provided
        Domain.ValueObjects.Address? address = null;
        if (!string.IsNullOrWhiteSpace(request.Street) ||
            !string.IsNullOrWhiteSpace(request.City) ||
            !string.IsNullOrWhiteSpace(request.State))
        {
            address = new Domain.ValueObjects.Address(
                request.Street?.Trim() ?? string.Empty,
                request.City?.Trim() ?? string.Empty,
                request.State?.Trim() ?? string.Empty,
                request.Pincode?.Trim() ?? string.Empty,
                request.Country?.Trim() ?? "India"
            );
        }

        // Create warehouse using factory method
        var warehouse = Warehouse.Create(
            request.Name,
            request.Code,
            address,
            request.ManagerId,
            request.Phone
        );

        await _warehouseRepository.AddAsync(warehouse, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Map to DTO
        var dto = MapToDto(warehouse);

        return Result<WarehouseDto>.SuccessResult(dto, "Warehouse created successfully");
    }

    private static WarehouseDto MapToDto(Warehouse warehouse)
    {
        AddressDto? addressDto = null;
        if (warehouse.Address != null)
        {
            addressDto = new AddressDto(
                warehouse.Address.Street,
                warehouse.Address.City,
                warehouse.Address.State,
                warehouse.Address.Pincode,
                warehouse.Address.Country
            );
        }

        return new WarehouseDto(
            warehouse.Id,
            warehouse.Name,
            warehouse.Code,
            addressDto,
            warehouse.ManagerId,
            warehouse.Manager?.FullName,
            warehouse.Phone,
            warehouse.IsActive,
            warehouse.CreatedAt,
            warehouse.UpdatedAt
        );
    }
}
