using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Suppliers.Commands.DeleteSupplier;

public record DeleteSupplierCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand, Result<bool>>
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSupplierCommandHandler(
        IRepository<Supplier> supplierRepository,
        IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _supplierRepository.Query()
            .FirstOrDefaultAsync(s => s.Id == request.Id && !s.DeletedAt.HasValue, ct);

        if (supplier == null)
            return Result<bool>.FailureResult("Supplier not found");

        // Soft delete
        supplier.Delete();

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.SuccessResult(true, "Supplier deleted successfully");
    }
}
