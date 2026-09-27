namespace PetManagementSystem.Services;

public class ServiceResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }

    /// <summary>Tên thuộc tính của ViewModel gây lỗi (để hiển thị cạnh ô nhập). Rỗng = lỗi chung.</summary>
    public string? Field { get; init; }

    public static ServiceResult Ok() => new() { Success = true };

    public static ServiceResult Fail(string error, string? field = null) =>
        new() { Success = false, Error = error, Field = field };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };

    public new static ServiceResult<T> Fail(string error, string? field = null) =>
        new() { Success = false, Error = error, Field = field };
}
