namespace EIVMS.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public T? Data { get; private set; }
    public List<string> Errors { get; private set; } = new();
    public int StatusCode { get; private set; }

    private ApiResponse() { }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode,
            Errors = new List<string>()
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            StatusCode = statusCode,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, string error, int statusCode = 400)
    {
        return ErrorResponse(message, statusCode, new List<string> { error });
    }
}