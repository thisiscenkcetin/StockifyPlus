# 1. GİRİŞ VE AMAÇ

Stok ve depo yönetimi işletmelerin günlük operasyonlarında yalnızca ürün sayımı yapılan bir süreç olarak değerlendirilmemektedir. Tedarik planlaması, satış sürekliliği, maliyet kontrolü ve müşteri memnuniyeti gibi kritik karar başlıklarının önemli bir kısmını stok verisinin doğruluğu belirler. Küçük ve orta ölçekli işletmelerde yapılan saha gözlemlerinde stok hareketleri yaygın biçimde Excel dosyaları, defter kayıtları veya birbiriyle konuşmayan farklı uygulamalar üzerinden takip edilmektedir. Bu parçalı yapı aynı ürünün farklı adlarla açılmasına, giriş-çıkış kayıtlarının gecikmeli işlenmesine, kritik stok eşiklerinin geç fark edilmesine ve dönem sonu sayımlarında yüksek sapmaların oluşmasına yol açmıştır. İşletmeler tarafında bu durum satın alma kararlarında gereksiz maliyet artışı ve depo alanının verimsiz kullanımı olarak karşılık bulmuştur.

StockifyPlus projesi belirtilen operasyonel kırılganlıkları azaltmak ve stok verisini tek merkezde yönetilebilir hale getirmek amacıyla tasarlanmıştır. Arka planda C# dili ile ASP.NET Core MVC mimarisi tercih edilmiş, veri katmanında Entity Framework Core Code-First yaklaşımı kullanılmış ve Microsoft SQL Server üzerinde ilişkisel bir veri modeli kurulmuştur. Tasarım yalnızca klasik kayıt ekranları üretmekle sınırlandırılmamıştır. Canlı bildirim, otonom uyarı mekanizması, dış servis entegrasyonu, raporlama ve kullanıcı deneyimini destekleyen arayüz kararları bir bütün olarak sistem oluşturmuştur. Bu yapı sayesinde uygulama temel CRUD sınırını aşarak karar destek niteliği taşıyan bir platforma dönüştürülmüştür.

[EKRAN GÖRÜNTÜSÜ: StockifyPlus giriş ekranı, rol bazlı kimlik doğrulama formu ve hatalı giriş uyarı mesajı]
Şekil 1.1: Sisteme güvenli erişim için tasarlanan kullanıcı giriş ekranı.

Projenin çıkış motivasyonu eğitim sürecinde edinilen web geliştirme bilgisinin gerçek bir problem üzerinde uygulanması gerekliliğinden doğmaktadır. Depo operasyonlarında "günün sonunda kayıt girme" alışkanlığı yaygın olup güncel stok bilgisinin karar anında kullanılamadığı gözlemlenmektedir. Bu eksiklik sonucunda sahada bulunmayan ürün satışa açılmakta, düşük stok seviyeleri geç fark edilmekte ve geçmiş hareket geriye dönük izlenmemektedir. StockifyPlus ile hedeflenen yaklaşım ürün-kategori-hareket ilişkilerinin tutarlı bir veri modelinde toplanmasını, işlemlerin zaman damgası ile kaydedilmesini ve yöneticinin tek panelden kritik göstergeleri izleyebilmesini esas almıştır.
Çalışmanın amaçları çok katmanlı biçimde tanımlanmıştır:

1. Ürün kartı yönetimini standartlaştırarak veri tutarlılığını artırmak
2. Stok giriş-çıkış ve ayarlama hareketlerinin izlenebilirliğini güçlendirmek
3. Kritik stok seviyesine düşen ürünlerin gerçek zamanlı bildirim ve gece e-posta uyarısıyla raporlanabilmesini sağlamak
4. Raporlama ve görselleştirme araçlarıyla yönetsel görünürlüğü artırmak

Bu kapsamda sistemde üretilen verinin Chart.js grafikleri ile izlenmesi, html2pdf.js ile rapor çıktısı alınması ve iş süreçlerinin arşivlenmesi hedeflenmiştir.

Projede yer alan modern bileşenler özellikle tez çalışmasının uygulama boyutunu güçlendirmiştir. Ön yüzde macOS/tablet hissi veren glassmorphism yaklaşımı benimsenmiş, kullanıcıya yoğun menü geçişi yerine tek ekranda toplanmış bir kontrol paneli deneyimi sunulmuştur. html5-qrcode entegrasyonu ile barkod okutma işlemi tarayıcı kamerası üzerinden web uygulamasına bağlanmıştır. EPPlus ile toplu Excel içe aktarma fonksiyonu geliştirilerek manuel veri giriş yükü azaltılmıştır. StockAI modülünde Gemini/Groq tabanlı doğal dil işleme yaklaşımı entegre edilerek kullanıcıların stokla ilgili sorularına metin tabanlı yanıt üretilebilmiştir. Bu tercihlerin her biri, stok yazılımının yalnızca veri saklayan bir araç olmaktan çıkarılıp iş akışını hızlandıran bir yapıya taşınmasına katkı sağlamıştır.

[EKRAN GÖRÜNTÜSÜ: All-in-One dashboard üzerinde kritik stok kartları, son hareket çizelgesi ve canlı bildirim alanı]
Şekil 1.2: Tek ekran mimarisiyle kurgulanan yönetim paneli görünümü.

Geliştirme sürecinde karşılaşılan teknik zorluklar, çalışmanın insan emeğiyle olgunlaştırılan doğasını açık biçimde ortaya koymuştur. SignalR ile canlı bildirim akışı devreye alınırken bazı istemcilerde bağlantı kurulamaması problemi yaşanmış, inceleme sonrasında middleware sıralamasının bu davranışı etkilediği görülmüştür. Program.cs içinde yönlendirme, oturum ve uç nokta eşleme adımlarının sırası yeniden düzenlenerek hub bağlantılarının kararlı hale geldiği doğrulanmıştır. Benzer biçimde barkod okuma ekranında ilk denemelerde kamera izin reddi nedeniyle tarama başlatılamamış, kullanıcıya anlaşılır izin yönlendirmeleri eklenmiş ve hata mesajları sadeleştirilmiştir. Excel içe aktarma tarafında da ondalık ayırıcı farklarından kaynaklı fiyat dönüşüm hatalarıyla karşılaşılmış, kültür bağımsız parse stratejisi ile bu sorun giderilmiştir.

Akademik kapsam açısından proje sınırları kontrollü biçimde belirlenmiştir. Çok şubeli çevrim dışı senkronizasyon, ERP ile çift yönlü entegrasyon, mobil native uygulama ve ileri düzey talep tahminleme gibi başlıklar mevcut dönemin dışında bırakılmıştır. Bu sınırlama, çekirdek fonksiyonların güvenilir biçimde tamamlanabilmesi için gerekli görülmüştür. Böylece veri modeli, iş kuralları ve kullanıcı işlemleri daha derin test edilmiş, tezde sunulan bulguların teknik karşılığı daha net ortaya konulmuştur. Devam çalışmalarıyla bu sınırların ilerleyen sürümlerde aşılabileceği öngörülmüştür.

Bu tez çalışmasının özgün katkısı, stok yönetimi alanında çoğunlukla ayrı ayrı ele alınan bileşenlerin tek bir uygulama çatısı altında birlikte çalıştırılmasıdır. Canlı bildirim, gecelik otonom uyarı, donanım tabanlı barkod okuma, toplu Excel veri yönetimi, LLM destekli karar yardımı ve istemci tarafı hızlı raporlama aynı mimari omurgada bir araya getirilmiştir. Bu birleşimde temel ilke, kullanıcı hızını artırırken veri bütünlüğünü zayıflatmamak olmuştur. Eğitim projesi ölçeğinde bu kadar farklı modülün tek veritabanı modeli ve ortak iş kurallarıyla yönetilmesi, çalışmayı yalnızca bir arayüz uygulaması olmaktan çıkarıp yöntemsel bir yazılım mühendisliği örneğine dönüştürmüştür.

1. bölümde ortaya konan problem tanımı, amaçlar ve kapsam doğrultusunda uygulamanın bir karar destek sistemi olarak konumlandığı görülmektedir. Takip eden bölümde, kullanılan teknolojilerin seçilme gerekçeleri ve katmanlı mimari yaklaşımı yöntemsel açıdan ayrıntılı biçimde ele alınmıştır.

# 2. MATERYAL VE YÖNTEM

## 2.1 Teknolojilerin Seçilme Nedenleri

StockifyPlus geliştirme hattında teknoloji seçimi yapılırken yalnızca “çalışır bir prototip” üretimi hedeflenmemiş, sürdürülebilirlik, bakım kolaylığı ve genişletilebilirlik gibi kurumsal ölçekte önem taşıyan ölçütler dikkate alınmıştır. Bu doğrultuda arka uçta ASP.NET Core MVC, veri erişiminde Entity Framework Core, veri depolamada SQL Server, gerçek zamanlı iletişimde SignalR, arka plan otomasyonunda Hosted Service, dış zekâ servisinde Gemini/Groq ve raporlama tarafında html2pdf.js ile Chart.js kullanılmıştır. Ön yüz geliştirme adımlarında tek ekran odaklı modern bir panel yaklaşımı planlanmış, tablet kullanımına uygun bir etkileşim katmanı hedeflenmiştir.

ASP.NET Core MVC seçiminin temel gerekçesi sorumluluk ayrımını net biçimde destekleyen bir yapı sunmasıdır. Model, View ve Controller katmanlarının ayrıştırılması sayesinde kullanıcı arayüzündeki değişimlerin veri erişim kodunu doğrudan etkilemesi önlenmiş, test ve bakım faaliyetleri daha yönetilebilir hale getirilmiştir. Ayrıca .NET 8 ekosisteminin performans, güvenlik ve uzun dönem destek avantajları proje açısından güçlü bir zemin oluşturmuştur. Özellikle kimlik doğrulama, oturum yönetimi ve yönlendirme altyapısının hazır ve olgun olması geliştirme hızını artırmıştır.

Entity Framework Core Code-First tercihinde veri modelinin doğrudan C# sınıfları üzerinden yönetilebilmesi belirleyici olmuştur. Migration mekanizması sayesinde model değişiklikleri sürüm kontrolüne izlenebilir biçimde yansıtılmış, şema evrimi dokümantasyonla birlikte ilerletilmiştir. LINQ tabanlı sorgu yazımı, geliştirici üretkenliğini artırmış ve ham SQL bağımlılığını azaltmıştır. Buna ek olarak Fluent API ile ilişki kurallarının açık tanımlanabilmesi, özellikle ürün-kategori-hareket tablolarında veri bütünlüğünü koruma açısından kritik rol oynamıştır.

[EKRAN GÖRÜNTÜSÜ: Entity Framework migration geçmişi ve veritabanı şema güncelleme adımları]
Şekil 2.1: Code-First yaklaşımında migration tabanlı şema yönetimi.

SignalR teknolojisi depo operasyonlarında anlık farkındalık ihtiyacı nedeniyle projeye dahil edilmiştir. Kritik stok seviyesine inen ürünlerin yalnızca sayfa yenileme sonrası görünmesi operasyonel gecikme oluşturduğundan, sunucunun istemcilere doğrudan bildirim gönderebildiği bir yapı benimsenmiştir. Entegrasyon sırasında bağlantı kopmaları ve istemci yeniden bağlanma davranışları test edilmiş, kararlı bir bildirim akışı için hub uç noktaları ile istemci script başlatma sırası yeniden düzenlenmiştir. Böylece kullanıcı panelinde gerçekten anlık bir uyarı deneyimi elde edilmiştir.

Arka plan servisleri tarafında StockAlertBackgroundService kullanılarak her gece belirlenen saatte kritik stok raporunun e-posta ile iletilmesi hedeflenmiştir. Bu tasarım sayesinde uygulama açık olmasa dahi stok kontrol döngüsü devam ettirilmiştir. SMTP altyapısı üzerinden çalışan bu mekanizma, manuel takip yükünü azaltan otonom bir katman olarak kurgulanmıştır. Gün sonu operasyonlarında insan hatası kaynaklı atlamaların azaltılması açısından da bu yaklaşımın etkili olduğu görülmüştür.

StockAI bileşeninde Gemini/Groq entegrasyonu, kullanıcıların doğal dilde soru sorarak stok bilgisine erişebilmesi amacıyla seçilmiştir. “Hangi ürünler kritik seviyede?”, “Bu hafta en çok çıkış yapan kalem hangisi?” gibi sorulara metin tabanlı hızlı yanıt üretimi planlanmıştır. Bu noktada modelden dönen cevapların doğrudan iş kuralına dönüştürülmemesi, yalnızca danışmanlık düzeyinde kullanılması benimsenmiştir. Böylece yapay zeka çıktılarından kaynaklanabilecek yanlış yönlendirme riski sınırlanmıştır. API erişiminde kesinti yaşanması ihtimaline karşı alternatif model/fallback yaklaşımının tanımlanması da süreklilik açısından önemli bir karar olmuştur.

EPPlus ile Excel toplu içe aktarma, sahada sık karşılaşılan “ürünleri tek tek girme” sorununu azaltmak için kullanılmıştır. İşletmelerden gelen başlangıç envanteri çoğunlukla Excel dosyalarında tutulduğundan, bu dosyaların doğrulama adımından geçirilerek sisteme alınması büyük zaman kazanımı sağlamıştır. İçe aktarma aşamasında sütun eşleme, zorunlu alan kontrolü, tekrar SKU denetimi ve format standardizasyonu adımları işletilmiştir. Bu akışta veri doğrulama sertleştirilerek bozuk kayıtların sisteme karışması önlenmiştir.

