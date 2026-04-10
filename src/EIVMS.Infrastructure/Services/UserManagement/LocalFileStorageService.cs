using EIVMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EIVMS.Infrastructure.Services.UserManagement;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalFileStorageService(IConfiguration config)
    {
        _basePath = config["FileStorage:BasePath"] ?? "wwwroot/uploads";
        _baseUrl = config["FileStorage:BaseUrl"] ?? "http://localhost:5001/uploads";
    }

    public async Task<string> UploadAsync(IFormFile file, string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return GetPublicUrl(path);
    }

    public async Task<bool> DeleteAsync(string fileUrl)
    {
        try
        {
            var relativePath = fileUrl.Replace(_baseUrl, "").TrimStart('/');
            var fullPath = Path.Combine(_basePath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public string GetPublicUrl(string path)
    {
        return $"{_baseUrl}/{path}".Replace("\\", "/");
    }
}