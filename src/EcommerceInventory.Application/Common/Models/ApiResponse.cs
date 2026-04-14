namespace EcommerceInventory.Application.Common.Models;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public ApiResponse() { }

    public ApiResponse(bool success, T data, string? message = null)
    {
        Success = success;
        Data = data;
        Message = message;
    }

    public ApiResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}