html2pdf.js ve Chart.js ikilisi, görsel yönetim raporlarının çıktı alınabilmesi için birlikte kullanılmıştır. Dashboard grafiklerinin PDF olarak arşivlenmesi, dönemsel toplantılarda hızlı rapor sunumu açısından pratik bir avantaj oluşturmuştur. Grafiklerin okunabilirliğini korumak için renk kontrastları ve etiket yerleşimi düzenlenmiş, çıktı kalitesinin düşmemesi adına uygun ölçekte render ayarları yapılmıştır.

## 2.2 Mimari Yaklaşım (Katmanlı Yapı)

Proje mimarisi tasarlanırken monolitik fakat katmanlı bir kurgu benimsenmiştir. İstekler Controller katmanında karşılanmış, iş kuralları Service katmanında yürütülmüş, veri erişimi Repository ve Unit of Work desenleri üzerinden yönetilmiş, kalıcılık ise ApplicationDbContext aracılığıyla SQL Server üzerinde sağlanmıştır. Bu yapı sayesinde her katmanın sorumluluğu netleşmiş, kodun okunabilirliği ve bakım maliyeti açısından daha dengeli bir temel kurulmuştur.

Aşağıdaki kod parçası, uygulamada servis kayıtlarının, SignalR hub altyapısının ve arka plan servisinin nasıl devreye alındığını gösteren kritik bir örnek niteliğindedir.

[Program.cs içinde DI, SignalR ve Hosted Service kaydı]
```csharp
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Bağlantı dizesi yapılandırılmadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddHostedService<StockAlertBackgroundService>();

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
app.MapHub<NotificationHub>("/notificationHub");
```

Bu kurgu içinde mimari kararların önemli bir bölümü veri bütünlüğü ve işlem güvenliği etrafında şekillendirilmiştir. Ürün silme, stok düşme, kritik eşik kontrolü ve kullanıcı yetkisi gerektiren işlemlerde kuralların Controller içine dağılmasına izin verilmemiş; kuralların Service katmanında merkezi şekilde çalıştırılması sağlanmıştır. Böylece aynı kuralın farklı ekranlarda çelişkili biçimde uygulanması riski azaltılmıştır. Repository katmanında ortak CRUD işlemleri genelleştirilmiş, alan bilgisi gerektiren sorgular için özel metodlar bırakılarak performans ve okunabilirlik dengesi korunmuştur.

[EKRAN GÖRÜNTÜSÜ: Katmanlı mimari diyagramı (Controller -> Service -> Repository -> DbContext -> SQL Server)]
Şekil 2.2: StockifyPlus katmanlı mimari veri akışı.

Bu bölümde teknoloji seçimi, mimari yaklaşım, veritabanı etkileşimi, entegrasyon katmanları ve yöntemsel kararlar bütüncül biçimde açıklanmıştır. Aktarılan çerçeve, bir sonraki bölümde sunulan uygulama bulgularının teknik arka planını oluşturmuştur.

## 2.3 StockAI (LLM) Entegrasyonunun Teorik Altyapısı

Stok yönetimi yazılımlarında klasik arama kutularının yalnızca anahtar kelime eşleşmesi ile sınırlı çalışması, özellikle teknik olmayan kullanıcıların sistemden hızlı bilgi almasını zorlaştırmaktadır. Bu nedenle StockifyPlus içinde doğal dil sorgularını işleyebilen bir yardımcı katman tasarlanmış ve StockAI bileşeni devreye alınmıştır. Teorik olarak bu katman, uygulama verisini doğrudan değiştiren bir otomasyon motoru olarak değil, kullanıcıyı doğru ekrana yönlendiren ve mevcut stok durumu hakkında yorumlayıcı metin üreten bir karar destek bileşeni olarak konumlandırılmıştır.

LLM entegrasyonunda temel akış üç adımda modellenmiştir: kullanıcı girdisinin alınması, uygun istem metniyle harici modele iletilmesi ve dönen cevabın sistem politikalarına göre filtrelenerek gösterilmesi. Bu akışta istem mühendisliği yaklaşımı önemli bir yer tutmaktadır; modelin depo terminolojisine uygun, kısa ve operasyon odaklı yanıt üretmesi için sistem düzeyi yönerge tanımlanmıştır. API katmanında hem birincil model hem de alternatif model desteği yapılandırılmış, bir servis kesintisi yaşandığında kullanıcıya boş cevap dönülmesi yerine kontrollü bir fallback akışı kurulmuştur.

LLM katmanının veritabanı ile ilişkisi doğrudan SQL sorgusu çalıştırma şeklinde kurgulanmamıştır. Bunun yerine kritik veri noktalarının uygulama servislerinden toplanıp modele bağlamsal bilgi olarak verilmesi yaklaşımı benimsenmiştir. Bu sayede modelin rastgele sorgu üretmesi engellenmiş, hangi veri alanına erişeceği uygulama katmanı tarafından sınırlandırılmıştır. Verinin denetimli aktarılması hem performans kontrolü sağlamış hem de yetki dışı veri sızdırma olasılığını düşürmüştür. Üretilen metin çıktılarının işlem onayı yerine rehber niteliğinde sunulması da insan denetimini süreçte tutan önemli bir güvenlik adımı olmuştur.

[EKRAN GÖRÜNTÜSÜ: StockAI sohbet penceresi, kullanıcı sorusu ve sistem tarafından üretilen stok yorumu]
Şekil 2.3: LLM tabanlı yardımcı asistanın kullanıcı etkileşim ekranı.

[AI ApiService içinde model seçimi ve fallback mantığı]
```csharp
public async Task<string> GenerateResponseAsync(string userMessage, CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(userMessage))
    {
        throw new ArgumentException("Kullanıcı mesajı boş olamaz.", nameof(userMessage));
    }

    if (string.IsNullOrWhiteSpace(_options.ApiKey))
    {
        var groqOnlyResponse = await TryGenerateWithGroqAsync(userMessage, cancellationToken);
        if (!string.IsNullOrWhiteSpace(groqOnlyResponse))
        {
            return groqOnlyResponse;
        }

        throw new InvalidOperationException("Gemini-Grog API yapılandırması eksik ve Groq fallback kullanılamadı.");
    }

    var modelCandidates = BuildModelCandidates(_options.Model);
    return await TryGenerateAcrossModelsAsync(modelCandidates, userMessage, cancellationToken);
}
```

Teknik terim yoğun ekranlarda kullanıcının doğru raporu bulma süresi kısaltılmış, karar alma hızının artması hedeflenmiştir. Model yanıtlarının doğruluk seviyesi zaman ve istem kalitesine göre değişebildiği için, kritik karar noktalarında nihai kontrolün uygulama verisi ve kullanıcı onayı ile tamamlanması ilkesi korunmuştur.

## 2.4 html5-qrcode ile Barkod ve Donanım Entegrasyonu

Depo süreçlerinde barkod okutma yeteneği, veri giriş hızını ve doğruluğunu doğrudan etkileyen bir işlevdir. Manuel ürün kodu yazımında oluşan karakter hataları, yanlış ürün seçimi ve zaman kaybı gibi sorunları azaltmak amacıyla html5-qrcode tabanlı kamera entegrasyonu kullanılmıştır. Teorik olarak bu yapı, tarayıcı API'leri üzerinden cihaz kamerasına erişim sağlayan istemci katmanı ile uygulamanın sunucu tarafındaki ürün doğrulama katmanının birleşimi olarak modellenmiştir. Donanım erişimi istemci tarafında gerçekleşirken barkodun hangi ürüne ait olduğuna sunucu tarafında karar verilmiş, güvenli bir kontrol hattı kurulmuştur.

Donanım entegrasyonunda izin yönetimi kritik başlıklardan biri olarak ele alınmıştır. Kullanıcının kamera iznini reddetmesi, güvenli bağlantı (HTTPS) bulunmaması veya tarayıcı uyumluluğu eksikliği durumlarında okuma süreci başlatılamamaktadır. Bu senaryolarda teknik hata metinleri yerine kullanıcıya anlaşılır yönlendirme gösterilmesi benimsenmiştir. Böylece sistem davranışı daha öngörülebilir hale getirilmiş, destek ihtiyacı da azaltılmıştır.

Barkod verisi alındıktan sonra mimari akış Controller katmanına yönlendirilmiştir. Sunucu tarafında barkod alanı üzerinden ürün sorgusu yapılmış, eşleşme bulunursa ürün kartı ve stok bilgisi birlikte döndürülmüş, eşleşme bulunamazsa kontrollü uyarı üretilmiştir. Bu yaklaşımda barkod değerinin yalnızca metin olarak saklanması yerine SKU/ürün kodu alanıyla tutarlı biçimde indekslenmesi planlanmış, sorgu maliyeti düşürülmüştür. Ayrıca tekrar eden yanlış barkod denemeleri log katmanına kaydedilerek kalite izlemesi yapılabilmiştir.

[EKRAN GÖRÜNTÜSÜ: Barkod tarama modal penceresi, kamera görüntüsü ve okuma sonucu paneli]
Şekil 2.4: html5-qrcode ile tarayıcı kamerası üzerinden barkod okuma akışı.

Bu altyapıda donanım ve yazılım katmanlarının birlikte ele alınması gerektiği net biçimde görülmüştür. Kamera görüntüsü başarılı biçimde alınsa dahi sunucu doğrulaması olmadan güvenilir işlem yapılamayacağı anlaşılmıştır. Barkod okuma bu yüzden yalnızca “hızlı giriş” özelliği olarak bırakılmamış, veritabanı doğrulamasıyla desteklenen bir kimlikleme mekanizmasına dönüştürülmüştür.

## 2.5 EPPlus ile Excel Toplu Aktarım ve Veri Bütünlüğü

Stok sistemine geçiş yapan işletmelerde ilk kurulum aşamasında binlerce satırlık ürün verisinin Excel dosyalarından taşınması yaygın bir ihtiyaçtır. Bu amaçla EPPlus kütüphanesi kullanılarak toplu içe aktarma modülü geliştirilmiştir. Teorik olarak modül; dosya alma, şablon doğrulama, satır bazlı kuralları çalıştırma, geçerli kayıtları toplu işleme ve hata raporu üretme adımlarından oluşan bir işlem hattı olarak tasarlanmıştır. Böylece tek adımda veri yüklenirken kalite kontrolünün kaybolması engellenmiştir.

Excel aktarımının veritabanı ile uyumlu çalışması için sütun eşleme stratejisi sabitlenmiştir. Ürün adı, SKU, kategori, birim fiyat, kritik stok seviyesi gibi alanlar hem Excel şablonunda hem de veri modelinde karşılıklı eşleştirilmiş; zorunlu alanlarda boş değer kabul edilmemiştir. Verinin metin, sayı ve tarih biçimleri ön doğrulamadan geçirilmiş, kültürel ondalık ayırıcı farklılıklarından doğabilecek dönüşüm hataları için normalize edici parse yöntemi uygulanmıştır. Bu süreçte hatalı satırların tüm aktarımı durdurması yerine satır bazlı hata listesiyle kullanıcıya geri bildirim verilmesi benimsenmiştir.

Toplu aktarım senaryolarında en büyük risklerden biri aynı SKU değerinin birden fazla satırda yer almasıdır. Bu risk iki katmanda ele alınmıştır: uygulama katmanında ön kontrol ve veritabanında benzersizlik indeksi. Uygulama katmanında dosya içi tekrarlar tespit edilerek kullanıcıya raporlanmış, veri tabanı katmanında da unique indeks ile son güvenlik bariyeri korunmuştur. Böylece veri kalitesi yalnızca kullanıcı dikkatine bırakılmamış, mimari seviyede güvence altına alınmıştır.

[EKRAN GÖRÜNTÜSÜ: Excel içe aktarma ekranı, şablon indirme butonu ve satır bazlı hata raporu]
Şekil 2.5: EPPlus tabanlı toplu ürün aktarımının doğrulama ekranı.

Mimari bakışta EPPlus modülü, yalnızca veri taşıma aracı değil, aynı zamanda veri kalitesi filtresi olarak konumlandırılmıştır. Hatalı satır raporları sayesinde tekrar eden giriş hataları azaltılmış, başlangıç envanterinin sisteme alınma süresi kısaltılmıştır.

## 2.6 SignalR ve Background Service Birlikte Çalışma Mimarisi

Stok yönetiminde olayların yalnızca kayıt altına alınması yeterli görülmemektedir; aynı zamanda doğru zamanda doğru kişiye bildirim ulaştırılması beklenmektedir. StockifyPlus içinde bu gereksinim iki katmanlı bir bildirim mimarisi ile ele alınmıştır. Birinci katmanda SignalR aracılığıyla anlık (real-time) istemci bildirimi üretilmiş, ikinci katmanda Background Service ile zamanlanmış e-posta raporu gönderimi gerçekleştirilmiştir.

SignalR hattında NotificationHub merkez rol üstlenmiştir. Uygulama içinde kritik stok eşiği tetiklendiğinde veya belirli bir hareket gerçekleştiğinde bağlı istemcilere push mesaj gönderimi yapılmıştır. Burada istemcinin sürekli sunucuyu sorgulaması yerine sunucunun doğrudan istemciye veri iletmesi benimsenmiştir. Böylece gereksiz HTTP trafiği azaltılmış, kullanıcı panelindeki gecikme süresi düşürülmüştür. WebSocket desteklenmeyen senaryolarda SignalR'ın alternatif taşıma yöntemlerine geçebilmesi, farklı ağ koşullarında iletişim sürekliliğini desteklemiştir.

