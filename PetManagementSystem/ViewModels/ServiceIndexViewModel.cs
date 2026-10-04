using PetManagementSystem.Models;

namespace PetManagementSystem.ViewModels;

public class ServiceIndexViewModel
{
    public List<Service> Services { get; set; } = new();
    public List<ServicePackage> Packages { get; set; } = new();
}
