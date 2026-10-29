using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.Services;

public interface IProductImageStorage
{
    /// <summary>Kiểm tra và lưu ảnh sản phẩm vào wwwroot/uploads/products; trả về đường dẫn dạng /uploads/products/xxx.jpg.</summary>
    Task<ServiceResult<string>> SaveAsync(IFormFile file);

    /// <summary>Đường dẫn ảnh do hệ thống lưu (nằm trong /uploads/products/) chứ không phải link ngoài.</summary>
    bool IsManaged(string? imageUrl);

    /// <summary>Xóa file ảnh nếu là ảnh do hệ thống lưu; link ngoài hoặc đường dẫn khác được bỏ qua.</summary>
    void DeleteIfManaged(string? imageUrl);
}
