using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Application.Common.Models;
using EcommerceInventory.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceInventory.Application.Features.Warehouses.Commands.DeleteWarehouse;

public record DeleteWarehouseCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Result<bool>>
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWarehouseCommandHandler(
        IRepository<Warehouse> warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await _warehouseRepository.Query()
            .FirstOrDefaultAsync(w => w.Id == request.Id && !w.DeletedAt.HasValue, ct);

        if (warehouse == null)
            return Result<bool>.FailureResult("Warehouse not found");

        // Soft delete
        warehouse.Delete();

        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.SuccessResult(true, "Warehouse deleted successfully");
    }
}
