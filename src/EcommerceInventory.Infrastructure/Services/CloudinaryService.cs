using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EcommerceInventory.Application.Common.Interfaces;
using EcommerceInventory.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcommerceInventory.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IOptions<CloudinarySettings> settings, ILogger<CloudinaryService> logger)
    {
        _logger = logger;
        var account = new Account(settings.Value.CloudName, settings.Value.ApiKey, settings.Value.ApiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<CloudinaryUploadResult> UploadImageAsync(Stream fileStream, string fileName, string contentType, string folder)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File is empty.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/jpg" };
        if (!allowedTypes.Contains(contentType.ToLower()))
            throw new ArgumentException("Only JPEG, PNG, and WebP images are allowed.");

        const long maxSize = 5 * 1024 * 1024;
        if (fileStream.Length > maxSize)
            throw new ArgumentException("File size must not exceed 5MB.");

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new Exception("Image upload failed: " + result.Error.Message);
        }

        _logger.LogInformation("Image uploaded to Cloudinary: {PublicId}", result.PublicId);

        return new CloudinaryUploadResult
        {
            PublicId = result.PublicId,
            SecureUrl = result.SecureUrl.ToString(),
            Format = result.Format,
            Bytes = result.Bytes,
            Width = result.Width,
            Height = result.Height
        };
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return false;

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Result == "ok")
        {
            _logger.LogInformation("Image deleted from Cloudinary: {PublicId}", publicId);
            return true;
        }

        _logger.LogWarning("Cloudinary delete returned: {Result} for {PublicId}", result.Result, publicId);
        return false;
    }
}
