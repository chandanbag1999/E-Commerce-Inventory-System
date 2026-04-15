using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using EcommerceInventory.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.UpdateSupplier;

public record UpdateSupplierCommand : IRequest<Result<SupplierDto>>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }
    public string? GstNumber { get; set; }
}

public class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Supplier ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.GstNumber)
            .MaximumLength(15).WithMessage("GST number cannot exceed 15 characters")
            .When(x => !string.IsNullOrEmpty(x.GstNumber));
    }
}

public class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, Result<SupplierDto>>
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupplierCommandHandler(
        IRepository<Supplier> supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SupplierDto>> Handle(UpdateSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _supplierRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.DeletedAt.HasValue, ct);

        if (supplier == null)
            return Result<SupplierDto>.FailureResult("Supplier not found");

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

        // Update supplier using entity method
        supplier.Update(
            request.Name,
            request.ContactName,
            request.Email,
            request.Phone,
            address,
            request.GstNumber
        );

        await _unitOfWork.SaveChangesAsync(ct);

        // Map to DTO
        var dto = MapToDto(supplier);

        return Result<SupplierDto>.SuccessResult(dto, "Supplier updated successfully");
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        AddressDto? addressDto = null;
        if (supplier.Address != null)
        {
            addressDto = new AddressDto(
                supplier.Address.Street,
                supplier.Address.City,
                supplier.Address.State,
                supplier.Address.Pincode,
                supplier.Address.Country
            );
        }

        return new SupplierDto(
            supplier.Id,
            supplier.Name,
            supplier.ContactName,
            supplier.Email,
            supplier.Phone,
            addressDto,
            supplier.GstNumber,
            supplier.IsActive,
            supplier.CreatedAt,
            supplier.UpdatedAt
        );
    }
}
