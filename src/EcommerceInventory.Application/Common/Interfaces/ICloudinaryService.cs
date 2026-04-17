namespace EcommerceInventory.Application.Common.Interfaces;

public class CloudinaryUploadResult
{
    public string PublicId  { get; set; } = string.Empty;
    public string SecureUrl { get; set; } = string.Empty;
    public string Format    { get; set; } = string.Empty;
    public long   Bytes     { get; set; }
    public int    Width     { get; set; }
    public int    Height    { get; set; }
}

public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadImageAsync(Stream fileStream, string fileName, string contentType, string folder);
    Task<bool> DeleteImageAsync(string publicId);
}
