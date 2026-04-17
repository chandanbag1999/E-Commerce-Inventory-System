using MediatR;

namespace EcommerceInventory.Application.Features.Users.Commands.UploadProfileImage;

public class UploadProfileImageCommand : IRequest<string>
{
    public Guid   UserId      { get; set; }
    public Stream FileStream  { get; set; } = null!;
    public string FileName    { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
