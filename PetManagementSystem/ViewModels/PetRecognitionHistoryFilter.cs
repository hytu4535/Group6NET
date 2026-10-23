using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels;

public sealed class PetRecognitionHistoryFilter
{
    [Range(1, int.MaxValue, ErrorMessage = "Số trang phải lớn hơn 0.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "Kích thước trang phải từ 1 đến 100.")]
    public int PageSize { get; set; } = 10;

    [Range(1, int.MaxValue, ErrorMessage = "Mã thú cưng không hợp lệ.")]
    public int? PetId { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? FromDate { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? ToDate { get; set; }
}
