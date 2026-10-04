using PetManagementSystem.Models;
using PetManagementSystem.ViewModels.Appointment;

namespace PetManagementSystem.Services;

public interface IAppointmentManagerService
{
    Task<ServiceResult<Appointment>> CreateAppointmentAsync(int userId, BookingInputModel model);
    Task<ServiceResult> CancelAppointmentAsync(int userId, int appointmentId, string cancelReason, string cancelledBy);
    Task<ServiceResult> UpdateAppointmentStatusAsync(int appointmentId, byte newStatus, int? vetId = null);
    Task<IEnumerable<Appointment>> GetAppointmentsByUserAsync(int userId);
    Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, int? userId = null);
    Task<IEnumerable<Service>> GetActiveServicesAsync();
    Task<IEnumerable<Pet>> GetPetsByUserAsync(int userId);
}
