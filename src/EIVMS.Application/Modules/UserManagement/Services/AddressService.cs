using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.UserManagement.DTOs.Address;
using EIVMS.Application.Modules.UserManagement.Interfaces;
using EIVMS.Domain.Entities.UserManagement;

namespace EIVMS.Application.Modules.UserManagement.Services;

public class AddressService : IAddressService
{
    private readonly IUserManagementRepository _repository;

    public AddressService(IUserManagementRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<AddressResponseDto>>> GetUserAddressesAsync(Guid userId)
    {
        var addresses = await _repository.GetUserAddressesAsync(userId);
        var dtos = addresses.Where(a => !a.IsDeleted).Select(MapToDto).ToList();
        return ApiResponse<List<AddressResponseDto>>.SuccessResponse(dtos);
    }

    public async Task<ApiResponse<AddressResponseDto>> GetAddressByIdAsync(Guid addressId, Guid userId)
    {
        var address = await _repository.GetAddressByIdAsync(addressId);
        if (address == null || address.IsDeleted)
            return ApiResponse<AddressResponseDto>.ErrorResponse("Address not found", 404);
        if (address.UserId != userId)
            return ApiResponse<AddressResponseDto>.ErrorResponse("Access denied", 403);
        return ApiResponse<AddressResponseDto>.SuccessResponse(MapToDto(address));
    }

    public async Task<ApiResponse<AddressResponseDto>> CreateAddressAsync(Guid userId, CreateAddressDto dto)
    {
        var existingAddresses = await _repository.GetUserAddressesAsync(userId);
        var activeCount = existingAddresses.Count(a => !a.IsDeleted);
        if (activeCount >= 10)
            return ApiResponse<AddressResponseDto>.ErrorResponse("Maximum 10 addresses allowed per user");

        var address = Address.Create(userId, dto.Type, dto.Label, dto.Street, dto.City, dto.State, dto.Country, dto.ZipCode, dto.ContactName, dto.ContactPhone);

        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            address.SetGeoCoordinates(dto.Latitude.Value, dto.Longitude.Value);

        if (dto.SetAsDefault || activeCount == 0)
        {
            await _repository.UnsetAllDefaultAddressesAsync(userId);
            address.SetAsDefault();
        }

        var created = await _repository.AddAddressAsync(address);
        return ApiResponse<AddressResponseDto>.SuccessResponse(MapToDto(created), "Address added successfully", 201);
    }

    public async Task<ApiResponse<AddressResponseDto>> UpdateAddressAsync(Guid addressId, Guid userId, CreateAddressDto dto)
    {
        var address = await _repository.GetAddressByIdAsync(addressId);
        if (address == null || address.IsDeleted)
            return ApiResponse<AddressResponseDto>.ErrorResponse("Address not found", 404);
        if (address.UserId != userId)
            return ApiResponse<AddressResponseDto>.ErrorResponse("Access denied", 403);

        address.Update(dto.Label, dto.Street, dto.City, dto.State, dto.Country, dto.ZipCode, dto.ContactName, dto.ContactPhone);

        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            address.SetGeoCoordinates(dto.Latitude.Value, dto.Longitude.Value);

        var updated = await _repository.UpdateAddressAsync(address);
        return ApiResponse<AddressResponseDto>.SuccessResponse(MapToDto(updated), "Address updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteAddressAsync(Guid addressId, Guid userId)
    {
        var address = await _repository.GetAddressByIdAsync(addressId);
        if (address == null || address.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Address not found", 404);
        if (address.UserId != userId)
            return ApiResponse<bool>.ErrorResponse("Access denied", 403);

        if (address.IsDefault)
        {
            var addresses = await _repository.GetUserAddressesAsync(userId);
            var otherActive = addresses.FirstOrDefault(a => !a.IsDeleted && a.Id != addressId);
            if (otherActive != null)
                otherActive.SetAsDefault();
        }

        address.SoftDelete();
        await _repository.UpdateAddressAsync(address);

        return ApiResponse<bool>.SuccessResponse(true, "Address deleted successfully");
    }

    public async Task<ApiResponse<bool>> SetDefaultAddressAsync(Guid addressId, Guid userId)
    {
        var address = await _repository.GetAddressByIdAsync(addressId);
        if (address == null || address.IsDeleted)
            return ApiResponse<bool>.ErrorResponse("Address not found", 404);
        if (address.UserId != userId)
            return ApiResponse<bool>.ErrorResponse("Access denied", 403);

        await _repository.UnsetAllDefaultAddressesAsync(userId);
        address.SetAsDefault();
        await _repository.UpdateAddressAsync(address);

        return ApiResponse<bool>.SuccessResponse(true, "Default address updated");
    }

    private static AddressResponseDto MapToDto(Address address)
    {
        return new AddressResponseDto
        {
            Id = address.Id,
            Type = address.Type,
            Label = address.Label,
            Street = address.Street,
            City = address.City,
            State = address.State,
            Country = address.Country,
            ZipCode = address.ZipCode,
            ContactName = address.ContactName,
            ContactPhone = address.ContactPhone,
            IsDefault = address.IsDefault,
            IsValidated = address.IsValidated,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            CreatedAt = address.CreatedAt
        };
    }
}