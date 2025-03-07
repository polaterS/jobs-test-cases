# Cityberry Travel Solution Documentation

## Proje Özeti

Cityberry Travel, seyahat destinasyonlarını kolayca yönetebilen bir web uygulamasıdır. Kullanıcılar, destinasyonları görüntüleyebilir, ekleyebilir, düzenleyebilir ve silebilir. Her destinasyon için tarih-saat seçenekleri tanımlanabilir, böylece müşteriler hangi tarihlerde seyahat edebileceklerini görebilirler.

## Mimari Yaklaşım

### Katmanlı Mimari

Proje, sorumlulukların açıkça ayrıldığı katmanlı bir mimari kullanmaktadır:

- **Sunum Katmanı**: MVC Views ve Controllers
- **İş Katmanı**: Servisler ve iş mantığı
- **Veri Erişim Katmanı**: Repository pattern ve UnitOfWork
- **Veritabanı Katmanı**: PostgreSQL

### Repository Pattern

Veritabanı işlemleri için Repository Pattern uygulanmıştır. Bu, veri erişim kodunun daha sürdürülebilir ve test edilebilir olmasını sağlar.

### Unit of Work Pattern

Unit of Work pattern, iş birimlerinin bütünlüğünü sağlamak için kullanılmıştır. Bu, birden fazla repository'nin tek bir transaction içinde çalışmasını sağlar.

### Önbellekleme Stratejisi

Performans optimizasyonu için memory cache kullanılmıştır. Önbellekleme stratejisi, sık kullanılan verileri hızlıca erişilebilir kılmak için uygulanmıştır.

Önbellek yönetimi:
- Read-through önbellekleme: Veriler önce önbellekte aranır, bulunamazsa veritabanından alınıp önbelleğe kaydedilir.
- Write-through önbellekleme: Veri güncellendikçe önbellek de güncellenir.
- Önbellek süresi: 1 saat olarak ayarlanmıştır.

## Kullanılan Teknolojiler

### Backend

- **ASP.NET Core MVC**: Web uygulaması çerçevesi
- **Entity Framework Core**: ORM (Object-Relational Mapping) aracı
- **PostgreSQL**: İlişkisel veritabanı sistemi
- **Memory Cache**: Önbellekleme çözümü (Redis yerine geçici çözüm)

### Frontend

- **Tailwind CSS**: Modern, utility-first CSS framework
- **Alpine.js**: Hafif JavaScript framework'ü
- **Font Awesome**: İkon kütüphanesi

### Geliştirme Araçları

- **Git**: Versiyon kontrol sistemi
- **Visual Studio/Visual Studio Code**: Geliştirme ortamı
- **NuGet**: Paket yöneticisi


## Ana Özellikler

### Destinasyon Yönetimi

Kullanıcılar aşağıdaki işlemleri yapabilirler:
- Tüm destinasyonları listeleme
- Detaylı destinasyon bilgilerini görüntüleme
- Yeni destinasyon ekleme
- Mevcut destinasyonu düzenleme
- Destinasyon silme

### Tarih-Saat Yönetimi

Her destinasyon için çoklu tarih-saat seçenekleri tanımlanabilir:
- Datetime seçicilerle kullanıcı dostu arayüz
- Dinamik olarak tarih-saat ekleyebilme/kaldırabilme
- Geçmiş tarihlerin filtrelenmesi

### Kullanıcı Arayüzü

Modern ve kullanıcı dostu bir arayüz tasarlanmıştır:
- Temiz, minimalist tasarım
- Sezgisel navigasyon

## API ve İntegrasyon

RESTful API, üçüncü taraf uygulamalarla entegrasyon için yapılandırılmıştır:
- Standard HTTP metodları (GET, POST, PUT, DELETE)
- JSON veri formatı
- Swagger dökümantasyonu

## Cache Yönetimi

Performans optimizasyonu için sistematik önbellekleme stratejisi:
- Statik ve sık kullanılan veriler için önbellekleme
- Önbellek temizleme ve geçersiz kılma mekanizmaları
- Veri tutarlılığını sağlama yöntemleri



## Proje Zorlukları ve Çözümleri

### Tarih-Saat Yönetimi

**Zorluk**: HTML datetime-local input alanlarıyla çalışırken format ve saat dilimi sorunları.

**Çözüm**: 
- ISO 8601 formatında (yyyy-MM-ddTHH:mm) tarih-saat değerlerinin kullanımı
- JavaScript ile tarih değerlerinin doğru formatlanması
- Hem client-side hem de server-side validasyon

### Önbellekleme Zorlukları

**Zorluk**: Redis sunucusu bağlantı sorunları.

**Çözüm**:
- In-memory önbelleklemeye geçiş
- Fallback mekanizmaları
- Graceful degradation yaklaşımı
