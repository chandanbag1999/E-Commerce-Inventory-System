using MediatR;

namespace EcommerceInventory.Application.Features.Categories.Commands.UploadCategoryImage;

public class UploadCategoryImageCommand : IRequest<string>
{
    public Guid   CategoryId  { get; set; }
    public Stream FileStream   { get; set; } = null!;
    public string FileName     { get; set; } = string.Empty;
    public string ContentType  { get; set; } = string.Empty;
}
