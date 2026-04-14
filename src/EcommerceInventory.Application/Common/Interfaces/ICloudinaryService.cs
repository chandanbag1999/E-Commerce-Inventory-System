using Microsoft.AspNetCore.Http;

namespace EcommerceInventory.Application.Common.Interfaces;

/// <summary>
/// Service for uploading images to Cloudinary
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Uploads an image to Cloudinary
    /// </summary>
    /// <returns>Tuple of (SecureUrl, PublicId)</returns>
    Task<(string SecureUrl, string PublicId)> UploadImageAsync(IFormFile file, string folder);

    /// <summary>
    /// Deletes an image from Cloudinary by public ID
    /// </summary>
    Task DeleteImageAsync(string publicId);
}
