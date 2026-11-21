namespace PetManagementSystem.Helpers;

/// <summary>Chuẩn hóa đường dẫn ảnh sản phẩm để hiển thị, và ảnh thay thế khi ảnh bị lỗi/không tải được.</summary>
public static class ProductImages
{
    public const string Placeholder = "/images/product-placeholder.svg";

    /// <summary>Gắn vào thẻ img: ảnh lỗi thì hiện ảnh thay thế thay vì biểu tượng ảnh vỡ kèm chữ alt.</summary>
    public const string OnError = "this.onerror=null;this.src='/images/product-placeholder.svg';";

    /// <summary>
    /// Trả về đường dẫn dùng được trong thẻ img: giữ nguyên http/https, đường dẫn gốc "/...", data URI;
    /// đường dẫn tương đối ("images/a.jpg", "~/images/a.jpg") được đưa về dạng bắt đầu bằng "/"; rỗng thì dùng ảnh thay thế.
    /// </summary>
    public static string Src(string? imageUrl)
    {
        var value = imageUrl?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return Placeholder;
        }

        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("//", StringComparison.Ordinal)
            || value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        value = value.Replace('\\', '/').TrimStart('~');
        return value.StartsWith('/') ? value : "/" + value;
    }
}
