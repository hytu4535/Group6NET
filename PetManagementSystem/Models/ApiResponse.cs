namespace PetManagementSystem.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public List<string> Errors { get; init; } = [];
    public int? Page { get; init; }
    public int? PageSize { get; init; }
    public int? TotalItems { get; init; }

    public static ApiResponse<T> Ok(
        T data,
        string message = "Thành công.",
        int? page = null,
        int? pageSize = null,
        int? totalItems = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public static ApiResponse<T> Fail(string message, params string[] errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors.Length > 0 ? [.. errors] : [message]
        };
    }
}