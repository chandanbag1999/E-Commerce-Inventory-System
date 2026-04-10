using Microsoft.AspNetCore.Http;

namespace EIVMS.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string path);
    Task<bool> DeleteAsync(string fileUrl);
    string GetPublicUrl(string path);
}