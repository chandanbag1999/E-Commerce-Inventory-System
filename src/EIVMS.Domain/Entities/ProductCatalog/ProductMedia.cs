using EIVMS.Domain.Common;
using EIVMS.Domain.Enums.ProductCatalog;

namespace EIVMS.Domain.Entities.ProductCatalog;

public class ProductMedia : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string? ThumbnailUrl { get; private set; }
    public MediaType Type { get; private set; } = MediaType.Image;
    public string? AltText { get; private set; }
    public string? Title { get; private set; }
    public int DisplayOrder { get; private set; } = 0;
    public bool IsPrimary { get; private set; } = false;
    public string? FileName { get; private set; }
    public long? FileSizeBytes { get; private set; }
    public string? MimeType { get; private set; }
    public int? WidthPx { get; private set; }
    public int? HeightPx { get; private set; }
    public int? DurationSeconds { get; private set; }
    public string? VideoProvider { get; private set; }
    public bool IsDeleted { get; private set; } = false;

    public Product Product { get; private set; } = null!;

    private ProductMedia() { }

    public static ProductMedia Create(
        Guid productId, string url, MediaType type = MediaType.Image,
        string? altText = null, int displayOrder = 0, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Media URL is required");

        return new ProductMedia
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Url = url.Trim(),
            Type = type,
            AltText = altText,
            DisplayOrder = displayOrder,
            IsPrimary = isPrimary,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetAsPrimary() { IsPrimary = true; SetUpdatedAt(); }
    public void UnsetPrimary() { IsPrimary = false; SetUpdatedAt(); }
    public void UpdateDisplayOrder(int order) { DisplayOrder = order; SetUpdatedAt(); }

    public void SetFileInfo(string? fileName, long? fileSizeBytes, string? mimeType, int? widthPx = null, int? heightPx = null)
    {
        FileName = fileName;
        FileSizeBytes = fileSizeBytes;
        MimeType = mimeType;
        WidthPx = widthPx;
        HeightPx = heightPx;
        SetUpdatedAt();
    }

    public void SetThumbnail(string thumbnailUrl) { ThumbnailUrl = thumbnailUrl; SetUpdatedAt(); }
    public void UpdateAltText(string? altText) { AltText = altText; SetUpdatedAt(); }
    public void SoftDelete() { IsDeleted = true; IsPrimary = false; SetUpdatedAt(); }
}
