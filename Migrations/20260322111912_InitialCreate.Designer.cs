using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StockifyPlus.Data;

#nullable disable

namespace StockifyPlus.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260322111912_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
                        .HasDefaultValue(new DateTime(2026, 3, 22, 14, 19, 11, 950, DateTimeKind.Local).AddTicks(5177));

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

                    b.HasIndex("CategoryId");

                    b.HasIndex("SKU")
                        .IsUnique()
                        .HasDatabaseName("IX_Product_SKU_Unique");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("StockifyPlus.Models.StockMovement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("MovementDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValue(new DateTime(2026, 3, 22, 14, 19, 11, 949, DateTimeKind.Local).AddTicks(5181));

                    b.Property<int>("MovementType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProductId", "MovementDate")
                        .HasDatabaseName("IX_StockMovement_ProductDate");

                    b.ToTable("StockMovements", (string)null);
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

            modelBuilder.Entity("StockifyPlus.Models.Category", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("StockifyPlus.Models.Product", b =>
                {
                    b.Navigation("StockMovements");
                });
#pragma warning restore 612, 618
        }
    }
}
