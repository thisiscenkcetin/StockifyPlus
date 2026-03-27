# StockifyPlus

StockifyPlus is a graduation project developed at Trakya University, Tunca Vocational School, Web Design and Coding Program. Proje, ASP.NET Core MVC ve Entity Framework (Code-First) yaklaşımı ile web tabanlı stok/depo takibi için tasarlanmıştır.

<p align="center">
  <img src="https://github.com/thisiscenkcetin/StockifyPlus/blob/main/wwwroot/images/Stockify.jpg?raw=true" alt="StockifyPlus Dashboard" width="100%">
</p>


## Core Capabilities / Temel Yetenekler

Planlanan modül yapısında aşağıdaki yetenekler merkeze alınmıştır:

- Product & Category Management: Ürün kartlarının kategori, birim ve barkod bilgileriyle yönetilmesi.
- Warehouse Tracking: Birden fazla depo için stok miktarının ayrı izlenmesi.
- Stock Movements: Giriş, çıkış, iade ve transfer hareketlerinin kayıt altına alınması.
- Threshold Alerts: Kritik seviyenin altına düşen ürünlerin uyarı mekanizması ile izlenmesi.
- Reporting Dashboard: Günlük hareket yoğunluğu, düşük stok listesi ve kategori dağılımı gibi özet metriklerin izlenmesi.

## Technology Stack / Teknoloji Altyapısı

- Backend: C# with ASP.NET Core MVC
- Data Access: Entity Framework Core (Code-First)
- Database: Microsoft SQL Server
- Frontend: Razor Views, Bootstrap, JavaScript
- Visualization: Chart.js
- Layering: Controller-Service/Repository-Data pattern (project scope doğrultusunda sadeleştirilmiş)


### Example: EF Core DbContext

```csharp
using Microsoft.EntityFrameworkCore;

namespace StockifyPlus.Data
{
	public class StockifyDbContext : DbContext
	{
		public StockifyDbContext(DbContextOptions<StockifyDbContext> options)
			: base(options)
		{
		}

		public DbSet<Product> Products => Set<Product>();
		public DbSet<Category> Categories => Set<Category>();
		public DbSet<Warehouse> Warehouses => Set<Warehouse>();
		public DbSet<StockMovement> StockMovements => Set<StockMovement>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Product>(entity =>
			{
				entity.Property(p => p.Name)
					  .HasMaxLength(120)
					  .IsRequired();

				entity.Property(p => p.Sku)
					  .HasMaxLength(64)
					  .IsRequired();

				entity.HasIndex(p => p.Sku)
					  .IsUnique();

				entity.HasOne(p => p.Category)
					  .WithMany(c => c.Products)
					  .HasForeignKey(p => p.CategoryId)
					  .OnDelete(DeleteBehavior.Restrict);
			});
		}
	}
}
```

## Planned Architecture / Planlanan Mimari Akış

Request lifecycle is designed as: `UI (Razor) -> Controller -> Service/Repository -> EF Core -> SQL Server`. Bu akışta doğrulama, iş kuralı ve veri erişim sorumlulukları ayrıştırılarak daha sürdürülebilir bir yapı elde edilmesi hedeflenmiştir.

## Installation / Kurulum

Repository kaynak kodu geliştirme sürecinde büyütülmektedir. Aşağıdaki adımlar, ASP.NET Core MVC + EF Core tabanlı tipik kurulum akışını verir ve proje de gerekli olan LLM api'si gizlenmiştir: 

1. Clone repo:
   ```bash
   git clone <REPOSITORY_URL>
   cd StockifyPlus
   ```
2. Update `appsettings.json` with SQL Server connection string.
3. Run migrations:
   ```bash
   dotnet ef database update
   ```
4. Start application:
   ```bash
   dotnet run
   ```

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.
