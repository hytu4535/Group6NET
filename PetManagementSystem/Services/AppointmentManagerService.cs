using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.ViewModels.Appointment;

namespace PetManagementSystem.Services;

public class AppointmentManagerService(
    AppDbContext dbContext,
    ILogger<AppointmentManagerService> logger) : IAppointmentManagerService
{
    private const int MaxCapacityPerSlot = 5;

    public async Task<ServiceResult<Appointment>> CreateAppointmentAsync(int userId, BookingInputModel model)
    {
        try
        {
            // 1. Validate Pet ownership
            var pet = await dbContext.Pets.FirstOrDefaultAsync(p => p.Id == model.PetId && p.UserId == userId);
            if (pet == null)
            {
                return ServiceResult<Appointment>.Fail("Thú cưng không tồn tại hoặc không thuộc sở hữu của bạn.", nameof(model.PetId));
            }

            model.ServiceIds ??= new List<int>();

            // 1.5. Include package services if ServicePackageId is provided
            if (model.ServicePackageId.HasValue && model.ServicePackageId.Value > 0)
            {
                var activePackage = await dbContext.PetPackages
                    .Include(pp => pp.ServicePackage)
                        .ThenInclude(sp => sp.ServicePackageServices)
                    .FirstOrDefaultAsync(pp => pp.PetId == model.PetId && pp.PackageId == model.ServicePackageId.Value && pp.Status == 1 && pp.EndDate >= DateTime.Now);

                if (activePackage != null && activePackage.ServicePackage?.ServicePackageServices != null)
                {
                    foreach (var sps in activePackage.ServicePackage.ServicePackageServices)
                    {
                        if (!model.ServiceIds.Contains(sps.ServiceId))
                        {
                            model.ServiceIds.Add(sps.ServiceId);
                        }
                    }
                }
            }

            // 2. Validate Services
            if (model.ServiceIds == null || !model.ServiceIds.Any())
            {
                return ServiceResult<Appointment>.Fail("Vui lòng chọn ít nhất một dịch vụ.", nameof(model.ServiceIds));
            }

            var services = await dbContext.Services
                .Where(s => model.ServiceIds.Contains(s.Id) && s.Status == 1)
                .ToListAsync();

            if (services.Count != model.ServiceIds.Count)
            {
                return ServiceResult<Appointment>.Fail("Một hoặc nhiều dịch vụ đã chọn không khả dụng hoặc đã ngừng hoạt động.", nameof(model.ServiceIds));
            }

            // 3. Validate Date & Time
            var bookingDateTime = model.AppointmentDate.Date.Add(model.AppointmentTime);
            if (bookingDateTime <= DateTime.Now)
            {
                return ServiceResult<Appointment>.Fail("Không thể đặt lịch hẹn trong thời gian quá khứ.", nameof(model.AppointmentDate));
            }

            if (bookingDateTime < DateTime.Now.AddHours(2))
            {
                return ServiceResult<Appointment>.Fail("Lịch hẹn phải được đặt trước ít nhất 2 giờ so với thời gian hiện tại.", nameof(model.AppointmentTime));
            }

            var startWorkingHour = new TimeSpan(8, 0, 0);
            var endWorkingHour = new TimeSpan(20, 0, 0);
            if (model.AppointmentTime < startWorkingHour || model.AppointmentTime > endWorkingHour)
            {
                return ServiceResult<Appointment>.Fail("Giờ hẹn phải nằm trong giờ làm việc của cửa hàng (08:00 - 20:00).", nameof(model.AppointmentTime));
            }

            // 4. Check Slot Conflict / Overload (Max capacity per slot/hour)
            var existingSlotCount = await dbContext.Appointments
                .CountAsync(a => a.AppointmentDate.Date == model.AppointmentDate.Date &&
                                 a.AppointmentTime.Hours == model.AppointmentTime.Hours &&
                                 a.Status != 5); // Status 5 = Cancelled

            if (existingSlotCount >= MaxCapacityPerSlot)
            {
                return ServiceResult<Appointment>.Fail("Khung giờ này đã đạt số lượng lịch hẹn tối đa. Vui lòng chọn khung giờ khác.", nameof(model.AppointmentTime));
            }

            // 5. Check Pet Overlapping Appointments
            var dayAppointments = await dbContext.Appointments
                .Where(a => a.PetId == model.PetId &&
                               a.AppointmentDate.Date == model.AppointmentDate.Date &&
                               a.Status != 5)
                .ToListAsync();

            var petOverlap = dayAppointments.Any(a => Math.Abs((a.AppointmentTime - model.AppointmentTime).TotalHours) < 1.5);

            if (petOverlap)
            {
                return ServiceResult<Appointment>.Fail("Thú cưng này đã có lịch hẹn khác trùng hoặc sát thời gian trong ngày.", nameof(model.AppointmentDate));
            }

            // 6. Database Transaction for creating appointment and appointment services
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                var appointment = new Appointment
                {
                    PetId = model.PetId,
                    UserId = userId,
                    AppointmentDate = model.AppointmentDate.Date,
                    AppointmentTime = model.AppointmentTime,
                    Reason = model.Reason,
                    Status = 1 // Pending (Chờ xác nhận)
                };

                dbContext.Appointments.Add(appointment);
                await dbContext.SaveChangesAsync();

                foreach (var service in services)
                {
                    dbContext.AppointmentServices.Add(new PetManagementSystem.Models.AppointmentService
                    {
                        AppointmentId = appointment.Id,
                        ServiceId = service.Id,
                        PriceAtBooking = service.Price
                    });
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Reload with navigations
                var createdAppointment = await GetAppointmentByIdAsync(appointment.Id, userId);
                return ServiceResult<Appointment>.Ok(createdAppointment ?? appointment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Lỗi khi thực hiện transaction đặt lịch hẹn.");
                return ServiceResult<Appointment>.Fail("Đã xảy ra lỗi hệ thống khi lưu lịch hẹn.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi tạo lịch hẹn.");
            return ServiceResult<Appointment>.Fail("Không thể tạo lịch hẹn do lỗi hệ thống.");
        }
    }

    public async Task<ServiceResult> CancelAppointmentAsync(int userId, int appointmentId, string cancelReason, string cancelledBy)
    {
        try
        {
            var appointment = await dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null)
            {
                return ServiceResult.Fail("Không tìm thấy lịch hẹn.");
            }

            // Check permissions (User can cancel own appointment, Admin can cancel any)
            if (cancelledBy == "User" && appointment.UserId != userId)
            {
                return ServiceResult.Fail("Bạn không có quyền hủy lịch hẹn này.");
            }

            // Check Status: Can only cancel if Pending (1) or Confirmed (2)
            if (appointment.Status == 3)
            {
                return ServiceResult.Fail("Không thể hủy lịch hẹn đang trong quá trình khám.");
            }
            if (appointment.Status == 4)
            {
                return ServiceResult.Fail("Không thể hủy lịch hẹn đã hoàn thành.");
            }
            if (appointment.Status == 5)
            {
                return ServiceResult.Fail("Lịch hẹn này đã bị hủy trước đó.");
            }

            // Check time constraint for User cancellation (e.g., at least 24h before appointment time)
            if (cancelledBy == "User")
            {
                var appointmentDateTime = appointment.AppointmentDate.Add(appointment.AppointmentTime);
                if (appointmentDateTime.AddHours(-24) < DateTime.Now)
                {
                    return ServiceResult.Fail("Chỉ được phép hủy lịch hẹn trước giờ hẹn ít nhất 24 giờ.");
                }
            }

            appointment.Status = 5; // Cancelled
            appointment.CancelledBy = cancelledBy;
            appointment.CancelReason = cancelReason;

            dbContext.Appointments.Update(appointment);
            await dbContext.SaveChangesAsync();

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi hủy lịch hẹn ID: {AppointmentId}", appointmentId);
            return ServiceResult.Fail("Không thể hủy lịch hẹn do lỗi hệ thống.");
        }
    }

    public async Task<ServiceResult> UpdateAppointmentStatusAsync(int appointmentId, byte newStatus, int? vetId = null)
    {
        try
        {
            var appointment = await dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null)
            {
                return ServiceResult.Fail("Không tìm thấy lịch hẹn.");
            }

            appointment.Status = newStatus;
            if (vetId.HasValue && vetId.Value > 0)
            {
                appointment.VetId = vetId.Value;
            }

            dbContext.Appointments.Update(appointment);
            await dbContext.SaveChangesAsync();

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Lỗi khi cập nhật trạng thái lịch hẹn ID: {AppointmentId}", appointmentId);
            return ServiceResult.Fail("Không thể cập nhật trạng thái lịch hẹn.");
        }
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByUserAsync(int userId)
    {
        return await dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
                .ThenInclude(v => v!.User)
            .Include(a => a.AppointmentServices)
                .ThenInclude(asv => asv.Service)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId, int? userId = null)
    {
        var query = dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.User)
            .Include(a => a.Veterinarian)
                .ThenInclude(v => v!.User)
            .Include(a => a.AppointmentServices)
                .ThenInclude(asv => asv.Service)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(a => a.Id == appointmentId && a.UserId == userId.Value);
        }
        else
        {
            query = query.Where(a => a.Id == appointmentId);
        }

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Service>> GetActiveServicesAsync()
    {
        return await dbContext.Services
            .Where(s => s.Status == 1)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Pet>> GetPetsByUserAsync(int userId)
    {
        return await dbContext.Pets
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
