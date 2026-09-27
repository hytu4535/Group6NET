namespace PetManagementSystem.Helpers;

public static class StringExtensions
{
    /// <summary>Cắt khoảng trắng hai đầu; chuỗi rỗng trả về null.</summary>
    public static string? TrimToNull(this string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
