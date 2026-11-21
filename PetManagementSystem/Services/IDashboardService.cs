using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetAsync();
}