Background Service katmanı gerçek zamanlı akıştan bağımsız çalışacak biçimde planlanmıştır. Her gece 00:00 zamanında kritik stok listesi hazırlanmış, alıcı adresi bildirim ayarlarından okunmuş ve SMTP servisi ile gönderim yapılmıştır. Süreçte veritabanı sorgusu AsNoTracking yaklaşımıyla optimize edilmiş, yalnızca rapor için gerekli alanların çekilmesine dikkat edilmiştir. Gönderim başarısız olduğunda döngünün tamamen kırılmaması için hata yakalama ve loglama mekanizmaları uygulanmıştır. Bu sayede geçici ağ sorunlarında sistemin ertesi gün yeniden çalışabilirliği korunmuştur.

[EKRAN GÖRÜNTÜSÜ: Canlı bildirim toast alanı ve gece gönderilen kritik stok e-posta örneği]
Şekil 2.6: Anlık ve zamanlanmış bildirim katmanlarının birlikte kullanım senaryosu.

[StockAlertBackgroundService içinde zamanlanmış kritik stok raporu akışı]
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        var now = DateTime.Now;
        var nextRun = now.Date.AddDays(1);
        var delay = nextRun - now;

        await Task.Delay(delay, stoppingToken);
        await ProcessCriticalStockReportAsync(stoppingToken);
    }
}

private async Task ProcessCriticalStockReportAsync(CancellationToken cancellationToken)
{
    using var scope = _scopeFactory.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var criticalProducts = await dbContext.Products
        .AsNoTracking()
        .Where(p => p.IsActive && p.StockQuantity <= p.CriticalStockLevel)
        .OrderBy(p => p.StockQuantity)
        .ToListAsync(cancellationToken);

}
```

Bu mimaride gerçek zamanlı bildirim ile gecelik raporlama katmanlarının birbirini tamamladığı görülmüştür. Kullanıcı paneli açıkken anlık uyarı alınabilmiş, panel kapalıyken de e-posta üzerinden kritik bilgi iletimi sürdürülmüştür. Bu yaklaşım, depo operasyonlarında “bilginin kaçırılması” riskini azaltan önemli bir tasarım kararı olarak öne çıkmıştır.

## 2.7 Veritabanı Konuşma Modeli, İşlem Sınırları ve Tutarlılık

Enterprise özelliklerin tamamında veritabanı etkileşimi ortak bir prensiple kurgulanmıştır: iş kuralı uygulama katmanında çalıştırılmalı, kalıcılık tek bir sorumluluk hattı üzerinden yönetilmelidir. Bu nedenle Controller katmanından doğrudan DbContext çağrısı yapılmamış, Service ve Repository sınırları korunmuştur.

LLM, barkod ve Excel modüllerinin her biri farklı giriş kanalı üretse de hepsinin ürün tablosu ve stok hareketi tablolarında buluştuğu görülmüştür. Bu nedenle alan doğrulama kuralları merkezi hale getirilmiş, modül bazlı farklı davranışların veri modelini bozması engellenmiştir. Örneğin barkodla bulunan ürün de Excel ile eklenen ürün de aynı SKU benzersizlik kuralına tabi tutulmuştur. Benzer biçimde kritik stok kontrolü tüm modüllerde aynı eşik alanı üzerinden yürütülmüş, iş kuralı tekrarları azaltılmıştır.

Performans açısından okuma ağırlıklı sorgularda AsNoTracking, liste ekranlarında sayfalama, sık kullanılan alanlarda indeksleme ve yalnızca gerekli sütunların seçilmesi gibi yöntemler uygulanmıştır. Loglama katmanında teknik hataların kayıt altına alınması, üretim ortamında kök neden analizi yapılmasını kolaylaştırmıştır.

[EKRAN GÖRÜNTÜSÜ: Ürün tablosu üzerinde SKU unique index ve kritik stok sorgu planı]
Şekil 2.7: Veri bütünlüğü ve performans için uygulanan temel veritabanı stratejileri.

## 2.8 Bölüm Sonu Değerlendirmesi

Materyal ve yöntem bölümünün ikinci yarısında ele alınan kurumsal bileşenler, StockifyPlus mimarisinin yalnızca ekran üretimine dayalı bir yapı olmadığını göstermektedir. LLM destekli StockAI katmanında kullanıcıya doğal dilde rehberlik sunulmuş, html5-qrcode ile donanım erişimi web uygulamasına taşınmış, EPPlus ile toplu veri geçişi hızlandırılmış, SignalR ve Background Service birlikte kullanılarak bildirim sürekliliği güçlendirilmiştir.

Geliştirme sürecinde karşılaşılan bağlantı kopmaları, izin yönetimi sorunları ve format dönüşüm hataları yöntemsel iyileştirmelerle aşılmıştır. Böylece sistem davranışı daha öngörülebilir hale getirilmiş, bakım maliyetini düşüren bir mimari omurga oluşturulmuştur. Takip eden bölümde uygulama çıktıları ekran akışları ve kod kanıtlarıyla ayrıntılı biçimde sunulacaktır.

# 3. BULGULAR VE UYGULAMA

Bu bölümde StockifyPlus projesinin geliştirme süreci sonunda elde edilen teknik bulgular, kullanıcı arayüzü çıktıları, katmanlar arası veri akışları ve entegrasyon sonuçları ayrıntılı biçimde sunulmuştur. Değerlendirme yaklaşımında yalnızca ekran görüntüsü sunumu yapılmamış; kod tarafındaki çalışma mantığı, performans davranışı, hata senaryoları ve sürdürülebilirlik kazanımları da birlikte ele alınmıştır. Uygulamanın kurumsal nitelik taşıyan bileşenleri tek tek incelenmiş ve her bileşenin veri modeli ile nasıl konuştuğu açıklanmıştır. Böylece sistemin yalnızca bir arayüz demonstrasyonu olmadığı, gerçek işlem yükü altında davranışı düşünülmüş bir mimari yaklaşım olduğu gösterilmiştir.

Uygulama gözlemlerinde öne çıkan temel bulgu, modül izolasyonu ile merkezi iş kurallarının birlikte kullanılmasının bakım faaliyetlerini belirgin biçimde kolaylaştırmasıdır. Arayüzde yapılan değişikliklerin servis katmanına düşük etkiyle taşınabildiği, servis katmanında yapılan kural güncellemelerinin de birden fazla ekranda tutarlı sonuç ürettiği doğrulanmıştır. Canlı bildirim, barkod tarama, AI asistanı, toplu veri içe aktarma ve PDF raporlama gibi özellikler eşzamanlı çalışabilen bir akışta birleştirilmiştir; bu akışta güvenlik, doğrulama ve kullanıcı deneyimi dengesi korunmuştur. Depo operasyonlarında işlem hızını artıran özelliklerin denetlenebilirlikten ödün vermeden uygulanması, projenin temel kazanımı olarak değerlendirilmiştir.

[EKRAN GÖRÜNTÜSÜ: Dashboard, StockAI, canlı bildirim ve hızlı işlem kartlarını aynı anda gösteren bütünleşik ana ekran]
Şekil 3.1: StockifyPlus uygulamasında kurumsal modüllerin aynı arayüzde birlikte çalıştığı all-in-one görünüm.

## 3.1 macOS/Tablet Konseptli All-in-One Dashboard Arayüzü

Dashboard ekranı tasarlanırken birincil hedef operatörün gün içinde en sık kullandığı bilgileri tek bakışta görebilmesidir. Bu nedenle geleneksel çok menülü panel kurgusu yerine, KPI kartları, haftalık trend grafiği, hızlı işlem butonları ve son hareket akışının tek ekrana yerleştirildiği bir düzen uygulanmıştır. Kullanıcı deneyimi açısından bu yaklaşım, ekranlar arasında dolaşma maliyetini düşürmüş ve kritik kararların daha hızlı alınmasına imkân vermiştir. Görsel tasarım tarafında cam efekti ve yumuşak gölge geçişleri ile modern bir görünüm hedeflenmiş, bu görünüm tablet kullanımında okunabilirliği bozmayacak biçimde sade tutulmuştur.

Arayüzün veri yükleme stratejisinde tam sayfa yenileme yerine parçalı güncelleme yaklaşımı benimsenmiştir. Dashboard açıldığında temel KPI değerleri sunucu tarafında hazırlanarak ViewData üzerinden taşınmış, hareket listesi ise periyodik AJAX çağrısı ile güncellenmiştir. Bu hibrit model sayesinde ilk açılışta hızlı bir görünürlük sağlanmış, sonrasında yalnızca değişen bileşenin güncellenmesi ile gereksiz trafik azaltılmıştır. Ayrıca canlı aktivite bileşeninin belli aralıklarla otomatik yenilenmesi, operatör ekranı açıkken işlem akışının kopmamasını desteklemiştir.

[EKRAN GÖRÜNTÜSÜ: KPI kartları (Kritik Stok, Toplam Ürün, Bugünkü Hareket) ve haftalık çizgi grafiğin yan yana görünümü]
Şekil 3.2: Yönetim kararlarına temel oluşturan KPI paneli ve trend grafiği yerleşimi.

[HomeController içinde dashboard KPI verilerinin hazırlanması]
```csharp
public async Task<IActionResult> Index()
{
    var username = HttpContext.Session.GetString("Username");
    ViewBag.Username = username;

    try
    {
        var activeProducts = await _productService.GetAllActiveProductsAsync();
        var lowStockProducts = await _productService.GetLowStockProductsAsync();
        var activeCategories = await _categoryService.GetAllActiveCategoriesAsync();
        var allMovements = await _stockMovementService.GetAllMovementsAsync();

        var todayMovements = allMovements?.Where(m => m.MovementDate.Date == DateTime.Now.Date).Count() ?? 0;

        ViewData["TotalProducts"] = activeProducts?.Count() ?? 0;
        ViewData["LowStockProducts"] = lowStockProducts?.Count() ?? 0;
        ViewData["TotalCategories"] = activeCategories?.Count() ?? 0;
        ViewData["TodayMovements"] = todayMovements;
    }
    catch
    {
        ViewData["TotalProducts"] = 0;
        ViewData["LowStockProducts"] = 0;
        ViewData["TotalCategories"] = 0;
        ViewData["TodayMovements"] = 0;
    }

    return View();
}
```

UI kararları uygulanırken karşılaşılan pratik sorunlardan biri, aynı ekranda hem grafik hem de liste bileşeninin bulunması nedeniyle mobil/tablet kırılımlarında taşma oluşmasıdır. İlk sürümlerde “Son Hareketler” alanında metinler üst üste binmiş, KPI kartları dar ekranlarda dikey sıraya geçtiğinde buton hizaları bozulmuştur. Bu durum grid düzeninin yeniden düzenlenmesi ve minimum yükseklik değerlerinin güncellenmesi ile çözülmüştür. Ayrıca rapor içeriğinin PDF’e aktarımı yapılırken grafik alanının boş yakalandığı bir hata görülmüş, istemci tarafında bir frame bekletilerek render stabilitesi artırılmıştır.

[Dashboard tarafında html2pdf rapor hazırlığı için bekleme ve seçenek ayarı]
```javascript
async function downloadPdfReport() {
    if (!reportContent || !window.html2pdf) {
        alert('PDF raporu olusturulamadi. Lutfen sayfayi yenileyip tekrar deneyin.');
        return;
    }

    await new Promise((resolve) => requestAnimationFrame(resolve));

    const options = {
        margin: [10, 10, 10, 10],
        filename: buildReportFileName(),
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true, backgroundColor: '#ffffff' },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' }
    };

    await html2pdf().set(options).from(reportContent).save();
}
```

Dashboard uygulamasında elde edilen önemli bir sonuç, kullanıcıların menüden çok hızlı aksiyon butonlarını tercih etmesidir. Özellikle “Yeni Ürün”, “Stok Düş”, “Barkod Oku” ve “Stok Bildirimi” kısa yollarının kullanım oranı yüksek bulunmuştur. Bu bulgu doğrultusunda kritik işlemler için tek tık erişim yaklaşımı korunmuş, ikincil işlevler üst menü altında bırakılmıştır. Böylelikle günlük operasyon hızına doğrudan katkı sağlayan bir arayüz davranışı elde edilmiştir.

## 3.2 StockAI: LLM Tabanlı Karar Destek Asistanı

StockAI modülü sistemde tutulan stok verisinin doğal dil ile sorgulanabilmesi amacıyla uygulamaya dahil edilmiştir. Modülün mimari rolü kullanıcı sorusunu alıp uygun bir istem metni ile dil modeline iletmek, dönen yanıtı güvenli biçimde kullanıcıya sunmak ve mümkün olduğu durumlarda doğrudan sistem verisiyle desteklenmiş cevaplar üretmektir. Burada kritik tasarım kararı, modelin veritabanına serbest erişim vermek yerine uygulama servislerinden üretilen kontrollü özet üzerinden çalıştırılmasıdır. Bu yaklaşım sayesinde hem veri güvenliği korunmuş hem de modelin tutarsız yanıt üretme riski azaltılmıştır.

Uygulamada yanıt mekanizması iki seviyede yapılandırılmıştır. İlk seviyede belirli anahtar sorgular (örneğin depodaki ürün listesi veya kritik stok soruları) doğrudan C# tarafında tespit edilerek model çağrısı yapılmadan hızlı cevap üretilmiştir. İkinci seviyede daha serbest metinli sorular için envanter snapshot'ı prompt içine gömülmüş ve dış servis çağrısı yapılmıştır. Bu katmanlama ile API maliyeti azaltılmış, yanıt süresi iyileştirilmiş ve kota tüketimi daha dengeli bir profile çekilmiştir.

[EKRAN GÖRÜNTÜSÜ: StockAI modal penceresinde kullanıcı mesajı, bot mesajı ve hata durumunda gösterilen uyarı]
Şekil 3.3: LLM destekli sohbet ekranında normal yanıt ve hata yönetimi davranışı.

[KOD BLOĞU: StockAIController içinde hızlı sorgu yakalama ve LLM çağrısı]
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken cancellationToken)
{
    if (request == null || string.IsNullOrWhiteSpace(request.Message))
    {
        return BadRequest(new { response = "Lütfen bir mesaj girin." });
    }

    var activeProducts = (await _productService.GetAllActiveProductsAsync()).ToList();
    var normalizedMessage = request.Message.Trim().ToLowerInvariant();

    if (IsInventoryListQuery(normalizedMessage))
    {
        return Json(new { response = BuildInventoryListResponse(activeProducts) });
    }

    if (IsLowStockQuery(normalizedMessage))
    {
        return Json(new { response = BuildLowStockResponse(activeProducts) });
    }

    var enrichedPrompt = BuildInventoryGroundedPrompt(request.Message, activeProducts);
    var responseText = await _geminiApiService.GenerateResponseAsync(enrichedPrompt, cancellationToken);
    return Json(new { response = responseText });
}
```

