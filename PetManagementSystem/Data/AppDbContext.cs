using Microsoft.EntityFrameworkCore;
using PetManagementSystem.Models;

namespace PetManagementSystem.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();

    public DbSet<Pet> Pets { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ChatConversation> ChatConversations { get; set; }

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
