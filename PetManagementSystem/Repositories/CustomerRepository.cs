using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Models;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Repositories;

public sealed class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    private static IQueryable<User> Customers(IQueryable<User> query) =>
        query.Where(user => user.Role != null &&
            user.Status != "deleted" &&
            (user.Role.Name.ToLower() == "member" || user.Role.Name.ToLower() == "customer"));

    public async Task<PagedResult<CustomerListItemViewModel>> SearchAsync(
        string? search,
        string? status,
        string? sort,
        bool descending,
        int page,
        int pageSize)
    {
        var query = Customers(dbContext.Users.AsNoTracking());
        var normalizedSearch = search?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            var pattern = normalizedSearch.ToLower();
            query = query.Where(user =>
                (user.FullName ?? string.Empty).ToLower().Contains(pattern) ||
                (user.Username ?? string.Empty).ToLower().Contains(pattern) ||
                (user.Email ?? string.Empty).ToLower().Contains(pattern) ||
                (user.PhoneNumber ?? string.Empty).ToLower().Contains(pattern));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(user => user.Status == status);
        }

        query = sort?.ToLowerInvariant() switch
        {
            "fullname" => descending ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
            "email" => descending ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
            "phone" => descending ? query.OrderByDescending(x => x.PhoneNumber) : query.OrderBy(x => x.PhoneNumber),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "pets" => descending ? query.OrderByDescending(x => dbContext.Pets.Count(p => p.UserId == x.Id)) : query.OrderBy(x => dbContext.Pets.Count(p => p.UserId == x.Id)),
            _ => descending ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new CustomerListItemViewModel
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Address ?? string.Empty,
                Status = user.Status,
                PetCount = dbContext.Pets.Count(pet => pet.UserId == user.Id),
                CreatedAt = user.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<CustomerListItemViewModel>(items, totalCount, page, pageSize);
    }

    public Task<User?> GetByIdAsync(int id) =>
        Customers(dbContext.Users.Include(user => user.Role))
            .FirstOrDefaultAsync(user => user.Id == id);

    public async Task<CustomerDetailViewModel?> GetDetailsAsync(int id)
    {
        var customer = await Customers(dbContext.Users.AsNoTracking())
            .FirstOrDefaultAsync(user => user.Id == id);
        if (customer is null)
        {
            return null;
        }

        var pets = await dbContext.Pets.AsNoTracking()
            .Where(pet => pet.UserId == id)
            .OrderByDescending(pet => pet.CreatedAt)
            .ToListAsync();
        var appointments = await dbContext.Appointments.AsNoTracking()
            .Include(appointment => appointment.Pet)
            .Where(appointment => appointment.UserId == id)
            .OrderByDescending(appointment => appointment.AppointmentDate)
            .ThenByDescending(appointment => appointment.AppointmentTime)
            .ToListAsync();
        var orders = await dbContext.Orders.AsNoTracking()
            .Where(order => order.UserId == id)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        return new CustomerDetailViewModel
        {
            Customer = customer,
            Pets = pets,
            Appointments = appointments,
            Orders = orders
        };
    }

    public async Task<bool> ExistsRelatedDataAsync(int id) =>
        await dbContext.Pets.AnyAsync(pet => pet.UserId == id) ||
        await dbContext.Appointments.AnyAsync(appointment => appointment.UserId == id) ||
        await dbContext.Orders.AnyAsync(order => order.UserId == id) ||
        await dbContext.Feedbacks.AnyAsync(feedback => feedback.UserId == id);

    public async Task AddAsync(User customer)
    {
        await dbContext.Users.AddAsync(customer);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(User customer)
    {
        dbContext.Users.Update(customer);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(User customer)
    {
        dbContext.Users.Remove(customer);
        await dbContext.SaveChangesAsync();
    }
}