Prompt mühendisliği tarafında elde edilen bulgu, modele kuralları açık ve kısa biçimde verilmediğinde “veri dışında tahmin” yapma eğiliminin arttığı yönündedir. Bu nedenle prompt içine canlı stok verisi işaretleyicilerle gömülmüş, yanıtın yalnızca bu veri setiyle üretilmesi kuralı net biçimde belirtilmiştir. Ayrıca bulunmayan bilgi durumunda modelden standart bir hata cümlesi döndürmesi istenmiştir. Bu yaklaşım, özellikle sunum sırasında modelin tutarsız örnekler üretmesini engelleyen önemli bir koruma katmanı olarak işlev görmüştür.

[KOD BLOĞU: Envanter verisi ile zenginleştirilmiş prompt üretimi]
```csharp
private static string BuildInventoryGroundedPrompt(string userMessage, List<Models.Product> products)
{
    var snapshot = new StringBuilder();
    snapshot.AppendLine("[CANLI_STOK_VERISI]");
    snapshot.AppendLine($"ToplamAktifUrun={products.Count}");

    foreach (var product in products.OrderBy(p => p.Name).Take(80))
    {
        snapshot.AppendLine($"Urun={product.Name};SKU={product.SKU};Kategori={product.Category?.Name ?? "-"};Stok={product.StockQuantity};Kritik={product.CriticalStockLevel};Fiyat={product.Price}");
    }

    snapshot.AppendLine("[/CANLI_STOK_VERISI]");
    snapshot.AppendLine("KURAL: Sadece yukarıdaki canlı stok verisine dayanarak cevap ver.");
    snapshot.AppendLine("KURAL: Veri dışında ürün/kategori uydurma.");
    snapshot.AppendLine("Kullanıcı Sorusu:");
    snapshot.AppendLine(userMessage.Trim());
    return snapshot.ToString();
}
```

LLM entegrasyonunda yaşanan gerçekçi zorluklardan biri dış servis tarafında kota ve yetkilendirme hatalarının kullanıcı deneyimini kesmesidir. Bu yüzden denenen yapı içinde HTTP 429, 401/403 ve model bulunamadı hataları ayrı ayrı yakalanmış, kullanıcıya teknik terim içermeyen anlaşılır geri dönüşler verilmiştir. Bir diğer zorluk, bazı cihazlarda modal içinde uzun yanıtların taşmasıdır; sohbet alanı sabit yükseklik ve otomatik kaydırma ile düzenlenerek sorun azaltılmıştır. Elde edilen sonuçta StockAI modülünün “işlem yapan bot” değil “karar destek asistanı” olarak konumlandırılması daha güvenilir bulunmuştur.

[EKRAN GÖRÜNTÜSÜ: API hata durumlarında (kota, yetki, model) kullanıcıya gösterilen farklı mesaj örnekleri]
Şekil 3.4: StockAI servis hatalarında kullanıcıyı yönlendiren kontrollü geri bildirim ekranları.

## 3.3 Optik Barkod ve QR Kod Donanım Entegrasyonu (html5-qrcode)

Stok giriş, çıkış ve ayarlama ekranlarında barkod okuyucu entegrasyonu ile manuel ürün seçiminin hızlandırılması hedeflenmiştir. Bu kapsamda html5-qrcode kütüphanesi istemciye eklenmiş, modal pencere içinde kamera akışı başlatılarak barkod okuma işlemi gerçekleştirilmiştir. Okuma sonrasında çözümlenen barkod değeri sunucuya POST edilmekte, sunucu tarafında ürün karşılığı doğrulanmakta ve eşleşme varsa ilgili ürün otomatik seçilmektedir. Bu akışın en önemli kazanımı, operatör kaynaklı SKU yazım hatalarının düşmesidir.

Donanım entegrasyonunda güvenli bağlantı gereksinimi ve cihaz izin politikaları kritik rol oynamıştır. Geliştirme testlerinde bazı tarayıcılarda HTTPS olmayan oturumlarda kamera açılamadığı görülmüş, dağıtım ayarlarında HTTPS zorlaması aktif tutulmuştur. Ayrıca kullanıcı izni reddettiğinde sistemin sessiz kalması yerine toast uyarısı ile yönlendirme verilmiştir. Kamera açılıp kapanma döngüsünde bellek sızıntısı riskine karşı modal kapanırken scanner.clear çağrısı zorunlu hale getirilmiş, ardışık taramalarda yaşanan kilitlenme problemi bu yolla giderilmiştir.

[EKRAN GÖRÜNTÜSÜ: Stok çıkışı ekranında barkod modalının açılması, kamera görüntüsü ve ürün eşleştirme sonucu]
Şekil 3.5: Tarayıcı kamerası ile barkod okutma sonrası ürünün otomatik seçildiği işlem akışı.

[KOD BLOĞU: StockOut sayfasında html5-qrcode başlatma ve sunucu doğrulama]
```javascript
const barcodeEndpoint = '@Url.Action("GetProductByBarcode", "Product")';
const productSelect = document.getElementById('productId');
let scanner = null;
let scanning = false;

async function onScanSuccess(decodedText) {
    if (scanning) return;
    scanning = true;

    const response = await fetch(barcodeEndpoint, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': tokenInput.value
        },
        body: JSON.stringify({ barcode: decodedText })
    });

    const result = await response.json();
    if (response.ok && result.success) {
        productSelect.value = String(result.id); // Doğru ürün seçimi otomatik yapılır
    }

    await stopScanner();
}

modalElement.addEventListener('shown.bs.modal', () => {
    scanner = new Html5QrcodeScanner('reader', {
        fps: 10,
        qrbox: { width: 250, height: 130 },
        rememberLastUsedCamera: true
    }, false);
    scanner.render(onScanSuccess, () => { });
});
```

[KOD BLOĞU: ProductController içinde barkod karşılığı ürün bulma]
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> GetProductByBarcode([FromBody] BarcodeLookupRequest request)
{
    if (request == null || string.IsNullOrWhiteSpace(request.Barcode))
    {
        return BadRequest(new { success = false, message = "Barkod boş olamaz." });
    }

    var barcode = request.Barcode.Trim();
    var products = await _productService.GetAllActiveProductsAsync();

    var product = products.FirstOrDefault(p =>
        !string.IsNullOrWhiteSpace(p.SKU) &&
        string.Equals(p.SKU.Trim(), barcode, StringComparison.OrdinalIgnoreCase));

    if (product == null)
    {
        return NotFound(new { success = false, message = "Bu barkoda ait aktif ürün bulunamadı." });
    }

    return Json(new { success = true, id = product.Id, name = product.Name, currentStock = product.StockQuantity, sku = product.SKU });
}
```

Bu modülde görülen bir diğer önemli bulgu, barkod okutma işleminin yalnızca “stok çıkışı” için değil stok girişi ve ayarlama ekranlarında da aynı tasarımla kullanılabilmesidir. Böylece kullanıcı eğitim maliyeti düşmüş, ekranlar arasında davranış tutarlılığı sağlanmıştır. Modallar arası bileşen tekrarının yüksek olması nedeniyle ortak JavaScript yardımcı fonksiyonları çıkarılması planlanmış; mevcut sürümde okunabilirliği korumak adına ekran bazlı script kullanımı sürdürülmüştür.

[EKRAN GÖRÜNTÜSÜ: Stok giriş, çıkış ve ayarlama ekranlarında aynı barkod modal deneyiminin karşılaştırmalı görünümü]
Şekil 3.6: Farklı hareket ekranlarında ortak barkod iş akışı tutarlılığı.

## 3.4 EPPlus ile Toplu Veri Yönetimi (Excel Import/Export)

Excel import/export modülü, saha operasyonlarında yüksek hacimli veri hareketini hızlandıran ana bileşenlerden biri olarak geliştirilmiştir. Export tarafında aktif ürün listesi kategori bilgisi ve fiyat alanı ile birlikte dışarı alınmış, import tarafında aynı şablondan geri yükleme desteklenmiştir. Bu yaklaşım sayesinde başlangıç envanterinin sisteme alınması, dönemsel sayım güncellemeleri ve dış raporlama için veri paylaşımı kolaylaştırılmıştır. Ürün sayısı arttığında manuel kayıt maliyetinin hızlı yükseldiği görülmüş, toplu işlem yeteneğinin operasyonel süreye doğrudan katkı verdiği doğrulanmıştır.

Import hattında veri kalitesi için çok aşamalı kontrol uygulanmıştır. Dosya uzantısı doğrulaması, sayfa varlığı kontrolü, satır bazlı zorunlu alan denetimi, fiyat dönüştürme, miktar parse kontrolü ve kategori eşleme sırasıyla yürütülmüştür. Kategori bulunamadığında otomatik kategori açılması sağlanmış, pasif durumdaki kategoriyle karşılaşıldığında kategori yeniden aktif edilmiştir. Bu kararla kullanıcı müdahalesi gerektiren adımlar azaltılmış, import akışı daha otonom hale getirilmiştir.

[EKRAN GÖRÜNTÜSÜ: Excel import ekranında başarılı/başarısız satır sayacı ve hata listesinin gösterimi]
Şekil 3.7: Toplu veri içe aktarma modülünde satır bazlı doğrulama çıktısı.

[KOD BLOĞU: ProductController içinde Excel export akışı]
```csharp
[HttpGet]
public async Task<IActionResult> ExportToExcel()
{
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

    var products = await _context.Products
        .Include(p => p.Category)
        .Where(p => p.IsActive)
        .OrderBy(p => p.Name)
        .ToListAsync();

    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Urunler");

    worksheet.Cells[1, 1].Value = "Stok Kodu (SKU)";
    worksheet.Cells[1, 2].Value = "Ürün Adı";
    worksheet.Cells[1, 3].Value = "Kategori";
    worksheet.Cells[1, 4].Value = "Fiyat";
    worksheet.Cells[1, 5].Value = "Miktar";

    var row = 2;
    foreach (var product in products)
    {
        worksheet.Cells[row, 1].Value = product.SKU;
        worksheet.Cells[row, 2].Value = product.Name;
        worksheet.Cells[row, 3].Value = product.Category?.Name ?? string.Empty;
        worksheet.Cells[row, 4].Value = product.Price;
        worksheet.Cells[row, 5].Value = product.StockQuantity;
        row++;
    }

    worksheet.Column(4).Style.Numberformat.Format = "#,##0.00";
    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

    return File(package.GetAsByteArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"StockifyPlus_Urunler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
}
```

[KOD BLOĞU: ProductController içinde Excel import satır işleme ve upsert mantığı]
```csharp
for (var row = 2; row <= rowCount; row++)
{
    var sku = worksheet.Cells[row, 1].Text?.Trim();
    var name = worksheet.Cells[row, 2].Text?.Trim();
    var categoryName = worksheet.Cells[row, 3].Text?.Trim();
    var priceText = worksheet.Cells[row, 4].Text?.Trim();
    var quantityText = worksheet.Cells[row, 5].Text?.Trim();

    if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(categoryName))
    {
        skippedCount++;
        errors.Add($"Satır {row}: SKU, Ürün Adı ve Kategori zorunludur.");
        continue;
    }

    if (!TryParseDecimal(priceText, out var price) ||
        !int.TryParse(quantityText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity))
    {
        skippedCount++;
        continue;
    }

    var normalizedSku = sku.ToUpperInvariant();
    if (productMap.TryGetValue(normalizedSku, out var existingProduct))
    {
        existingProduct.Name = name;
        existingProduct.Price = price;
        existingProduct.StockQuantity = Math.Max(0, existingProduct.StockQuantity + quantity);
        updatedCount++;
    }
    else
    {
        await _context.Products.AddAsync(new Product
        {
            Name = name,
            SKU = normalizedSku,
            Price = price,
            StockQuantity = Math.Max(0, quantity),
            CriticalStockLevel = 10,
            Category = category,
            IsActive = true
        });
        createdCount++;
    }
}
```

Excel modülünde karşılaşılan belirgin zorluk, farklı kaynaklardan gelen fiyat formatlarının tutarsız olmasıdır. Özellikle “53.400,00” ile “53,400.00” biçimleri aynı dosyada görüldüğünde parse hataları oluşmuş, ilk sürümlerde satırların bir kısmı atlanmıştır. Sorunun çözümü için fiyat alanına normalize edici dönüşüm adımları eklenmiş ve birden fazla kültür ayarında parse denemesi yapılmıştır. Ayrıca hatalı satırın tüm işlemi kesmesi yerine satır bazlı hata listesi döndürülmesiyle kullanıcıya düzeltilebilir geri bildirim sunulmuştur.

[EKRAN GÖRÜNTÜSÜ: Fiyat formatı hatası, eksik kategori ve tekrar SKU senaryolarında üretilen hata mesajları]
Şekil 3.8: Excel içe aktarma sürecinde veri kalitesi kontrol mekanizmasının örnek çıktıları.

## 3.5 Frontend Tabanlı Dinamik PDF Raporlama (html2pdf.js)

Raporlama bileşeninde kullanıcıların ekran görüntüsü almadan standart çıktı üretmesi hedeflenmiş ve html2pdf.js tabanlı istemci tarafı PDF üretimi uygulanmıştır. Bu tercihle sunucu tarafında ek rapor servisleri geliştirilmeden anlık dashboard görünümünün A4 formatına dönüştürülmesi sağlanmıştır. Haftalık toplantılarda güncel KPI ve grafiklerin paylaşımında bu özellik pratik bir avantaj sunmuştur. Dosya adı zaman damgası ile oluşturularak arşivleme kolaylaştırılmış, aynı gün birden fazla rapor alındığında dosya çakışması engellenmiştir.

PDF üretim akışında grafikler canvas üzerinde çizildiği için render zamanlaması kritik bulunmuştur. İlk denemelerde grafik alanının boş çıktığı gözlenmiş, capture adımından önce requestAnimationFrame ile bir frame bekletilerek sorun azaltılmıştır. Yüksek çözünürlük için html2canvas ölçek değeri artırılmış; dosya boyutunun gereksiz büyümemesi adına kalite parametresi dengelenmiştir. A4 yön seçimi ekranın en-boy oranına göre dinamik belirlenmiş ve rapor düzeninin taşmadan çıkması sağlanmıştır.

[EKRAN GÖRÜNTÜSÜ: PDF Raporu İndir butonu, indirilen raporun ilk sayfası ve grafiklerin çıktı kalitesi]
Şekil 3.9: Dashboard görünümünün istemci tarafında PDF’e dönüştürüldüğü raporlama süreci.

[KOD BLOĞU: Home/Index.cshtml içinde html2pdf seçenekleri ve buton durumu yönetimi]
```javascript
const options = {
    margin: [10, 10, 10, 10],
    filename: buildReportFileName(),
    image: { type: 'jpeg', quality: 0.98 },
    html2canvas: {
        scale: 2,
        useCORS: true,
        backgroundColor: '#ffffff'
    },
    jsPDF: {
        unit: 'mm',
        format: 'a4',
        orientation
    }
};

