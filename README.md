# 

# ✉️ MyAcademy_IdentityMailProject - CyberMail 

CyberMail, ASP.NET Core 8.0 MVC ve ASP.NET Core Identity altyapısı kullanılarak geliştirilmiş, rol tabanlı yetkilendirme, gelişmiş analitik panelleri, dosya ekleme özellikli mesajlaşma akışları ve otomatik sistem kurulumu (Data Seeding) mekanizmalarına sahip modüler bir kurumsal iletişim platformudur.

> **💡 Modern Tasarım Yaklaşımı:** Projenin tüm görsel tasarımı, UI elemanları ve CSS yapısı tamamen **Stitch AI** yapay zeka aracı ile işbirliği yapılarak geliştirilmiştir.

## 📸 Ekran Görüntüleri

| **Giriş Yap** |
<img width="1600" height="803" alt="Login" src="https://github.com/user-attachments/assets/6804e8c2-481f-4d62-b24b-dcdfb4395dbc" />

| **Kayıt Ol** |
<img width="1600" height="798" alt="Register" src="https://github.com/user-attachments/assets/5ed75580-fba4-4c81-afa7-d1ddd038bceb" />

| **Doğrulama** |
<img width="1600" height="798" alt="VerifyQuestion" src="https://github.com/user-attachments/assets/623a118b-99a0-42f3-9d79-09283ef777d0" />

| **Doğrulama 2** |
<img width="1600" height="799" alt="VerifyEmail" src="https://github.com/user-attachments/assets/75719f08-726d-4774-8c52-7ade9314b7fb" />

| **Şifremi Unuttum** |
<img width="1600" height="799" alt="ForgetPassword" src="https://github.com/user-attachments/assets/8a7944be-6bdc-43f6-ad3e-1f16f32df9ff" />

| **Şifremi Unuttum 2** |
<img width="1600" height="800" alt="ForgetPassword2" src="https://github.com/user-attachments/assets/b32639e9-430e-4971-b913-8dc639bf50f1" />

| **Şifremi Unuttum 3** |
<img width="1600" height="799" alt="ForgetPassword3" src="https://github.com/user-attachments/assets/740aba59-86b4-4708-a53e-df53f3ed8b50" />

| **Kullanıcı Profili** |
<img width="1600" height="803" alt="UserProfile" src="https://github.com/user-attachments/assets/e4afb06c-eb05-4aed-8d14-a7d76696ae21" />

| **Mesajlar Sayfası** |
<img width="1600" height="800" alt="MessageIndex" src="https://github.com/user-attachments/assets/fe02f3aa-eea5-4565-b29f-80b5fed041a5" />

| **Kategoriye Göre Mesajlar** |
<img width="1600" height="798" alt="MessageCategories" src="https://github.com/user-attachments/assets/b65b86e2-3d47-4f0b-9251-448e995b95d2" />





## 🚀 Öne Çıkan Özellikler

### 1. Kimlik Doğrulama, Yetkilendirme & Profil

* **🔒 Güvenli Altyapı:** ASP.NET Core Identity ile RBAC (`Süper Admin`, `Admin`, `Standart Kullanıcı`).

* **🛡️ E-Posta & Şifre Yönetimi:** `MimeMessage` (MailKit/MimeKit) kütüphanesi ile benzersiz e-posta kontrolü, token tabanlı mail doğrulama ve link üzerinden şifre sıfırlama (Forgot Password).

* **👤 Profil Paneli:** Bilgi, şifre ve **avatar** resmi güncelleme imkanı.

### 2. Gelişmiş Mesajlaşma Akışları

* **📬 Kapsamlı Mesaj Kutuları:** Gelen, Giden, Taslaklar, Önemli (Yıldızlı) ve Çöp Kutusu (Taşıma/Geri Yükleme/Kalıcı Silme) klasörleri.

