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

        public DbSet<ProductCustomField> ProductCustomFields { get; set; }

        public DbSet<Wishlist> Wishlists { get; set; }

        public DbSet<Budget> Budgets { get; set; }

        public DbSet<StockAiActionLog> StockAiActionLogs { get; set; }

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
                    .HasDatabaseName("IX_Category_Name");
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
                    .HasDatabaseName("IX_Product_SKU_Unique");

                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_Product_Name");

                entity.HasIndex(e => e.CategoryId)
                    .HasDatabaseName("IX_Product_CategoryId");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_Product_IsActive");

                entity.HasIndex(e => new { e.CategoryId, e.IsActive })
                    .HasDatabaseName("IX_Product_Category_Active");

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
                    .HasDatabaseName("IX_StockMovement_ProductDate");

                entity.HasIndex(e => e.MovementDate)
                    .HasDatabaseName("IX_StockMovement_MovementDate");

                entity.HasIndex(e => e.MovementType)
                    .HasDatabaseName("IX_StockMovement_MovementType");

                entity.HasIndex(e => new { e.MovementType, e.MovementDate })
                    .HasDatabaseName("IX_StockMovement_Type_Date");

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
                    .HasDatabaseName("IX_AppUser_Username_Unique");

                entity.HasIndex(e => e.Email)
                    .HasDatabaseName("IX_AppUser_Email");
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

            modelBuilder.Entity<ProductCustomField>(entity =>
            {
                entity.ToTable("ProductCustomFields");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FieldValue)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.FieldType)
                    .HasMaxLength(20)
                    .HasDefaultValue("Text");

                entity.Property(e => e.Unit)
                    .HasMaxLength(20);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValue(DateTime.Now);

                entity.HasIndex(e => e.ProductId)
                    .HasDatabaseName("IX_ProductCustomField_ProductId");

                entity.HasIndex(e => e.FieldName)
                    .HasDatabaseName("IX_ProductCustomField_FieldName");

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.CustomFields)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ProductCustomField_Product");
            });

            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.ToTable("Wishlists");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.TargetPrice)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.CurrentPrice)
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Priority)
                    .HasDefaultValue(2);

                entity.Property(e => e.Category)
                    .HasMaxLength(100);

                entity.Property(e => e.ProductUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.IsNotified)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsPurchased)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValue(DateTime.Now);

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Wishlist_UserId");

                entity.HasIndex(e => e.Priority)
                    .HasDatabaseName("IX_Wishlist_Priority");

                entity.HasIndex(e => e.IsPurchased)
                    .HasDatabaseName("IX_Wishlist_IsPurchased");

                entity.HasIndex(e => new { e.UserId, e.IsPurchased })
                    .HasDatabaseName("IX_Wishlist_User_Purchased");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Wishlist_User");
            });

            modelBuilder.Entity<Budget>(entity =>
            {
                entity.ToTable("Budgets");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.MonthlyLimit)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.CurrentSpent)
                    .HasColumnType("decimal(10,2)")
                    .HasDefaultValue(0);

                entity.Property(e => e.StartDate)
                    .IsRequired();

                entity.Property(e => e.EndDate)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.IsWarningNotified)
                    .HasDefaultValue(false);

                entity.Property(e => e.IsCriticalNotified)
                    .HasDefaultValue(false);

                entity.Property(e => e.WarningThreshold)
                    .HasDefaultValue(80);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValue(DateTime.Now);

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_Budget_UserId");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_Budget_IsActive");

                entity.HasIndex(e => new { e.StartDate, e.EndDate })
                    .HasDatabaseName("IX_Budget_DateRange");

                entity.HasIndex(e => new { e.UserId, e.IsActive })
                    .HasDatabaseName("IX_Budget_User_Active");

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Budget_User");

                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Budget_Category");
            });

            modelBuilder.Entity<StockAiActionLog>(entity =>
            {
                entity.ToTable("StockAiActionLogs");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                    .HasMaxLength(100);

                entity.Property(e => e.ActionType)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(40);

                entity.Property(e => e.EntityType)
                    .HasMaxLength(80);

                entity.Property(e => e.EntityKey)
                    .HasMaxLength(80);

                entity.Property(e => e.UserPrompt)
                    .IsRequired()
                    .HasMaxLength(1200);

                entity.Property(e => e.AgentResponse)
                    .IsRequired()
                    .HasMaxLength(1600);

                entity.Property(e => e.Metadata)
                    .HasMaxLength(2000);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValue(DateTime.Now);

                entity.HasIndex(e => e.CreatedAt)
                    .HasDatabaseName("IX_StockAiActionLog_CreatedAt");

                entity.HasIndex(e => e.ActionType)
                    .HasDatabaseName("IX_StockAiActionLog_ActionType");

                entity.HasIndex(e => new { e.UserId, e.CreatedAt })
                    .HasDatabaseName("IX_StockAiActionLog_User_CreatedAt");
            });
        }
    }
}
