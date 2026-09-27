using System.Globalization;
using PetManagementSystem.Models;

namespace PetManagementSystem.Helpers;

public static class CommerceFormat
{
    private static readonly CultureInfo Vietnamese = new("vi-VN");

    public static string Money(decimal value) =>
        value.ToString("#,##0.##", Vietnamese) + "đ";

    public static string Date(DateTime? value) =>
        value?.ToString("dd/MM/yyyy", Vietnamese) ?? "-";

    public static string DateTimeText(DateTime? value) =>
        value?.ToString("dd/MM/yyyy HH:mm", Vietnamese) ?? "-";

    /// <summary>Tên hiển thị của người dùng: ưu tiên họ tên, sau đó tới username.</summary>
    public static string DisplayName(User? user) =>
        string.IsNullOrWhiteSpace(user?.FullName) ? user?.Username ?? "-" : user!.FullName!;
}
