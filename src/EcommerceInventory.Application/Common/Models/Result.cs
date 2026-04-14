namespace EcommerceInventory.Application.Common.Models;

/// <summary>
/// Generic result wrapper for operations
/// </summary>
public class Result<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }

    public static Result<T> SuccessResult(T data, string? message = null)
    {
        return new Result<T> { Success = true, Data = data, Message = message };
    }

    public static Result<T> FailureResult(List<string> errors, string? message = null)
    {
        return new Result<T> { Success = false, Errors = errors, Message = message };
    }

    public static Result<T> FailureResult(string error, string? message = null)
    {
        return new Result<T> { Success = false, Errors = new List<string> { error }, Message = message };
    }
}

/// <summary>
/// Non-generic result wrapper
/// </summary>
public class Result
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }

    public static Result SuccessResult(string? message = null)
    {
        return new Result { Success = true, Message = message };
    }

    public static Result FailureResult(List<string> errors, string? message = null)
    {
        return new Result { Success = false, Errors = errors, Message = message };
    }

    public static Result FailureResult(string error, string? message = null)
    {
        return new Result { Success = false, Errors = new List<string> { error }, Message = message };
    }
}
