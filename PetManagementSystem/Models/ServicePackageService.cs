namespace PetManagementSystem.Models;

public class ServicePackageService
{
    public int PackageId { get; set; }
    public ServicePackage? ServicePackage { get; set; }

    public int ServiceId { get; set; }
    public Service? Service { get; set; }
}
