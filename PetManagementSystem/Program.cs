using System.Text.Json;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Data;
using PetManagementSystem.Filters;
using PetManagementSystem.Helpers;
using PetManagementSystem.Middleware;
using PetManagementSystem.Models;
using PetManagementSystem.Repositories;
using PetManagementSystem.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IVeterinarianRepository, VeterinarianRepository>();
builder.Services.AddScoped<IPetRepository, PetRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IChatConversationRepository, ChatConversationRepository>();
builder.Services.AddHttpClient<IClientAiChatService, ClientAiChatService>(client =>
    client.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddScoped<IClientCartRepository, ClientCartRepository>();
builder.Services.AddScoped<IClientOrderRepository, ClientOrderRepository>();
builder.Services.AddScoped<IStorefrontRepository, StorefrontRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IAppointmentManagerService, AppointmentManagerService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IChatConversationService, ChatConversationService>();
builder.Services.AddScoped<IClientCartService, ClientCartService>();
builder.Services.AddScoped<IClientOrderService, ClientOrderService>();
builder.Services.AddScoped<IStorefrontService, StorefrontService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();

// Commerce
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IImportReceiptRepository, ImportReceiptRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImportReceiptService, ImportReceiptService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = async context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Vui lòng đăng nhập."));
                return;
            }
            context.Response.Redirect(context.RedirectUri);
        };
        options.Events.OnRedirectToAccessDenied = async context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Bạn không có quyền thực hiện thao tác này."));
                return;
            }
            context.Response.Redirect(context.RedirectUri);
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddPermissionPolicies();
builder.Services.AddScoped<AdminAccessFilter>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        await context.HttpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Fail("Bạn đã gửi nhiều câu hỏi trong thời gian ngắn. Vui lòng thử lại sau."),
            cancellationToken);
    };
    options.AddPolicy("ai-chat", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

builder.Services.AddHttpClient<IPetRecognitionService, PetRecognitionService>(
    (serviceProvider, client) =>
    {
        var configuration = serviceProvider
            .GetRequiredService<IConfiguration>();

        var baseUrl = configuration["AIService:BaseUrl"];

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "AIService:BaseUrl is not configured.");
        }

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(60);
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Đảm bảo bảng orders có cột thông tin người nhận (họ tên, email). Idempotent; tương đương Database/AddOrderReceiverInfo.sql.
    try
    {
        dbContext.Database.ExecuteSqlRaw(
            "IF COL_LENGTH('dbo.orders', 'receiver_name') IS NULL ALTER TABLE dbo.orders ADD receiver_name NVARCHAR(150) NULL; " +
            "IF COL_LENGTH('dbo.orders', 'receiver_email') IS NULL ALTER TABLE dbo.orders ADD receiver_email NVARCHAR(150) NULL;");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[SETUP] Could not ensure orders receiver columns: {ex.Message}");
    }

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    var adminUser = dbContext.Users.FirstOrDefault(x => x.Username == "admin01");
    if (adminUser != null)
    {
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Huy123456");
        dbContext.SaveChanges();
        Console.WriteLine("[SETUP] Updated admin01 password hash for Huy123456 successfully.");
    }

    var adminRole = dbContext.Roles
        .Include(role => role.RolePermissions)
        .FirstOrDefault(role => role.Name.ToLower() == "admin" && role.Status == "active");
    if (adminRole != null)
    {
        var permissionIds = dbContext.Permissions.Select(permission => permission.Id).ToList();
        var assignedPermissionIds = adminRole.RolePermissions.Select(rolePermission => rolePermission.PermissionId).ToHashSet();
        var missingRolePermissions = permissionIds
            .Where(permissionId => !assignedPermissionIds.Contains(permissionId))
            .Select(permissionId => new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (missingRolePermissions.Count > 0)
        {
            dbContext.RolePermissions.AddRange(missingRolePermissions);
            dbContext.SaveChanges();
            Console.WriteLine($"[SETUP] Granted admin role {missingRolePermissions.Count} additional permissions.");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<AuditUserActionMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=UserLogin}/{id?}");
app.MapControllers();

app.Run();
