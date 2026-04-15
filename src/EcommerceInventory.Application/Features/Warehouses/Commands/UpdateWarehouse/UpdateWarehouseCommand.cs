using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Warehouses.DTOs;
using EcommerceInventory.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.UpdateWarehouse;

public record UpdateWarehouseCommand : IRequest<Result<WarehouseDto>>
{
    public Guid Id { get; set; }
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

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID is required");

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

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWarehouseCommandHandler(
        IRepository<Warehouse> warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WarehouseDto>> Handle(UpdateWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await _warehouseRepository.Query()
            .Include(w => w.Manager)
            .FirstOrDefaultAsync(w => w.Id == request.Id && !w.DeletedAt.HasValue, ct);

        if (warehouse == null)
            return Result<WarehouseDto>.FailureResult("Warehouse not found");

        // Check if code is unique (excluding current warehouse)
        var codeExists = await _warehouseRepository.Query()
            .AnyAsync(w => w.Code == request.Code.ToUpper() && w.Id != request.Id && !w.DeletedAt.HasValue, ct);

        if (codeExists)
            return Result<WarehouseDto>.FailureResult("Warehouse code already exists");

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

        // Update warehouse using entity method
        warehouse.Update(
            request.Name,
            request.Code,
            address,
            request.ManagerId,
            request.Phone
        );

        await _unitOfWork.SaveChangesAsync(ct);

        // Map to DTO
        var dto = MapToDto(warehouse);

        return Result<WarehouseDto>.SuccessResult(dto, "Warehouse updated successfully");
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
