namespace PetManagementSystem.Helpers;
using PetManagementSystem.Models;
public static class StringExtensions
{
    /// <summary>Cắt khoảng trắng hai đầu; chuỗi rỗng trả về null.</summary>
    public static string? TrimToNull(this string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>Bỏ khoảng trắng, dấu chấm, gạch ngang trong số điện thoại ("0912 345.678" -> "0912345678").</summary>
    public static string NormalizePhone(this string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : System.Text.RegularExpressions.Regex.Replace(value, @"[\s.\-()]", string.Empty);
}
