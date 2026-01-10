# Sensei 🥋
<img width="1024" height="572" alt="sensei_banner" src="https://github.com/user-attachments/assets/cd6852ba-3f0f-429d-b6b6-339e61035797" />

🌐 **Resmi Web Sitesi:** [senseithelastsamurai.netlify.app](https://senseithelastsamurai.netlify.app/)

## 📖 Project Overview (Proje Özeti)

**Sensei**, klasik dövüş sanatları temalı, 2D pixel art grafiklere sahip zorlu bir aksiyon ve platform oyunudur. Oyuncular, bilge ve yetenekli bir Samuray ustasını (Sensei) kontrol ederek, karanlık bir ormanda pusuya yatmış birbirinden tehlikeli ve benzersiz yeteneklere sahip boss'lara karşı hayatta kalma mücadelesi verirler.

Bu proje, oyun geliştirme dersi kapsamında geliştirilmiş olup; hassas dövüş mekanikleri, karakter animasyonları, hit-box mantığı ve zorlu boss savaşları üzerine odaklanmaktadır.

**Temel Özellikler:**
* **Tek Kahraman, Zorlu Düşmanlar:** Beyaz saçlı, tecrübeli bir Samuray ustasını kontrol edin.
* **Benzersiz Boss Savaşları:** Her biri farklı saldırı modellerine ve stratejilere sahip çeşitli boss karakterleriyle (Kırmızı Zırhlı Samuray, Dev Şövalye, Panda Savaşçı, Okçu, Suikastçı vb.) yüzleşin.
* **Dinamik Dövüş Sistemi:** Hassas zamanlama gerektiren saldırı, savunma ve kaçınma mekanikleri.
* **Atmosferik Pixel Art:** Sonbahar yapraklarıyla kaplı orman temalı detaylı pixel art grafikler ve akıcı animasyonlar.
* **Can Barı Sistemi:** Hem oyuncu hem de boss için dinamik sağlık göstergeleri.

## 🛠 Technology Stack (Teknolojiler)

Bu proje aşağıdaki teknolojiler ve araçlar kullanılarak geliştirilmiştir:

* **Oyun Motoru:** Unity 2022.3 LTS
* **Programlama Dili:** C#
* **IDE:** Visual Studio / VS Code
* **UI Tasarım:** Gemini Banana Pro
* **Sürüm Kontrol:** Git & GitHub

## ⚙️ Installation & Setup (Kurulum)

Projeyi yerel makinenizde çalıştırmak için aşağıdaki adımları izleyin:

1.  **Repoyu Klonlayın:**
    Terminal veya Komut İstemi'ni açın ve şu komutu girin:
    ```bash
    git clone [https://github.com/berkeduruu/Sensei.git](https://github.com/berkeduruu/Sensei.git)
    ```

2.  **Unity Hub ile Açın:**
    * Unity Hub'ı başlatın.
    * "Add" butonuna tıklayın ve klonladığınız `Sensei` klasörünü seçin.
    * Projenin uyumlu olduğu Unity sürümünün yüklü olduğundan emin olun.

3.  **Projeyi Yükleyin:**
    * Projeye tıklayarak Unity Editor'de açılmasını bekleyin.

4.  **Oyunu Başlatın:**
    * `Assets/Scenes` klasörü altındaki ana oyun sahnesini açın.
    * Yukarıdaki ▶️ **Play** butonuna basarak oyunu test edebilirsiniz.

## 🎮 Usage Instructions (Kullanım ve Kontroller)

Oyun şu an için PC platformu (Klavye girişi) için tasarlanmıştır.

| Eylem | Tuş |
| :--- | :--- |
| **Hareket** | `A` (Sol) / `D` (Sağ) |
| **Zıplama** | `Space` (Boşluk) |
| **Saldırı (Hafif)** | `Sol Tık` veya `J` |
| **Saldırı (Ağır)** | `Sağ Tık` veya `K` |
| **Dash (Atılma)** | `Shift` |
| **Menü / Durdur** | `ESC` |

## 🔑 API Keys & Environment Variables

* Bu proje şu aşamada harici bir API veya bulut veritabanı bağlantısı **gerektirmez**.
* Çevrimdışı (Offline) olarak çalışacak şekilde tasarlanmıştır.

## 🐛 Known Issues & Troubleshooting (Bilinen Sorunlar)

Geliştirme süreci devam ettiği için bazı hatalar mevcut olabilir:

* **Boss Yapay Zekası:** Bazı boss'lar belirli durumlarda takılabilir veya beklenmedik davranışlar sergileyebilir.
* **Hitbox Hassasiyeti:** Bazı saldırıların hitbox'ları üzerinde iyileştirme çalışmaları devam etmektedir.

## 📄 License & Credits (Lisans ve Emeği Geçenler)

### Geliştirici Ekibi
Bu oyun aşağıda isimleri bulunan geliştirici ekip tarafından hazırlanmıştır:

* Semih İkbal
* Semih Öksüzoğlu
* Ahmet Berk Öz
* Veysel Kan
* Gürkan Dizoğlu
* Ahmet Özgür Korkmaz
* Berke Duru

### Varlıklar (Assets) ve Teşekkürler

**Grafik ve Çevre Varlıkları:**
Oyun içerisindeki 2D pixel art karakterler ve çevre tasarımları Itch.io üzerinden temin edilmiştir.
* [Itch.io Samurai Bundle Linki](https://itch.io/s/110075/samurai-bundle-2d-pixel-art)

[Samurai Asset Bundle Preview]
<img width="1842" height="914" alt="image" src="https://github.com/user-attachments/assets/de246666-e8df-43ce-bacb-87f086bfc950" />

**Kullanıcı Arayüzü (UI):**
Oyunun UI elementleri (Can barları, menü butonları vb.) **Gemini Banana Pro** kullanılarak oluşturulmuştur.
<img src= Assets/Sprites/HealthBars.png>
<img src= Assets/Sprites/DeathPanel_1.png>


<p align="center">
  <img src="docs/ui_hpbar.png" alt="UI Health Bar" width="300">
  <img src="docs/ui_menu_button.png" alt="UI Button" width="150">
</p>
