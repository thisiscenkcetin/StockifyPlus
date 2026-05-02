# StockifyPlus

StockifyPlus, Trakya Üniversitesi bitirme projesi kapsamında geliştirilen ASP.NET Core MVC tabanlı stok ve depo yönetim uygulamasıdır. Uygulama ürün, kategori, stok hareketi, kritik stok takibi, Excel işlemleri, barkod yönetimi, raporlama ve StockAI Agent destekli karar yardımcısı modüllerini tek proje altında toplar.

![StockifyPlus](https://github.com/thisiscenkcetin/StockifyPlus/blob/main/wwwroot/images/Stockify.jpg?raw=true)

## Özellikler

- Ürün ve kategori yönetimi
- Stok giriş, çıkış ve ayarlama işlemleri
- Kritik stok seviyesinde SignalR ile anlık bildirim
- EPPlus ile Excel içe ve dışa aktarım
- Kamera ile barkod okuma ve barkod etiketi yazdırma
- Dashboard üzerinde KPI, hareket ve rapor görünümü
- pdfMake ile PDF iş zekası akıllı rapor çıktısı
- Karanlık tema ve responsive arayüz
- StockAI ile kontrollü stok analizi ve onaylı işlem akışı
- Talep havuzu ve StockAI audit geçmişi

## Teknoloji Yığını

- C# ve ASP.NET Core MVC 8.0
- Entity Framework Core Code-First
- Microsoft SQL Server
- Bootstrap 5.3
- JavaScript ve Fetch API
- SignalR
- IHostedService
- EPPlus
- Chart.js
- pdfMake
- html5-qrcode, JsBarcode ve QRious
- Gemini API ve Groq fallback

## Mimari

Projede katmanlı bir yapı kullanılmıştır. İstekler Controller katmanında karşılanır. İş kuralları Service katmanında yürütülür. Veri erişimi Repository ve Unit of Work yaklaşımıyla yönetilir. Kalıcılık Entity Framework Core üzerinden SQL Server veritabanında sağlanır.

Temel akış:

```text
Razor View -> Controller -> Service -> Repository/UnitOfWork -> EF Core -> SQL Server
```

StockAI modülü de aynı servis sınırlarını kullanır. Kullanıcıdan gelen doğal dil komutu önce sınıflandırılır. Yazma işlemi gerekiyorsa alanlar ayrıştırılır, yetki ve risk kontrolleri yapılır, önizleme üretilir ve açık onaydan sonra mevcut servisler üzerinden işlem uygulanır.

## Kurulum

1. Depoyu klonlayın.

```bash
git clone https://github.com/thisiscenkcetin/StockifyPlus.git
cd StockifyPlus
```

2. `appsettings.json` içindeki SQL Server bağlantı dizesini kendi ortamınıza göre düzenleyin.

3. NuGet paketlerini yükleyin.

```bash
dotnet restore
```

4. Veritabanını oluşturun.

```bash
dotnet ef database update
```

5. Uygulamayı çalıştırın.

```bash
dotnet run
```

## Yapılandırma

Gemini ve Groq API anahtarları ortam değişkeni veya yerel yapılandırma üzerinden verilmelidir. API anahtarları güvenlik nedeniyle depoya eklenmemiştir. Düzgün çalıması için api eklenmesi gerekmektedir. Aksi takdirde ai agent bağlantı sorunu yaşayacaktır. 

Örnek `.env` alanları:

```text
GEMINI_API_KEY=
GROQ_API_KEY=
```

## Akademik Kapsam

Bu proje stok yönetimi sürecini yazılım mühendisliği bakışıyla ele almak için hazırlanmıştır. Veri modeli, iş kuralları, kullanıcı arayüzü, dış servis entegrasyonu, canlı bildirim, raporlama ve kontrollü StockAI işlemleri birlikte değerlendirilmiştir.

dev.cenkcetin@gmail.com

Bu proje MIT lisansı ile paylaşılmıştır.
