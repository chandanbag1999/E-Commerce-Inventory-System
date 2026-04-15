using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Application.Features.Suppliers.DTOs;
using EcommerceInventory.Domain.Entities;
using FluentValidation;
using MediatR;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand : IRequest<Result<SupplierDto>>
{
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

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
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

public class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, Result<SupplierDto>>
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierCommandHandler(
        IRepository<Supplier> supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken ct)
    {
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

        // Create supplier using factory method
        var supplier = Supplier.Create(
            request.Name,
            request.ContactName,
            request.Email,
            request.Phone,
            address,
            request.GstNumber
        );

        await _supplierRepository.AddAsync(supplier, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Map to DTO
        var dto = MapToDto(supplier);

        return Result<SupplierDto>.SuccessResult(dto, "Supplier created successfully");
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
