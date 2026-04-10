using EIVMS.Application.Common.Models;
using EIVMS.Application.Modules.UserManagement.DTOs.Address;

namespace EIVMS.Application.Modules.UserManagement.Interfaces;

public interface IAddressService
{
    Task<ApiResponse<List<AddressResponseDto>>> GetUserAddressesAsync(Guid userId);
    Task<ApiResponse<AddressResponseDto>> GetAddressByIdAsync(Guid addressId, Guid userId);
    Task<ApiResponse<AddressResponseDto>> CreateAddressAsync(Guid userId, CreateAddressDto dto);
    Task<ApiResponse<AddressResponseDto>> UpdateAddressAsync(Guid addressId, Guid userId, CreateAddressDto dto);
    Task<ApiResponse<bool>> DeleteAddressAsync(Guid addressId, Guid userId);
    Task<ApiResponse<bool>> SetDefaultAddressAsync(Guid addressId, Guid userId);
}