* **📎 Mesaj Özellikleri:** Kayıtlı kullanıcılara **dosya eki (Attachment)** ile mesaj gönderimi, mesaj yanıtlama (Reply) ve otomatik okundu takibi.

* **🚫 Spam Bildirimi:** İstenmeyen mesajlar için "Şikayet Et" seçeneği.

### 3. Arama, Filtreleme ve Kategori Yönetimi

* **🔍 Dinamik Arama:** Kapsam (İsim, Konu, İçerik) seçimi ile anlık arama.

* **📊 Çoklu Filtre & Sıralama:** Tarih (Yeni/Eski), alfabetik, okunma durumu ve kategori bazlı süzme.

* **🏷️ Kategori Sistemi:** Admin tarafından yönetilen ikonlu mesaj kategorileri ve mesajlara atanması.

### 4. Admin Analitik Paneli & Yönetim

* **📈 Dashboard Metrikleri:** Toplam/aktif kullanıcı, toplam/bugün gönderilen mesaj, okunmamış/çöp kutusundaki mesaj hacmi.

* **📊 Aktivite Raporları:** En fazla mesaj gönderen kullanıcılar (Top Senders) ve en çok kullanılan kategorilerin analizi.

* **🛠️ Yönetim Paneli:** Kullanıcı pasife alma, rol atama ve şikayet edilen mesajları inceleme yetkisi.

### 5. Otomatik Sistem Kurulumu (Data Seeding)

* **🛠️ DbInitializer Mekanizması:** Uygulama ilk kez ayağa kalktığında otomatik olarak;

  * 3 temel rolü (`Süper Admin`, `Admin`, `Standart Kullanıcı`),

  * 5 sistem korumalı varsayılan kategoriyi,

  * Ve varsayılan Süper Admin hesabını (`auth@cybermail.com`) veritabanına ekler (Plug-and-Play).

## 🛠️ Teknolojik Mimarisi

* **Framework:** .NET 8.0 / ASP.NET Core MVC

* **Authentication & ORM:** ASP.NET Core Identity & Entity Framework Core

* **Database:** Microsoft SQL Server

* **Frontend & Tasarım:** **Stitch AI** ile üretilmiş Razor Views (CSHTML), Tailwind CSS, Material Symbols Icons

* **Mail Kütüphanesi:** MimeKit & MailKit (for MimeMessage)

* **Design Pattern:** Repository & Dependency Injection (IoC Scope Management)

## ⚡ Kurulum ve Çalıştırma

### Gereksinimler

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

* SQL Server (LocalDB veya Server Instance)

### Adım Adım Kurulum

1. **Projeyi klonlayın:**

   ```
   git clone https://github.com/kullanici-adi/CyberMail.git
   cd CyberMail
   
   ```

2. **Veritabanı bağlantısını yapılandırın:**
   `appsettings.json` dosyasındaki SQL Server bağlantı dizesini (Connection String) kendi local SQL Server'ınıza göre güncelleyin. `DbInitializer` otomatik kurulumu bu veritabanına yapacaktır.

3. **Veritabanı Migrasyonlarını Uygulayın ve Çalıştırın:**

   ```
   dotnet ef database update
   dotnet run
   
   ```

> ℹ️ **Önemli Not:** `DbInitializer` sınıfı, uygulama ilk başladığında gerekli rolleri, kategorileri ve yönetici hesabını veritabanına otomatik ekler. Ek bir seed komutu çalıştırmanıza gerek yoktur.

## 🔐 Varsayılan Giriş Bilgileri (Geliştirici Hesabı)

Sistemi hemen test etmek için `DbInitializer` tarafından otomatik oluşturulan Süper Admin hesabı:

* **E-Posta:** `auth@cybermail.com`

* **Şifre:** `Admin123!`

* **Rol:** `Süper Admin`

## 📝 Lisans

Bu proje eğitim ve portföy amacıyla geliştirilmiştir. MIT Lisansı altında açık kaynaklıdır.