downloadReportBtn.disabled = true;
const originalLabel = downloadReportBtn.innerHTML;
downloadReportBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Hazirlaniyor...';

try {
    await html2pdf().set(options).from(reportContent).save();
} finally {
    downloadReportBtn.disabled = false;
    downloadReportBtn.innerHTML = originalLabel;
}
```

PDF raporlama modülünde elde edilen temel çıktı, operasyonel rapor çıktılarının dışa alınmasının kullanıcı tarafında teknik bilgi gerektirmeden yapılabilmesidir. Buna ek olarak çok uzun içeriklerde sayfa kırılım kontrolünün manuel düzenleme gerektirdiği görülmüş, pagebreak kuralları ile bu durum kısmen dengelenmiştir. İleri sürüm için sunucu tarafı şablonlu rapor üretimi değerlendirilmiş; mevcut sürümde hızlı ve düşük maliyetli olduğu için frontend tabanlı yaklaşım korunmuştur.

## 3.6 SignalR Gerçek Zamanlı Bildirimler ve Background Service ile Otonom Sistemler

StockifyPlus bildirim mimarisinde iki farklı zaman ölçeği birlikte çalıştırılmıştır: anlık bildirim ve zamanlanmış denetim. Anlık bildirimde SignalR kullanılarak kritik stok olayları istemci ekranına toast mesaj olarak iletilmiştir. Zamanlanmış denetimde ise StockAlertBackgroundService kullanılarak her gece kritik stok raporu hazırlanmış ve e-posta ile gönderilmiştir. Bu ikili model sayesinde hem operatör ekran başındayken hem de ekran dışında kaldığında bilgi akışı devam ettirilmiştir.

SignalR tarafında HubConnectionBuilder ile istemci bağlantısı kurulmuş; otomatik yeniden bağlanma aktif edilmiştir. Bu sayede kısa süreli ağ kopmalarında kullanıcı müdahalesi olmadan bağlantı tekrar kurulabilmiştir. Bildirim ayarı kapalı kullanıcılar için istemci tarafında kontrol eklenmiş, yetkisiz veya gereksiz bildirim üretimi önlenmiştir. Toast bileşeninde ürün adı, kalan stok ve kritik seviye aynı kartta gösterilerek karar için gerekli bilgi tek mesajda sunulmuştur.

[EKRAN GÖRÜNTÜSÜ: Sağ üst köşede görünen kritik stok toast bildirimi ve aynı anda güncellenen hareket ekranı]
Şekil 3.10: SignalR tabanlı gerçek zamanlı uyarıların kullanıcı arayüzünde gösterimi.

[KOD BLOĞU: _Layout.cshtml içinde SignalR istemci bağlantısı ve bildirim dinleyicisi]
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/notification')
    .withAutomaticReconnect()
    .build();

connection.on('ReceiveStockAlert', function (payload) {
    if (!@isPushEnabled.ToString().ToLowerInvariant()) {
        return;
    }
    showStockAlertToast(payload);
});

async function startConnection() {
    try {
        await connection.start();
    } catch {
        setTimeout(startConnection, 5000);
    }
}

startConnection();
```

Background Service katmanında günlük zamanlama için bir sonraki 00:00 hesabı yapılmış, bu zamana kadar beklenip rapor üretim fonksiyonu tetiklenmiştir. Alıcı e-posta adresi öncelikle kullanıcı bildirim ayarlarından okunmuş, bulunamadığında SMTP yapılandırmasındaki yönetici adresine düşülmüştür. Bu fallback düzeni ile tek bir ayar eksikliği nedeniyle raporlamanın tamamen durması engellenmiştir. Görev döngüsünde hata yakalama uygulanarak servis çökmesi riski düşürülmüş, log kayıtları ile izlenebilirlik artırılmıştır.

[KOD BLOĞU: StockAlertBackgroundService içinde gecelik rapor tetikleme mantığı]
```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    _logger.LogInformation("StockAlertBackgroundService baslatildi.");

    while (!stoppingToken.IsCancellationRequested)
    {
        try
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddDays(1); // Bir sonraki gece 00:00
            var delay = nextRun - now;

            await Task.Delay(delay, stoppingToken);
            await ProcessCriticalStockReportAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kritik stok worker dongusunde beklenmeyen hata olustu.");
        }
    }
}
```

[EKRAN GÖRÜNTÜSÜ: Gece 00:00 sonrası gönderilen kritik stok e-posta raporunun örnek içeriği]
Şekil 3.11: Background service tarafından üretilen otonom kritik stok e-posta bildirimi.

Gerçek kullanım testlerinde bir zorluk, geliştirme ortamında çok sayıda tarayıcı sekmesi açıkken aynı olay için çoklu toast görünmesidir. Bu durum SignalR istemcilerinin bağımsız bağlantı açmasından kaynaklanmış, üretim kullanım senaryosunda kullanıcı başına tek aktif ekran yaklaşımı ile yönetilmiştir. Bir diğer zorluk, saat dilimi farkı olan ortamlarda günlük tetikleme zamanının şaşmasıdır; bu risk için sunucu saatinin merkezi olarak yönetilmesi ve gerekli görülürse UTC tabanlı zamanlamaya geçilmesi önerilmiştir.

## 3.7 Uygulama Bulgularının Toplu Değerlendirilmesi

Bulgular bütüncül olarak değerlendirildiğinde StockifyPlus projesinde geliştirilen modüllerin birbirini tamamlayan bir yapı oluşturduğu görülmüştür. Dashboard katmanında görünürlük, StockAI katmanında yorumlama desteği, barkod modülünde hızlı ve doğru ürün seçimi, EPPlus modülünde yüksek hacimli veri hareketi, html2pdf modülünde rapor üretimi ve SignalR/Background Service katmanında bildirim sürekliliği birlikte çalışmıştır. Bu birliktelik uygulamayı yalnızca veri kaydı yapan bir sistem olmaktan çıkarıp operasyon yönetimini hızlandıran bir platforma dönüştürmüştür.

