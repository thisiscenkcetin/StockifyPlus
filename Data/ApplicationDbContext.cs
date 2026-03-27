using Microsoft.EntityFrameworkCore;
using StockifyPlus.Models;
using StockifyPlus.Models.Enums;

namespace StockifyPlus.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<StockMovement> StockMovements { get; set; }

        public DbSet<AppUser> AppUsers { get; set; }

        public DbSet<NotificationSetting> NotificationSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.Name)
                    .HasName("IX_Category_Name");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.SKU)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.StockQuantity)
                    .HasDefaultValue(0);

                entity.Property(e => e.CriticalStockLevel)
                    .HasDefaultValue(10);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(e => e.SKU)
                    .IsUnique()
                    .HasName("IX_Product_SKU_Unique");

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Product_Category");
            });

            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.ToTable("StockMovements");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.MovementType)
                    .IsRequired()
                    .HasDefaultValue(MovementType.Giriş);

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.MovementDate)
                    .IsRequired()
                    .HasDefaultValue(DateTime.Now);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.HasIndex(e => new { e.ProductId, e.MovementDate })
                    .HasName("IX_StockMovement_ProductDate");

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.StockMovements)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_StockMovement_Product");
            });

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.FullName)
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .HasMaxLength(100);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasDefaultValue(UserRole.DepoPersoneli);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValue(DateTime.Now);

                entity.HasIndex(e => e.Username)
                    .IsUnique()
                    .HasName("IX_AppUser_Username_Unique");

                entity.HasIndex(e => e.Email)
                    .HasName("IX_AppUser_Email");
            });

            modelBuilder.Entity<NotificationSetting>(entity =>
            {
                entity.ToTable("NotificationSettings");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.PushEnabled)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.AlertEmail)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastUpdatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValue("System");

                entity.Property(e => e.LastUpdatedAt)
                    .IsRequired()
                    .HasDefaultValue(DateTime.Now);
            });
        }
    }
}
