using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StockifyPlus.Data;

#nullable disable

namespace StockifyPlus.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("StockifyPlus.Models.AppUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2026, 5, 1, 17, 59, 9, 931, DateTimeKind.Local).AddTicks(3781));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<DateTime?>("LastLoginDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastModifiedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("Role")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(2);

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .HasDatabaseName("IX_AppUser_Email");

                    b.HasIndex("Username")
                        .IsUnique()
                        .HasDatabaseName("IX_AppUser_Username_Unique");

                    b.ToTable("AppUsers", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.Budget", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2026, 5, 1, 17, 59, 9, 933, DateTimeKind.Local).AddTicks(9796));

                    b.Property<decimal>("CurrentSpent")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(10,2)")
                        .HasDefaultValue(0m);

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<bool>("IsCriticalNotified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsWarningNotified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LastUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("MonthlyLimit")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("WarningThreshold")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(80);

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("IsActive")
                        .HasDatabaseName("IX_Budget_IsActive");

                    b.HasIndex("UserId")
                        .HasDatabaseName("IX_Budget_UserId");

                    b.HasIndex("StartDate", "EndDate")
                        .HasDatabaseName("IX_Budget_DateRange");

                    b.HasIndex("UserId", "IsActive")
                        .HasDatabaseName("IX_Budget_User_Active");

                    b.ToTable("Budgets", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .HasDatabaseName("IX_Category_Name");

                    b.ToTable("Categories", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.NotificationSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("AlertEmail")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("LastUpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2026, 5, 1, 17, 59, 9, 931, DateTimeKind.Local).AddTicks(7904));

                    b.Property<string>("LastUpdatedBy")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasDefaultValue("System");

                    b.Property<bool>("PushEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.HasKey("Id");

                    b.ToTable("NotificationSettings", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("CriticalStockLevel")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(10);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<decimal>("Price")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(10,2)")
                        .HasDefaultValue(0m);

                    b.Property<string>("SKU")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("StockQuantity")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.HasKey("Id");

                    b.HasIndex("CategoryId")
                        .HasDatabaseName("IX_Product_CategoryId");

                    b.HasIndex("IsActive")
                        .HasDatabaseName("IX_Product_IsActive");

                    b.HasIndex("Name")
                        .HasDatabaseName("IX_Product_Name");

                    b.HasIndex("SKU")
                        .IsUnique()
                        .HasDatabaseName("IX_Product_SKU_Unique");

                    b.HasIndex("CategoryId", "IsActive")
                        .HasDatabaseName("IX_Product_Category_Active");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.ProductCustomField", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2026, 5, 1, 17, 59, 9, 932, DateTimeKind.Local).AddTicks(617));

                    b.Property<string>("FieldName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("FieldType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasDefaultValue("Text");

                    b.Property<string>("FieldValue")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<string>("Unit")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("FieldName")
                        .HasDatabaseName("IX_ProductCustomField_FieldName");

                    b.HasIndex("ProductId")
                        .HasDatabaseName("IX_ProductCustomField_ProductId");

                    b.ToTable("ProductCustomFields", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.StockAiActionLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ActionType")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<string>("AgentResponse")
                        .IsRequired()
                        .HasMaxLength(1600)
                        .HasColumnType("nvarchar(1600)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2026, 5, 1, 17, 59, 9, 934, DateTimeKind.Local).AddTicks(8362));

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<string>("EntityKey")
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.Property<string>("EntityType")
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.Property<string>("Metadata")
                        .HasMaxLength(2000)
                        .HasColumnType("nvarchar(2000)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("UserPrompt")
                        .IsRequired()
                        .HasMaxLength(1200)
                        .HasColumnType("nvarchar(1200)");

                    b.Property<string>("Username")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("ActionType")
                        .HasDatabaseName("IX_StockAiActionLog_ActionType");

                    b.HasIndex("CreatedAt")
                        .HasDatabaseName("IX_StockAiActionLog_CreatedAt");

                    b.HasIndex("UserId", "CreatedAt")
                        .HasDatabaseName("IX_StockAiActionLog_User_CreatedAt");

                    b.ToTable("StockAiActionLogs", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.StockMovement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("MovementDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2026, 5, 1, 17, 59, 9, 930, DateTimeKind.Local).AddTicks(4596));

                    b.Property<int>("MovementType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MovementDate")
                        .HasDatabaseName("IX_StockMovement_MovementDate");

                    b.HasIndex("MovementType")
                        .HasDatabaseName("IX_StockMovement_MovementType");

                    b.HasIndex("MovementType", "MovementDate")
                        .HasDatabaseName("IX_StockMovement_Type_Date");

                    b.HasIndex("ProductId", "MovementDate")
                        .HasDatabaseName("IX_StockMovement_ProductDate");

                    b.ToTable("StockMovements", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.Wishlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Category")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2026, 5, 1, 17, 59, 9, 932, DateTimeKind.Local).AddTicks(8753));

                    b.Property<decimal?>("CurrentPrice")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("IsNotified")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<bool>("IsPurchased")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LastUpdatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Priority")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(2);

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("ProductUrl")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime?>("PurchaseDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal?>("TargetPrice")
                        .HasColumnType("decimal(10,2)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IsPurchased")
                        .HasDatabaseName("IX_Wishlist_IsPurchased");

                    b.HasIndex("Priority")
                        .HasDatabaseName("IX_Wishlist_Priority");

                    b.HasIndex("UserId")
                        .HasDatabaseName("IX_Wishlist_UserId");

                    b.HasIndex("UserId", "IsPurchased")
                        .HasDatabaseName("IX_Wishlist_User_Purchased");

                    b.ToTable("Wishlists", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.Budget", b =>
                {
                    b.HasOne("StockifyPlus.Models.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("FK_Budget_Category");

                    b.HasOne("StockifyPlus.Models.AppUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Budget_User");

                    b.Navigation("Category");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StockifyPlus.Models.Product", b =>
                {
                    b.HasOne("StockifyPlus.Models.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_Product_Category");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("StockifyPlus.Models.ProductCustomField", b =>
                {
                    b.HasOne("StockifyPlus.Models.Product", "Product")
                        .WithMany("CustomFields")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_ProductCustomField_Product");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("StockifyPlus.Models.StockMovement", b =>
                {
                    b.HasOne("StockifyPlus.Models.Product", "Product")
                        .WithMany("StockMovements")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_StockMovement_Product");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("StockifyPlus.Models.Wishlist", b =>
                {
                    b.HasOne("StockifyPlus.Models.AppUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Wishlist_User");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StockifyPlus.Models.Category", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("StockifyPlus.Models.Product", b =>
                {
                    b.Navigation("CustomFields");

                    b.Navigation("StockMovements");
                });
#pragma warning restore 612, 618
        }
    }
}