Teknik açıdan dikkat çeken bir diğer sonuç, farklı teknoloji ailesine ait bileşenlerin (C#, Razor, JavaScript, harici CDN kütüphaneleri, dış API servisleri) tek bir yaşam döngüsünde yönetilebilmesidir. Bu süreçte middleware sıralaması, istemci render zamanlaması, parse normalizasyonu, bağlantı yeniden kurma, satır bazlı hata yönetimi gibi gerçek geliştirme sorunlarıyla karşılaşılmış ve çözüm adımları kalıcı hale getirilmiştir. Böylece tez kapsamında yalnızca başarılı senaryolar değil, zorlu senaryolarda sistemin nasıl toparlandığı da gösterilebilir düzeye taşınmıştır.

[EKRAN GÖRÜNTÜSÜ: Tüm enterprise özelliklerin kısa özetini içeren uygulama akış diyagramı]
Şekil 3.12: Dashboard’dan bildirim ve raporlamaya uzanan uçtan uca StockifyPlus operasyon akışı.

Bu bölümde sunulan çıktılar, sonuç ve öneri değerlendirmesi için teknik kanıt niteliği taşımıştır. Uygulama davranışı ekran, kod ve iş akışı üçlüsü ile birlikte incelendiğinde, geliştirilen mimarinin eğitim projesi ölçeğinde güçlü bir temel sağladığı doğrulanmıştır.


# 5. ÖNERİLERİN UYGULANMASI VE İLERİ DÜZEY MODÜLLER

Projenin ilk halinde ürün, kategori ve stok hareketi gibi temel işlemler çalışır durumdaydı. Fakat gerçek bir depo uygulamasında yalnızca kayıt eklemek veya listelemek yeterli olmuyor. Kullanıcı geçmişte ne olduğunu görmek, kritik stokları zamanında fark etmek, ürünleri toplu şekilde içeri almak, barkodla hızlı işlem yapmak ve gerektiğinde rapor çıkarabilmek istiyor. Bu yüzden 5. bölümden itibaren sistemin olgunlaşan taraflarını anlattım.

Bu aşamada benim için en önemli konu yeni özellik eklerken çekirdek yapıyı bozmamaktı. Bu nedenle loglama, talep havuzu, PDF raporlama, Excel işlemleri, barkod yönetimi, karanlık tema ve mobil uyumluluk gibi modülleri ayrı ayrı düşünmedim. Hepsini mevcut servis katmanı, Entity Framework modeli ve ortak arayüz dosyalarıyla uyumlu hale getirmeye çalıştım. Böylece proje yalnızca farklı ekranlardan oluşan bir deneme uygulaması olmaktan çıktı. Daha düzenli takip edilebilen, kullanıcı davranışını hatırlayan ve operasyonu kolaylaştıran bir stok yönetim sistemine dönüştü.

## 5.1 Loglama, Audit Hafızası ve Talep Havuzu

Stok yönetimi uygulamalarında yapılan işlemin sonucunu görmek kadar o işlemin kaydını tutmak da önemlidir. Bir ürün neden azaldı, hangi kullanıcı işlem yaptı, StockAI hangi komutu yorumladı veya kullanıcı hangi öneriyi aldı gibi sorular sonradan cevaplanabilmelidir. Bu yüzden sistemde iki ayrı izleme yaklaşımı kullandım. Normal uygulama akışındaki hatalar ve bilgilendirmeler `ILogger` ile takip edilirken, StockAI tarafındaki konuşma ve işlem geçmişi için ayrıca `StockAiActionLog` modeli oluşturuldu.

Bu model benim için projenin kritik parçalarından biridir. Çünkü StockAI sadece cevap veren bir sohbet kutusu değildir. Bazı durumlarda ürün oluşturma, stok hareketi hazırlama veya risk analizi yapma gibi işlemlere dokunur. Böyle bir yapıda kullanıcının yazdığı komut ile sistemin verdiği cevabın kaybolmaması gerekir.

[KOD BLOĞU: StockAiActionLog modeli ile kalıcı AI işlem hafızası]
```csharp
public class StockAiActionLog
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [StringLength(100)]
    public string? Username { get; set; }

    [Required]
    [StringLength(60)]
    public string ActionType { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string Status { get; set; } = string.Empty;

    [StringLength(80)]
    public string? EntityKey { get; set; }

    [Required]
    [StringLength(1200)]
    public string UserPrompt { get; set; } = string.Empty;

    [Required]
    [StringLength(1600)]
    public string AgentResponse { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
```

Burada `ActionType` alanı işlemin türünü, `Status` alanı ise işlemin durumunu belirtir. Örneğin bir cevap yalnızca analiz ise `Insight`, kullanıcı onayı bekleyen bir taslak ise `Preview`, gerçekten uygulanmış bir işlem ise `Applied` olarak tutulur. Bu ayrım küçük gibi görünse de StockAI'nin güvenli çalışması için gereklidir. Çünkü sistemde her cevap aynı ağırlıkta değildir. Bir öneri ile gerçek stok hareketi aynı şekilde kaydedilmemelidir.

Audit kaydı ayrı bir servis üzerinden yazılır. Bunu özellikle ayırdım. Çünkü StockAI işlemi başarılı olsa bile audit kaydında geçici bir hata oluşabilir. Böyle bir durumda kullanıcının ana işlemini tamamen bozmak doğru olmaz. Bu yüzden audit servisinde hata yakalanır ve sistem loguna uyarı olarak düşülür. Ana işlem gereksiz yere kesilmez.

[KOD BLOĞU: StockAiAuditService içinde toleranslı audit yazma akışı]
```csharp
public async Task RecordAsync(
    int? userId,
    string? username,
    string actionType,
    string status,
    string? entityType,
    int? entityId,
    string? entityKey,
    string userPrompt,
    string agentResponse,
    string? metadata = null,
    CancellationToken cancellationToken = default)
{
    try
    {
        var log = new StockAiActionLog
        {
            UserId = userId,
            Username = Truncate(username, 100),
            ActionType = TruncateRequired(actionType, 60),
            Status = TruncateRequired(status, 40),
            EntityType = Truncate(entityType, 80),
            EntityId = entityId,
            EntityKey = Truncate(entityKey, 80),
            UserPrompt = TruncateRequired(userPrompt, 1200),
            AgentResponse = TruncateRequired(agentResponse, 1600),
            Metadata = Truncate(metadata, 2000),
            CreatedAt = DateTime.Now
        };

        await _context.StockAiActionLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "StockAI audit log yazılamadı. ActionType: {ActionType}", actionType);
    }
}
```

Talep havuzu tarafında amaç stokta olmayan veya ileride alınması planlanan ürünleri ayrı bir alanda takip etmekti. Bu özellik klasik stok kartından farklıdır. Stok kartı elde bulunan ürünü temsil eder. Talep havuzu ise henüz alınmamış veya takip edilen ihtiyacı temsil eder. Bu yüzden `WishlistService` içinde kullanıcıya ait aktif talepler, öncelik değerleri ve satın alınma durumu ayrı olarak yönetilir.

[KOD BLOĞU: WishlistService içinde kullanıcı bazlı aktif talep sorgusu]
```csharp
public async Task<IEnumerable<Wishlist>> GetActiveWishlistAsync(int userId)
{
    return await _context.Wishlists
        .Where(w => w.UserId == userId && !w.IsPurchased)
        .OrderByDescending(w => w.Priority)
        .ThenByDescending(w => w.CreatedDate)
        .ToListAsync();
}

public async Task MarkAsPurchasedAsync(int id)
{
    var wishlist = await _context.Wishlists.FindAsync(id);
    if (wishlist != null)
    {
        wishlist.IsPurchased = true;
        wishlist.PurchaseDate = DateTime.Now;
        wishlist.LastUpdatedDate = DateTime.Now;

        _context.Wishlists.Update(wishlist);
        await _context.SaveChangesAsync();
    }
}
```

Bu bölümdeki asıl kazanım şudur: uygulama yalnızca bugünkü stok miktarını göstermiyor. Geçmişte yapılan işlemi, StockAI'nin verdiği cevabı ve gelecekte alınması düşünülen ürünleri de sistem içinde tutuyor. Bu da depo yöneticisine hem geriye dönük kontrol hem de ileriye dönük planlama imkanı veriyor.

## 5.2 Responsive UI, Karanlık Tema ve Kullanıcı Deneyimi

Arayüz tarafında ilk hedefim uygulamayı sadece masaüstü ekranda değil, küçük ekranlarda da kullanılabilir hale getirmekti. Depo ortamında kullanıcı her zaman geniş monitör başında olmayabilir. Tablet, dizüstü bilgisayar veya küçük ekranlı bir cihaz üzerinden işlem yapılabilir. Bu yüzden sidebar yapısı, dashboard kartları, mobil menü, StockAI paneli ve bildirimler ortak CSS ve JavaScript dosyaları içinde düzenlendi.

Karanlık tema da bu çalışmanın bir parçasıdır. Depo gibi uzun süre kullanılan ekranlarda açık tema her zaman rahat olmayabilir. Bu nedenle tema değişimi kullanıcı tercihi olarak saklanır. Kullanıcı sayfayı yenilese bile seçtiği tema korunur.

[KOD BLOĞU: app.js içinde tema değiştirme ve logo uyarlama]
```javascript
function initThemeToggle() {
    const themeToggle = document.getElementById('themeToggle');
    const html = document.documentElement;
    const icon = themeToggle?.querySelector('i');
    const brandLogo = document.querySelector('.brand-logo-img');

    function applyTheme(theme) {
        const isDark = theme === 'dark';

        if (theme === 'dark') {
            html.setAttribute('data-theme', 'dark');
            icon.classList.remove('fa-moon');
            icon.classList.add('fa-sun');
        } else {
            html.removeAttribute('data-theme');
            icon.classList.remove('fa-sun');
            icon.classList.add('fa-moon');
        }

        if (brandLogo) {
            const logoSrc = isDark ? brandLogo.dataset.darkLogo : brandLogo.dataset.lightLogo;
            if (logoSrc) {
                brandLogo.src = logoSrc;
            }
        }
    }

    applyTheme(localStorage.getItem('theme') || 'light');

    themeToggle.addEventListener('click', function () {
        const newTheme = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
        applyTheme(newTheme);
        localStorage.setItem('theme', newTheme);
    });
}
```

CSS tarafında temayı ayrı ayrı tüm sınıflara yazmak yerine değişkenler üzerinden yönettim. Bu karar hem kod tekrarını azalttı hem de Bootstrap bileşenleriyle özel tasarımın birbirini ezmesini azalttı. Bir kartın, menünün veya metnin rengi değişecekse bunu tek tek aramak yerine tema değişkeni üzerinden kontrol etmek daha sağlıklı oldu.

[KOD BLOĞU: app.css içinde tema değişkenleri]
```css
:root {
    --primary: #1e88e5;
    --accent: #2196F3;
    --bg-gradient-start: #e8eef5;
    --bg-gradient-end: #f0f4f8;
    --text-primary: #222;
    --text-secondary: #666;
    --card-bg: rgba(255, 255, 255, 0.8);
    --border-color: rgba(255, 255, 255, 0.3);
}

[data-theme="dark"] {
    --primary: #42a5f5;
    --accent: #64b5f6;
    --bg-gradient-start: #0f1419;
    --bg-gradient-end: #1a1d2e;
    --text-primary: #e8eaed;
    --text-secondary: #b8bcc4;
    --card-bg: rgba(30, 33, 48, 0.8);
    --border-color: rgba(255, 255, 255, 0.1);
}
```

Mobil menüde asıl problem sidebar'ın küçük ekranda çok yer kaplamasıydı. Bu yüzden menüyü açılıp kapanan bir yapıya çevirdim. Arka plan perdesi, Escape tuşuyla kapanma ve odak yönetimi eklenince kullanıcı deneyimi daha kontrollü hale geldi. Bu detaylar küçük görünse de gerçek kullanımda uygulamanın daha düzgün hissettirmesini sağlıyor.

[KOD BLOĞU: app.js içinde mobil menü durum yönetimi]
```javascript
function setMenuState(isOpen) {
    body.classList.toggle('mobile-menu-open', isOpen);
    toggle.setAttribute('aria-expanded', String(isOpen));
    toggle.setAttribute('aria-label', isOpen ? 'Menüyü kapat' : 'Menüyü aç');
    backdrop.hidden = !isOpen;

    if (isOpen) {
        lastFocusedElement = document.activeElement;
        window.setTimeout(function () {
            const firstFocusable = sidebar.querySelector(focusableSelector);
            firstFocusable?.focus();
        }, 120);
        return;
    }

    if (lastFocusedElement instanceof HTMLElement) {
        lastFocusedElement.focus();
    }
}
```

[EKRAN GÖRÜNTÜSÜ: Mobil görünümde açılmış yan menü, karanlık tema düğmesi ve StockAI kısa yol butonu]

## 5.3 Power BI Konseptli Raporlama, Excel ve Barkod Yönetimi

Dashboard ekranı yalnızca anlık bilgi vermek için değil, yöneticiye çıktı alınabilir bir özet sunmak için de kullanıldı. İlk denemelerde HTML içeriğini doğrudan PDF'e çevirme yaklaşımı düşündüm. Fakat grafik, sayfa düzeni ve Türkçe karakter kontrolü açısından daha düzenli sonuç almak için rapor tarafında `pdfMake` ile şablon üretmek daha doğru oldu. Bu sayede kritik stok sayısı, toplam ürün, günlük hareket, stok sağlık puanı ve son hareketler tek rapor altında toplanabildi.

[KOD BLOĞU: Dashboard içinde PDF raporu oluşturma akışı]
```javascript
async function downloadPdfReport() {
    if (!window.pdfMake) {
        alert('PDF raporu oluşturulamadı. Lütfen sayfayı yenileyip tekrar deneyin.');
        return;
    }

    const chartCanvas = document.getElementById('trendChart');
    if (!chartCanvas) {
        alert('Grafik alanı bulunamadı.');
        return;
    }

    await new Promise((resolve) => requestAnimationFrame(resolve));

    downloadReportBtn.disabled = true;
    const originalLabel = downloadReportBtn.innerHTML;
    downloadReportBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Hazirlaniyor...';

    try {
        const docDefinition = buildPdfMakeReport(chartCanvas);
        window.pdfMake.createPdf(docDefinition).download(buildReportFileName());
    } finally {
        downloadReportBtn.disabled = false;
        downloadReportBtn.innerHTML = originalLabel;
    }
}
```

Excel modülü projenin en pratik bölümlerinden biri oldu. Çünkü gerçek hayatta birçok işletme ürün listesini zaten Excel dosyasında tutuyor. Bu dosyayı sisteme almak, kullanıcıya ciddi zaman kazandırır. Burada sadece dosya okumak yeterli değildi. SKU eşleşmesi, kategori kontrolü, yeni kategori oluşturma, fiyat formatı düzeltme ve hatalı satırı atlayıp diğer satırlara devam etme gibi kararlar da gerekiyordu.

[KOD BLOĞU: ProductController içinde Excel satır doğrulama ve upsert yaklaşımı]
```csharp
for (var row = 2; row <= rowCount; row++)
{
    var sku = worksheet.Cells[row, 1].Text?.Trim();
    var name = worksheet.Cells[row, 2].Text?.Trim();
    var categoryName = worksheet.Cells[row, 3].Text?.Trim();
    var priceText = worksheet.Cells[row, 4].Text?.Trim();
    var quantityText = worksheet.Cells[row, 5].Text?.Trim();

    if (string.IsNullOrWhiteSpace(sku) ||
        string.IsNullOrWhiteSpace(name) ||
        string.IsNullOrWhiteSpace(categoryName))
    {
        skippedCount++;
        errors.Add($"Satır {row}: SKU, Ürün Adı ve Kategori zorunludur.");
        continue;
    }

    if (!TryParseDecimal(priceText, out var price))
    {
        skippedCount++;
        errors.Add($"Satır {row}: Fiyat değeri geçersiz ({priceText}).");
        continue;
    }

    var normalizedSku = sku.ToUpperInvariant();
    if (productMap.TryGetValue(normalizedSku, out var existingProduct))
    {
        existingProduct.Name = name;
        existingProduct.Price = price;
        existingProduct.StockQuantity = Math.Max(0, existingProduct.StockQuantity + quantity);
        existingProduct.Category = category;
        updatedCount++;
    }
}
```

Fiyat alanı beklediğimden daha fazla sorun çıkardı. Bazı dosyalarda `53.400,00`, bazılarında `53,400.00`, bazılarında da para birimiyle birlikte yazılmış değerler vardı. Bu yüzden tek bir kültür ayarıyla parse etmek yeterli olmadı. Önce metni temizleyip sonra farklı formatlara göre deneme yapan bir yöntem kullandım.

[KOD BLOĞU: Kültür bağımsız fiyat dönüştürme]
```csharp
private static bool TryParseDecimal(string? value, out decimal result)
{
    result = 0;
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    var sanitized = value
        .Trim()
        .Replace("₺", string.Empty)
        .Replace("TL", string.Empty, StringComparison.OrdinalIgnoreCase)
        .Replace(" ", string.Empty);

    var normalized = sanitized;
    var hasDot = sanitized.Contains('.');
    var hasComma = sanitized.Contains(',');

    if (hasDot && hasComma)
    {
        normalized = sanitized.LastIndexOf(',') > sanitized.LastIndexOf('.')
            ? sanitized.Replace(".", string.Empty).Replace(',', '.')
            : sanitized.Replace(",", string.Empty);
    }
    else if (hasComma)
    {
        normalized = sanitized.Replace(',', '.');
    }

    return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out result)
        || decimal.TryParse(sanitized, NumberStyles.Number, CultureInfo.InvariantCulture, out result)
        || decimal.TryParse(sanitized, NumberStyles.Number, CultureInfo.GetCultureInfo("tr-TR"), out result);
}
```

Barkod tarafında iki ayrı ihtiyaç vardı. Birincisi mevcut ürünün barkodunu okuyup sistemde karşılığını bulmak. İkincisi ise ürün için yazdırılabilir etiket üretmek. Okuma tarafında barkod değeri SKU ile eşleştirildi. Yazdırma tarafında ise ürün detayından CODE128 barkod ve QR kod içeren etiket üretildi. Bu yapı fiziksel raf etiketi ile dijital ürün kartı arasında bağlantı kurdu.

[KOD BLOĞU: Barkod değerini SKU ile eşleştiren sunucu aksiyonu]
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> GetProductByBarcode([FromBody] BarcodeLookupRequest request)
{
    if (request == null || string.IsNullOrWhiteSpace(request.Barcode))
    {
        return BadRequest(new { success = false, message = "Barkod boş olamaz." });
    }

    var barcode = request.Barcode.Trim();
    var products = await _productService.GetAllActiveProductsAsync();
    var product = products.FirstOrDefault(p =>
        !string.IsNullOrWhiteSpace(p.SKU) &&
        string.Equals(p.SKU.Trim(), barcode, StringComparison.OrdinalIgnoreCase));

    if (product == null)
    {
        return NotFound(new { success = false, message = "Bu barkoda ait aktif ürün bulunamadı." });
    }

    return Json(new
    {
        success = true,
        id = product.Id,
        name = product.Name,
        currentStock = product.StockQuantity,
        sku = product.SKU
    });
}
```

[KOD BLOĞU: Barcode.cshtml içinde barkod ve QR etiket üretimi]
```javascript
function paintBarcode(svg) {
    JsBarcode(svg, sku, {
        format: 'CODE128',
        lineColor: '#111827',
        width: 2.25,
        height: 68,
        displayValue: false,
        margin: 18
    });
}

function paintQr(canvas) {
    new QRious({
        element: canvas,
        value: sku,
        size: 84,
        level: 'M',
        background: '#ffffff',
        foreground: '#111827'
    });
}

function renderLabels() {
    const count = clampCopyCount(copyInput.value);
    sheet.innerHTML = '';

    for (let index = 0; index < count; index += 1) {
        const label = template.content.firstElementChild.cloneNode(true);
        sheet.appendChild(label);
        paintBarcode(label.querySelector('.skuBarcode'));
        paintQr(label.querySelector('.skuQr'));
    }
}
```

[EKRAN GÖRÜNTÜSÜ: Karanlık temada dashboard, PDF rapor indirme butonu ve barkod yazdırma ekranı]

# 6. YAPAY ZEKA ENTEGRASYONUNUN İLERİ SEVİYESİ: STOCKAI

StockAI bölümü projenin en çok düşündüğüm kısmı oldu. İlk aşamada asistan yalnızca stokla ilgili sorulara cevap veren bir sohbet alanı gibi çalışıyordu. Daha sonra bu yapıyı ürün ekleme, ürün güncelleme, stok girişi, stok çıkışı, risk analizi ve hafıza sorgusu gibi işlemleri anlayabilecek hale getirdim. Burada dikkat ettiğim en önemli nokta şuydu: StockAI hiçbir zaman veritabanına doğrudan serbest şekilde müdahale etmemelidir.

Bu nedenle büyük dil modeli sadece metin üretimi ve yorumlama tarafında kullanıldı. Gerçek işlem yapılacaksa önce mesaj sınıflandırılır, alanlar ayrıştırılır, kullanıcının rolü kontrol edilir, risk seviyesi hesaplanır ve açık onay aranır. Bu şartlar sağlanmadan ürün veya stok üzerinde değişiklik yapılmaz.

## 6.1 Controller'dan Servis Katmanına Taşınan Agent Mimarisi

İlk olarak StockAI mantığını controller içinde büyütmemeye çalıştım. Controller yalnızca kullanıcının mesajını alır, session içinden rol ve kullanıcı bilgisini okur, sonra işi `StockAiAgentService` sınıfına devreder. Bu karar kodun okunabilirliği için önemliydi. Eğer bütün karar mantığı controller içinde kalsaydı ürün ekleme, stok hareketi, analiz, audit ve hata yönetimi kısa sürede iç içe geçerdi.

[KOD BLOĞU: StockAIController içinde session bilgisiyle agent servisine yönlendirme]
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Ask([FromBody] AskRequest request, CancellationToken cancellationToken)
{
    if (request == null || string.IsNullOrWhiteSpace(request.Message))
    {
        return BadRequest(new { response = "Lütfen bir mesaj girin." });
    }

    var userRole = HttpContext.Session.GetString("UserRole");
    var username = HttpContext.Session.GetString("Username");
    var userId = int.TryParse(HttpContext.Session.GetString("UserId"), out var parsedUserId)
        ? parsedUserId
        : (int?)null;

    var responseText = await _stockAiAgentService.ProcessAsync(
        request.Message,
        userRole,
        userId,
        username,
        cancellationToken);

    return Json(new { response = responseText });
}
```

Agent servisinde mesaj önce belirli niyetlere göre kontrol edilir. Kullanıcı stok hareketi yapmak istiyorsa bu LLM'e bırakılmaz. Aynı şekilde ürün ekleme, ürün güncelleme, risk analizi, audit geçmişi ve hafıza sorgusu gibi başlıklar da uygulamanın kendi kodu içinde değerlendirilir. Böylece sistem kritik kararları rastgele model cevabına bırakmamış olur.

[KOD BLOĞU: StockAiAgentService içinde niyet sınıflandırma sırası]
```csharp
public async Task<string> ProcessAsync(
    string message,
    string? userRole,
    int? userId,
    string? username,
    CancellationToken cancellationToken = default)
{
    var activeProducts = (await _productService.GetAllActiveProductsAsync()).ToList();
    var normalizedMessage = Normalize(message);

    if (IsStockMovementIntent(normalizedMessage))
    {
        return await HandleStockMovementAsync(message, userRole, userId, username, cancellationToken);
    }

    if (IsUpdateProductIntent(normalizedMessage))
    {
        return await HandleUpdateProductAsync(message, userRole, userId, username, cancellationToken);
    }

    if (IsCreateProductIntent(normalizedMessage))
    {
        return await HandleCreateProductAsync(message, userRole, userId, username, cancellationToken);
    }

    if (IsRiskAnalysisQuery(normalizedMessage))
    {
        var response = BuildRiskAnalysisResponse(activeProducts, userRole);
        await RecordInsightAsync(userId, username, "RiskAnalysis", message, response, cancellationToken);
        return response;
    }

    var enrichedPrompt = BuildInventoryGroundedPrompt(message, activeProducts);
    return await _geminiApiService.GenerateResponseAsync(enrichedPrompt, cancellationToken);
}
```

## 6.2 Güvenli Yazma İşlemleri: Parse, Yetki, Önizleme ve Onay

StockAI'nin yazma işlemi yapabilmesi için birkaç şartın aynı anda sağlanması gerekir. Kullanıcının rolü uygun olmalı, zorunlu alanlar eksiksiz gelmeli, kategori sistemde bulunmalı, SKU benzersiz olmalı ve komutta açık şekilde `onayla` ifadesi yer almalıdır. Bu zincir özellikle bilerek kuruldu. Çünkü doğal dil ile işlem yaptırmak kullanışlıdır fakat kontrolsüz bırakılırsa stok gibi hassas bir alanda risklidir.

[KOD BLOĞU: Ürün oluşturma işleminde önizleme ve açık onay]
```csharp
if (!CanWriteProducts(userRole))
{
    return "Bu işlem için ürün yazma yetkisi gerekiyor. Analiz yapabilirim ama ürün kaydı oluşturamam.";
}

var fields = ParseFields(message);
var validation = ValidateCreateFields(fields);
if (!string.IsNullOrWhiteSpace(validation))
{
    return validation;
}

var category = FindCategory(categories, fields["kategori"]);
if (category == null)
{
    return BuildCategoryNotFoundResponse(fields["kategori"], categories);
}

if (!HasApproval(message))
{
    var previewResponse = string.Join(Environment.NewLine, new[]
    {
        "Ürün ekleme önizlemesi hazır. Kaydetmem için aynı komuta `onayla` ekle.",
        $"- Ad: {name}",
        $"- SKU: {sku}",
        $"- Kategori: {category.Name}",
        $"- Fiyat: {price:0.##}",
        $"- Kritik stok: {critical}",
        $"- İlk stok: {stock}"
    });

    await RecordAuditAsync(userId, username, "ProductCreate", "Preview",
        "Product", null, sku, message, previewResponse,
        $"Category={category.Name};InitialStock={stock}", cancellationToken);

    return previewResponse;
}
```

Stok hareketlerinde kontrol daha serttir. Çünkü yanlış bir stok çıkışı ürün miktarını negatife düşürebilir veya kritik seviyenin altına indirebilir. Bu yüzden önce işlem sonrası stok hesaplanır. Eğer stok negatife düşüyorsa işlem durdurulur. Hareket büyükse veya kritik seviyeye yaklaştırıyorsa sistem normal onaya ek olarak daha güçlü bir risk onayı ister.

[KOD BLOĞU: Riskli stok hareketinde çift onay mantığı]
```csharp
var projectedStock = product.StockQuantity + signedQuantity;
if (projectedStock < 0)
{
    return $"Bu işlem stok miktarını negatife düşürür. Mevcut stok: {product.StockQuantity}, hareket: {signedQuantity}.";
}

var isRisky = IsRiskyStockMovement(product, signedQuantity, projectedStock);
if (!HasApproval(message))
{
    var previewResponse = BuildStockMovementPreview(
        product, movementKind, signedQuantity, projectedStock,
        description, isRisky, requiresRiskApproval: false);

    await RecordAuditAsync(userId, username, "StockMovement", "Preview",
        "Product", product.Id, product.SKU, message, previewResponse,
        $"Movement={movementKind};Quantity={signedQuantity};ProjectedStock={projectedStock};Risky={isRisky}",
        cancellationToken);

    return previewResponse;
}

if (isRisky && !HasRiskApproval(message))
{
    return BuildStockMovementPreview(
        product, movementKind, signedQuantity, projectedStock,
        description, isRisky, requiresRiskApproval: true);
}
```

İşlem uygulanacaksa StockAI yine doğrudan ürün tablosunu güncellemez. Stok hareketi `IStockMovementService` üzerinden yapılır. Bu benim özellikle koruduğum bir mimari sınırdır. Çünkü normal stok ekranı da StockAI de aynı servis kurallarından geçerse sistem davranışı tutarlı kalır.

[KOD BLOĞU: StockAI stok hareketini servis katmanından uygular]
```csharp
var agentDescription = $"StockAI: {description}".Trim();

switch (movementKind)
{
    case StockAiMovementKind.In:
        await _stockMovementService.RecordStockInAsync(product.Id, Math.Abs(quantity), agentDescription);
        break;

    case StockAiMovementKind.Out:
        await _stockMovementService.RecordStockOutAsync(product.Id, Math.Abs(quantity), agentDescription);
        break;

    case StockAiMovementKind.Adjustment:
        await _stockMovementService.RecordStockAdjustmentAsync(product.Id, quantity, agentDescription);
        break;
}

var response = $"Stok hareketi uygulandı: {product.Name} | SKU: {product.SKU} | Yeni stok: {projectedStock}.";
await RecordAuditAsync(userId, username, "StockMovement", "Applied",
    "Product", product.Id, product.SKU, message, response,
    $"Movement={movementKind};Quantity={signedQuantity};ProjectedStock={projectedStock};Risky={isRisky}",
    cancellationToken);
```

## 6.3 Canlı Veriye Dayalı Risk Analizi ve Geri Alma Önerisi

StockAI tarafında en önemli kurallardan biri veri uydurmamasıdır. Kullanıcı stokla ilgili soru sorduğunda modelin tahmin yürütmesi yerine uygulamadaki canlı ürün listesinden yararlanması gerekir. Bu yüzden genel modele gidilecek sorularda bile önce aktif ürünler alınır. Ürün adı, SKU, kategori, stok miktarı, kritik seviye ve fiyat bilgisi sınırlı bir bağlam olarak prompt içine eklenir.

[KOD BLOĞU: Canlı stok verisiyle sınırlandırılmış prompt üretimi]
```csharp
private static string BuildInventoryGroundedPrompt(string userMessage, List<Product> products)
{
    var snapshot = new StringBuilder();
    snapshot.AppendLine("[CANLI_STOK_VERISI]");
    snapshot.AppendLine($"ToplamAktifUrun={products.Count}");

    foreach (var product in products.OrderBy(p => p.Name).Take(80))
    {
        snapshot.AppendLine(
            $"Urun={product.Name};SKU={product.SKU};Kategori={product.Category?.Name ?? "-"};" +
            $"Stok={product.StockQuantity};Kritik={product.CriticalStockLevel};Fiyat={product.Price}");
    }

    snapshot.AppendLine("[/CANLI_STOK_VERISI]");
    snapshot.AppendLine("KURAL: Sadece yukarıdaki canlı stok verisine dayanarak cevap ver.");
    snapshot.AppendLine("KURAL: Veri dışında ürün/kategori uydurma.");
    snapshot.AppendLine("Kullanıcı Sorusu:");
    snapshot.AppendLine(userMessage.Trim());

    return snapshot.ToString();
}
```

Geri alma kısmında doğrudan veritabanını eski haline sarmayı doğru bulmadım. Çünkü stok hareketinden sonra başka hareketler eklenmiş olabilir. Bir ürün oluşturulduktan sonra ona hareket bağlanmış olabilir. Bu yüzden StockAI geri alma işini kör şekilde yapmaz. Son uygulanmış işlemi audit hafızasından bulur. Eğer bu işlem stok hareketiyse ters yönde yeni bir onaylı komut önerir. Ürün oluşturma veya güncelleme gibi daha hassas işlemlerde ise manuel kontrol önerir.

[KOD BLOĞU: Audit metadata üzerinden güvenli geri alma önerisi]
```csharp
var logs = await _stockAiAuditService.GetRecentAsync(userId, includeAllUsers, 5, cancellationToken);
var lastApplied = logs.FirstOrDefault(log =>
    string.Equals(log.Status, "Applied", StringComparison.OrdinalIgnoreCase));

if (lastApplied != null &&
    string.Equals(lastApplied.ActionType, "StockMovement", StringComparison.OrdinalIgnoreCase))
{
    var quantity = ExtractMetadataInt(lastApplied.Metadata, "Quantity");
    if (quantity.HasValue && !string.IsNullOrWhiteSpace(lastApplied.EntityKey))
    {
        var inverseQuantity = -quantity.Value;
        var movementCommand = inverseQuantity > 0
            ? $"stok giriş sku={lastApplied.EntityKey} miktar={inverseQuantity} açıklama=StockAI geri alma onayla"
            : $"stok çıkış sku={lastApplied.EntityKey} miktar={Math.Abs(inverseQuantity)} açıklama=StockAI geri alma onayla";

        lines.Add("Güvenli geri alma önerisi: Son stok hareketinin tersini yeni bir onaylı stok hareketi olarak işle.");
        lines.Add($"Önerilen komut: `{movementCommand}`");
    }
}
```

Bu yaklaşım StockAI'nin sınırlarını netleştirir. Sistem kullanıcıya yardımcı olur fakat son kararı tamamen modelin eline bırakmaz. Özellikle stok gibi sayısal doğruluğun önemli olduğu bir alanda bu ayrım gereklidir.

[EKRAN GÖRÜNTÜSÜ: StockAI üzerinden riskli stok çıkışı komutu, önizleme mesajı ve kesin onay uyarısı]

## 6.4 Gerçek Zamanlı Bildirimlerle AI ve Stok Servislerinin Birleşmesi

StockAI ile yapılan stok hareketleri normal stok ekranıyla aynı servisten geçtiği için kritik stok bildirimleri de ortak çalışır. Bu benim için önemliydi. Eğer StockAI ayrı bir yoldan stok düşseydi bildirim mekanizması atlanabilirdi. Mevcut yapıda stok miktarı kritik seviyeye inerse `StockMovementService` SignalR üzerinden bağlı istemcilere uyarı gönderir.

[KOD BLOĞU: Stok servisi içinde kritik stok bildirimi]
```csharp
private async Task NotifyCriticalStockAsync(Product product)
{
    if (product.StockQuantity > product.CriticalStockLevel)
    {
        return;
    }

    try
    {
        await _hubContext.Clients.All.SendAsync("ReceiveStockAlert", new
        {
            ProductName = product.Name,
            RemainingStock = product.StockQuantity,
            CriticalLevel = product.CriticalStockLevel,
            AlertTime = DateTime.Now.ToString("HH:mm:ss")
        });
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Kritik stok bildirimi yayınlanırken hata oluştu. ProductId: {ProductId}", product.Id);
    }
}
```

İstemci tarafında bağlantı koparsa otomatik yeniden bağlanma denenir. Kullanıcı bildirimleri kapattıysa toast gösterilmez. Bu şekilde canlı sistem davranışı ile kullanıcı tercihi aynı anda dikkate alınmış olur.

[KOD BLOĞU: app.js içinde SignalR uyarı dinleyicisi]
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build();

connection.on('ReceiveStockAlert', function (payload) {
    if (getFlag('isPushEnabled')) {
        showStockAlertToast(payload);
    }
});

async function startConnection() {
    try {
        await connection.start();
    } catch {
        window.setTimeout(startConnection, 5000);
    }
}

startConnection();
```

# 7. TEZ SAVUNMASI İÇİN HAZIRLIK NOTLARI

Savunmada özellikle şunu anlatmak gerekir: StockifyPlus yalnızca ürün ekleme ve silme ekranlarından oluşan basit bir CRUD uygulaması değildir. Projede servis katmanı, Repository/Unit of Work yapısı, Entity Framework Core model kuralları, SignalR bildirimleri, Excel işleme, PDF raporlama, barkod üretimi ve StockAI agent mimarisi birlikte çalışır. Bu yüzden savunmada özellikleri tek tek saymak yerine bu parçaların aynı veri bütünlüğü çizgisi üzerinde nasıl birleştiğini anlatmak daha doğru olur.

İlk güçlü nokta veri modelidir. Ürünlerde SKU alanı benzersiz indeksle korunur. Ürün-kategori ilişkisi `Restrict` ile tanımlanmıştır. Böylece kategori tarafındaki hatalı bir silme işlemi ürün verisini beklenmedik şekilde bozmaz. Stok hareketleri ise ürünle doğrudan ilişkili tutulur. Bu kararlar veritabanı tarafında da iş kurallarının desteklenmesini sağlar.

[KOD BLOĞU: ApplicationDbContext içinde veri bütünlüğü ve performans indeksleri]
```csharp
modelBuilder.Entity<Product>(entity =>
{
    entity.Property(e => e.SKU)
        .IsRequired()
        .HasMaxLength(50);

    entity.HasIndex(e => e.SKU)
        .IsUnique()
        .HasName("IX_Product_SKU_Unique");

    entity.HasIndex(e => new { e.CategoryId, e.IsActive })
        .HasName("IX_Product_Category_Active");

    entity.HasOne(e => e.Category)
        .WithMany(c => c.Products)
        .HasForeignKey(e => e.CategoryId)
        .OnDelete(DeleteBehavior.Restrict)
        .HasConstraintName("FK_Product_Category");
});
```

İkinci güçlü nokta servis katmanı disiplinidir. Stok çıkışı yapılırken ürün miktarı controller içinde veya StockAI içinde doğrudan azaltılmaz. İşlem `StockMovementService` üzerinden geçer. Servis önce miktarı kontrol eder, sonra hareket kaydını oluşturur ve ürünü günceller. Bu yapı aynı kuralın farklı ekranlarda farklı çalışmasını engeller.

[KOD BLOĞU: Stok çıkışında yetersiz stok kontrolü ve kayıt]
```csharp
public async Task<StockMovement> RecordStockOutAsync(int productId, int quantity, string? description)
{
    if (quantity <= 0)
        throw new ValidationException("Çıkış miktarı 0'dan büyük olmalıdır.");

    var product = await _productService.GetProductByIdAsync(productId);

    if (product.StockQuantity < quantity)
        throw new BusinessException($"Ürün için yeterli stok yok. Mevcut: {product.StockQuantity}, İstenen: {quantity}");

    product.StockQuantity -= quantity;

    var movement = new StockMovement
    {
        ProductId = productId,
        MovementType = MovementType.Çıkış,
        Quantity = quantity,
        MovementDate = DateTime.Now,
        Description = description?.Trim() ?? string.Empty
    };

    await _unitOfWork.StockMovementRepository.AddAsync(movement);
    _unitOfWork.ProductRepository.Update(product);
    await _unitOfWork.SaveChangesAsync();

    return movement;
}
```

Üçüncü güçlü nokta geliştirme sürecinde karşılaşılan hatalardır. Fiyat formatları her dosyada aynı gelmedi. SignalR bağlantısında kopma ve tekrar bağlanma senaryoları test edildi. PDF üretiminde grafik henüz çizilmeden çıktı alma sorunu yaşandı. Mobil menüde ekran küçükken kullanılabilirlik bozuldu. StockAI tarafında ise en büyük risk modelin kontrolsüz işlem yapabilmesiydi. Bu sorunların her biri kodda ayrı bir kararla çözüldü.

StockAI anlatılırken özellikle şu nokta vurgulanmalıdır: StockAI veritabanına doğrudan komut gönderen bağımsız bir model değildir. Uygulama servisleriyle sınırlandırılmış, onay mekanizmasına bağlı ve audit kaydı tutan kontrollü bir ajan katmanıdır. Bu ayrım projenin teknik güvenlik tarafını güçlendiren en önemli kararlardan biridir.

[EKRAN GÖRÜNTÜSÜ: StockAI hafıza/audit sorgusu, son işlemler listesi ve önerilen geri alma komutu]

# 8. SONUÇLAR

StockifyPlus çalışması sonunda küçük ve orta ölçekli işletmelerin kullanabileceği bütünleşik bir stok yönetim uygulaması ortaya çıktı. Ürün ve kategori yönetimi, stok giriş-çıkış işlemleri, kritik stok bildirimi, Excel içe ve dışa aktarımı, barkod okuma, barkod yazdırma, dashboard raporlama, karanlık tema, mobil uyum ve StockAI agent yapısı aynı ASP.NET Core MVC projesi içinde toplandı.

Bu projenin değeri yalnızca çok sayıda özellik içermesi değildir. Asıl değer bu özelliklerin aynı mimari düzen içinde çalışmasıdır. Entity Framework Core ile kurulan veri modeli, Repository/Unit of Work yapısı, servis katmanı, SignalR bildirimleri, PDF ve Excel işlemleri, barkod desteği ve StockAI audit hafızası birbirini tamamlayan parçalar haline geldi.

StockAI tarafında ulaşılan sonuç ayrıca önemlidir. Yapay zeka desteği doğrudan ve sınırsız işlem yapan bir yapı olarak değil, parse eden, doğrulayan, önizleme sunan, onay bekleyen ve işlemi servis katmanından uygulayan kontrollü bir yardımcı olarak tasarlandı. Bu yaklaşım stok yönetimi gibi veri bütünlüğünün önemli olduğu bir alanda daha güvenli bir çözüm sundu.

Geliştirme sürecinde fiyat formatı uyumsuzlukları, PDF render zamanlaması, mobil ekran sorunları, SignalR bağlantı yönetimi ve StockAI güvenliği gibi birçok problemle karşılaştım. Bu problemler proje için zaman kaybı gibi görünse de aslında uygulamayı olgunlaştıran kısımlar oldu. Mevcut haliyle StockifyPlus ileride çok şubeli depo yönetimi, satış hızına dayalı tedarik tahmini, rol bazlı görev atama ve daha detaylı rapor şablonları için sağlam bir temel sunmaktadır.

[EKRAN GÖRÜNTÜSÜ: Dashboard, StockAI paneli, barkod etiketi ve rapor çıktısını birlikte gösteren kapanış kolajı]

# KAYNAKLAR

Bu kaynakça hazırlanırken projede gerçekten kullanılan teknolojiler esas alınmıştır. Özellikle ASP.NET Core MVC, Entity Framework Core, SQL Server, SignalR, Background Service, EPPlus, Chart.js, pdfMake, barkod/QR kütüphaneleri ve Gemini/Groq API dokümantasyonları dikkate alınmıştır. Kaynak seçiminde blog yazıları yerine resmi dokümantasyonlar ve kütüphanelerin kendi doküman sayfaları tercih edilmiştir.

1. Microsoft. (2026). *ASP.NET Core documentation*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/aspnet/core/

2. Microsoft. (2026). *ASP.NET Core MVC overview and web application development*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/aspnet/core/mvc/overview

3. Microsoft. (2026). *Entity Framework Core documentation*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/ef/core/

4. Microsoft. (2026). *SQL Server technical documentation*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/sql/sql-server/

5. Microsoft. (2026). *Overview of ASP.NET Core SignalR*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction

6. Microsoft. (2026). *Background tasks with hosted services in ASP.NET Core*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services

7. Microsoft. (2026). *Dependency injection in ASP.NET Core*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

8. Microsoft. (2026). *Session and state management in ASP.NET Core*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state

9. Microsoft. (2026). *Razor syntax reference for ASP.NET Core*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
   https://learn.microsoft.com/en-us/aspnet/core/mvc/views/razor

10. EPPlus Software AB. (2026). *EPPlus documentation*. Erişim tarihi: 02.05.2026.  
    https://epplussoftware.com/docs/

11. EPPlus Software AB. (2026). *EPPlus: Excel spreadsheets for .NET*. Erişim tarihi: 02.05.2026.  
    https://www.epplussoftware.com/

12. Chart.js Contributors. (2026). *Chart.js documentation*. Erişim tarihi: 02.05.2026.  
    https://www.chartjs.org/docs/latest/

13. pdfMake. (2026). *pdfMake documentation*. Erişim tarihi: 02.05.2026.  
    https://pdfmake.org/

14. scanapp-org. (2026). *html5-qrcode documentation*. Erişim tarihi: 02.05.2026.  
    https://scanapp.org/html5-qrcode-docs/

15. mebjas. (2026). *html5-qrcode GitHub repository*. GitHub. Erişim tarihi: 02.05.2026.  
    https://github.com/mebjas/html5-qrcode

16. JsBarcode Contributors. (2026). *JsBarcode documentation and repository*. Erişim tarihi: 02.05.2026.  
    https://github.com/lindell/JsBarcode

17. neocotic. (2026). *QRious: Pure JavaScript QR code generation using canvas*. GitHub. Erişim tarihi: 02.05.2026.  
    https://github.com/neocotic/qrious

18. Bootstrap Team. (2026). *Bootstrap 5 documentation*. Erişim tarihi: 02.05.2026.  
    https://getbootstrap.com/docs/5.3/

19. MDN Web Docs. (2025). *Window: localStorage property*. Mozilla Developer Network. Erişim tarihi: 02.05.2026.  
    https://developer.mozilla.org/docs/Web/API/Window/localStorage

20. Google AI for Developers. (2026). *Gemini API reference*. Erişim tarihi: 02.05.2026.  
    https://ai.google.dev/api

21. Groq. (2026). *Groq API reference*. GroqDocs. Erişim tarihi: 02.05.2026.  
    https://console.groq.com/docs/api-reference

22. Microsoft. (2026). *.NET logging in C# and ASP.NET Core*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
    https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

23. Microsoft. (2026). *Entity Framework Core indexes*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
    https://learn.microsoft.com/en-us/ef/core/modeling/indexes

24. Microsoft. (2026). *Entity Framework Core relationships*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
    https://learn.microsoft.com/en-us/ef/core/modeling/relationships

25. Microsoft. (2026). *Entity Framework Core migrations overview*. Microsoft Learn. Erişim tarihi: 02.05.2026.  
    https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

26. StockifyPlus proje kaynak kodları. (2026). *StockifyPlus ASP.NET Core MVC stok yönetim uygulaması*. Yerel proje deposu: `StockifyPlus`. İncelenen dosyalar: `Program.cs`, `ApplicationDbContext.cs`, `ProductController.cs`, `StockMovementService.cs`, `StockAiAgentService.cs`, `StockAiAuditService.cs`, `app.js`, `app.css`, `Home/Index.cshtml`, `Product/Barcode.cshtml`.
