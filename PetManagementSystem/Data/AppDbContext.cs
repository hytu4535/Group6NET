using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Models;

namespace PetManagementSystem.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Identity & System
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();

    // Services & Appointments (Kiên)
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServicePackage> ServicePackages => Set<ServicePackage>();
    public DbSet<ServicePackageService> ServicePackageServices => Set<ServicePackageService>();
    public DbSet<PetPackage> PetPackages => Set<PetPackage>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();
    public DbSet<PetHealthRecord> PetHealthRecords => Set<PetHealthRecord>();
    public DbSet<VaccinationRecord> VaccinationRecords => Set<VaccinationRecord>();
    public DbSet<MedicalPrescription> MedicalPrescriptions => Set<MedicalPrescription>();

    // Pets, Feedback, Notification & Chat (Hsang / Bạn)
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<PetImage> PetImages => Set<PetImage>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();

    // Commerce
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ImportReceipt> ImportReceipts => Set<ImportReceipt>();
    public DbSet<ImportDetail> ImportDetails => Set<ImportDetail>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsUnicode(true);
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.Address).HasColumnName("address").IsUnicode(true);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Description).HasColumnName("description").IsUnicode(true);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(150).IsUnicode(true);
            entity.Property(e => e.Description).HasColumnName("description").IsUnicode(true);
            entity.Property(e => e.Module).HasColumnName("module").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.ToTable("staff");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Position).HasColumnName("position").HasMaxLength(150).IsUnicode(true);
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithOne(u => u.Staff)
                .HasForeignKey<Staff>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.ToTable("veterinarians");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Specialty).HasColumnName("specialty").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.LicenseNo).HasColumnName("license_no").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.YearsOfExperience).HasColumnName("years_of_experience");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithOne(u => u.Veterinarian)
                .HasForeignKey<Veterinarian>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Cấu hình mảng Dịch vụ & Khám bệnh
        ConfigureServicesAndAppointments(modelBuilder);

        // Cấu hình mảng Thương mại & Khách hàng
        ConfigureCommerce(modelBuilder);
    }

    private static void ConfigureServicesAndAppointments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.Description).HasColumnName("description").IsUnicode(true);
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<ServicePackage>(entity =>
        {
            entity.ToTable("service_packages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.Description).HasColumnName("description").IsUnicode(true);
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ValidityDays).HasColumnName("validity_days");
        });

        modelBuilder.Entity<ServicePackageService>(entity =>
        {
            entity.ToTable("service_package_services");
            entity.HasKey(e => new { e.PackageId, e.ServiceId });
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");

            entity.HasOne(e => e.ServicePackage)
                .WithMany(p => p.ServicePackageServices)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Service)
                .WithMany(s => s.ServicePackageServices)
                .HasForeignKey(e => e.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PetPackage>(entity =>
        {
            entity.ToTable("pet_packages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.PackageId).HasColumnName("package_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date").HasColumnType("date");
            entity.Property(e => e.EndDate).HasColumnName("end_date").HasColumnType("date");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.PetPackages)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ServicePackage)
                .WithMany(sp => sp.PetPackages)
                .HasForeignKey(e => e.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VetId).HasColumnName("vet_id");
            entity.Property(e => e.AppointmentDate).HasColumnName("appointment_date").HasColumnType("date");
            entity.Property(e => e.AppointmentTime).HasColumnName("appointment_time");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(e => e.Pet)
                .WithMany()
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Veterinarian)
                .WithMany()
                .HasForeignKey(e => e.VetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AppointmentService>(entity =>
        {
            entity.ToTable("appointments_services");
            entity.HasKey(e => new { e.AppointmentId, e.ServiceId });
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.PriceAtBooking).HasColumnName("price_at_booking").HasColumnType("decimal(10,2)");

            entity.HasOne(es => es.Appointment)
                .WithMany(a => a.AppointmentServices)
                .HasForeignKey(es => es.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(es => es.Service)
                .WithMany()
                .HasForeignKey(es => es.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PetHealthRecord>(entity =>
        {
            entity.ToTable("pet_health_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.VetId).HasColumnName("vet_id");
            entity.Property(e => e.WeightKg).HasColumnName("weight_kg").HasColumnType("decimal(5,2)");
            entity.Property(e => e.Temperature).HasColumnName("temperature").HasColumnType("decimal(4,1)");
            entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            entity.Property(e => e.Treatment).HasColumnName("treatment");
            entity.Property(e => e.VisitDate).HasColumnName("visit_date").HasColumnType("date");

            entity.HasOne(e => e.Pet)
                .WithMany()
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Veterinarian)
                .WithMany()
                .HasForeignKey(e => e.VetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<VaccinationRecord>(entity =>
        {
            entity.ToTable("vaccination_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.VaccineName).HasColumnName("vaccine_name").HasMaxLength(150);
            entity.Property(e => e.BatchNumber).HasColumnName("batch_number").HasMaxLength(50);
            entity.Property(e => e.AdministeredDate).HasColumnName("administered_date").HasColumnType("date");
            entity.Property(e => e.NextDueDate).HasColumnName("next_due_date").HasColumnType("date");
            entity.Property(e => e.VetId).HasColumnName("vet_id");

            entity.HasOne(e => e.Pet)
                .WithMany()
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Veterinarian)
                .WithMany()
                .HasForeignKey(e => e.VetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MedicalPrescription>(entity =>
        {
            entity.ToTable("medical_prescriptions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.MedicationName).HasColumnName("medication_name").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.Dosage).HasColumnName("dosage").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");

            entity.HasOne(e => e.PetHealthRecord)
                .WithMany(r => r.MedicalPrescriptions)
                .HasForeignKey(e => e.RecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCommerce(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SupplierName).HasColumnName("supplier_name").HasMaxLength(150).IsUnicode(true);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ProductName).HasColumnName("product_name").HasMaxLength(150).IsUnicode(true);
            entity.Property(e => e.Description).HasColumnName("description").IsUnicode(true);
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(20).IsUnicode(true);
            entity.Property(e => e.ImportPrice).HasColumnName("import_price").HasPrecision(12, 2);
            entity.Property(e => e.SellPrice).HasColumnName("sell_price").HasPrecision(12, 2);
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsUnicode(false);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportReceipt>(entity =>
        {
            entity.ToTable("import_receipts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ReceiptDate).HasColumnName("receipt_date");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(14, 2);
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(255).IsUnicode(true);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.ImportReceipts)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportDetail>(entity =>
        {
            entity.ToTable("import_details");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ImportPrice).HasColumnName("import_price").HasPrecision(12, 2);
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(14, 2);

            entity.HasOne(e => e.Receipt)
                .WithMany(r => r.Details)
                .HasForeignKey(e => e.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("carts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(12, 2);

            entity.HasOne(e => e.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.ShippingAddress).HasColumnName("shipping_address").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15).IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(14, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(255).IsUnicode(true);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Price).HasColumnName("price").HasPrecision(12, 2);
            entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasPrecision(14, 2);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(14, 2);
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.TransactionCode).HasColumnName("transaction_code").HasMaxLength(100).IsUnicode(false);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("invoices");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.InvoiceDate).HasColumnName("invoice_date");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasPrecision(14, 2);
            entity.Property(e => e.TaxAmount).HasColumnName("tax_amount").HasPrecision(14, 2);
            entity.Property(e => e.IssuedBy).HasColumnName("issued_by");

            entity.HasOne(e => e.Order)
                .WithOne(o => o.Invoice)
                .HasForeignKey<Invoice>(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IssuedByUser)
                .WithMany()
                .HasForeignKey(e => e.IssuedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.ToTable("pets");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.QrToken).HasColumnName("qr_token").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Species).HasColumnName("species").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.Breed).HasColumnName("breed").HasMaxLength(80).IsUnicode(true);
            entity.Property(e => e.Gender).HasColumnName("gender").HasMaxLength(10).IsUnicode(true);
            entity.Property(e => e.WeightKg).HasColumnName("weight_kg").HasColumnType("decimal(4, 2)");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.QrToken).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PetImage>(entity =>
        {
            entity.ToTable("pet_images");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PetId).HasColumnName("pet_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
            entity.Property(e => e.IsAvatar).HasColumnName("is_avatar");
            entity.Property(e => e.CreatedAt).HasColumnName("uploaded_at");

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.PetImages)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.ToTable("feedbacks");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(2000).IsUnicode(true);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AppointmentId).HasColumnName("appointment_id");
            entity.Property(e => e.Message).HasColumnName("message").HasMaxLength(1000).IsUnicode(true);
            entity.Property(e => e.Type).HasColumnName("type").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatConversation>(entity =>
        {
            entity.ToTable("chat_conversations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.UserRequest).HasColumnName("user_request").IsUnicode(true);
            entity.Property(e => e.AiResponse).HasColumnName("ai_response").IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}