using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.ViewModels.Commerce;

public class CategoryRowViewModel
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CategoryListViewModel : PagedListViewModel<CategoryRowViewModel>
{
    public string? Search { get; set; }
}

public class CategoryUpsertViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "Mô tả tối đa 255 ký tự")]
    public string? Description { get; set; }
}
