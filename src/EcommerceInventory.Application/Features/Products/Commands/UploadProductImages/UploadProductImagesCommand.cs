using MediatR;

namespace EcommerceInventory.Application.Features.Products.Commands.UploadProductImages;

public class UploadProductImagesCommand : IRequest<List<string>>
{
    public Guid ProductId { get; set; }
    public List<ProductFileUpload> Files { get; set; } = new();
}

public class ProductFileUpload
{
    public Stream FileStream  { get; set; } = null!;
    public string FileName    { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
