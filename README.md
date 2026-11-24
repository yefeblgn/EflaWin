# Efla Win

**Diller / Languages:** [TR](README.md) · [EN](README.en.md) · [ES](README.es.md) · [RU](README.ru.md) · [DE](README.de.md) · [中文](README.zh.md) · [日本語](README.ja.md)

Efla Win, yeni bir Windows bilgisayarı kurarken uygulama indirme, sürücü denetimi ve sık kullanılan sistem ayarlarını tek pencerede toplar. WinUI 3 ile geliştirilmiştir.

<img src="Assets/efla.png" alt="Efla Win logosu" width="72" height="72">

## Başlangıç

`Efla Win 1.0.0.exe` dosyasını 64 bit Windows 11’de aç ve yönetici onayını ver. Kurulum paketi gerekmez. .NET ve Windows App SDK bileşenleri EXE’ye dahildir ve ilk çalıştırmada yerel önbelleğe açılır. İndirmeler ve Windows Update taraması internet bağlantısı gerektirir.

### Uygulamaları indir ve kur

1. **Önerilenler** ile başla veya **Tüm uygulamalar** ekranında kategoriyi seç. **Ctrl+F** arama kutusuna odaklanır.
2. Tek bir uygulama için **İndir** düğmesini kullan. Birkaç tanesini sıraya almak için logo yanındaki kutuları işaretleyip **Seçilenleri indir** düğmesine bas.
3. İndirilen dosyalar varsayılan olarak Windows’un İndirilenler klasöründeki `Efla Win` dizinine yazılır. Alt çubuktaki **Değiştir** ile başka bir klasör seçebilir, **Klasörü aç** ile dosyaları görebilirsin.
4. İndirme tamamlanınca **Kur** düğmesi kurucuyu açar. Windows uygulamayı kurulu olarak bildirdiğinde **Kaldır** düğmesi görünür ve onayından sonra uygulamanın kaldırma aracı açılır.

Bir kartın sağ tık menüsündeki **Kaynağı aç** seçeneği, katalogda yer alan dağıtım sayfasını açar.

### İndirmeleri ve taramayı takip et

Sol menüdeki **Mevcut İşlemler**, sayfa değiştirsen de indirme kuyruğunu ve sürücü taramasını gösterir. İndirme sırasında alınan veri miktarı görünür. Sunucu toplam boyutu bildirirse o da gösterilir. İşlemin yanındaki **×** düğmesi o işi durdurur.

Pencere küçültüldüğünde bildirim alanına gider. Efla Win simgesinin sağ tık menüsünden pencereyi, indirme klasörünü veya hafıza ekranını açabilirsin. **Çıkış** uygulamayı kapatır.

### Windows’u düzenle

| Ekran | Neler yapabilirsin |
| --- | --- |
| **Windows ayarları** | Arama, görev çubuğu, gizlilik, performans ve Dosya Gezgini seçeneklerini kontrol et. Başlangıç uygulamaları, depolama, ağ ve proxy sayfalarına geç. |
| **Oyun ayarları** | Oyun Modu, arka planda kayıt ve Xbox Game Bar seçeneklerini denetle. Grafik tercihlerini ve güç planını aç. |
| **Donanım** | İşlemciyi, ekran kartını, Windows sürümünü, RAM kullanımını ve disklerin boş alanını gör. |
| **Sürücüler** | Intel, AMD ve NVIDIA araçlarını cihazına göre öne çıkar. Windows Update’te sunulan sürücüleri tara ve kurulum için **İsteğe bağlı güncellemeler** ekranını aç. |
| **Hafıza** | Anlık RAM kullanımını gör. **Temizle** ile bu oturumdaki uygun uygulamaların çalışma kümelerini küçült. |
| **Uygulama ayarları** | Sistem, açık veya koyu temayı, vurgu rengini, yazı boyutunu, kalın yazıyı, kontrastı ve hareket azaltmayı ayarla. |

Bir ayarın yanında **Kilitli** yazıyorsa, bilgi düğmesi Windows’un uyguladığı ilkeyi ve kayıt yolunu gösterir. **Kullanılamıyor**, ilgili seçeneğin bu Windows sürümünde veya bilgisayarda desteklenmediğini gösterir. Bazı ayarlar ancak yeniden oturum açıldığında yansır.

## Kaynaktan derle

Windows’ta .NET 10 SDK ve Windows SDK kurulu olmalı. Kaynak arşivini açıp `EflaWin.csproj` dosyasının bulunduğu klasörde PowerShell’i çalıştır.

```powershell
dotnet publish .\EflaWin.csproj -c Release -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -p:EnableCompressionInSingleFile=true
```

Yayın çıktısı `bin\Release\net10.0-windows10.0.19041.0\win-x64\publish\EflaWin.exe` yolundadır. Dağıtım için bu EXE yeterlidir.

## Geri bildirim

Hata bildirirken Windows sürümünü, hatanın görüldüğü ekranı ve tekrarlama adımlarını ekle. İndirme sorunuysa uygulama adını ve kartta görünen hata metnini de belirt. [GitHub](https://github.com/yefeblgn/eflawin)

## Kaynaklar ve lisans

Windows seçenekleri hazırlanırken [Optimizer](https://github.com/hellzerg/optimizer), [Sparkle](https://github.com/thedogecraft/sparkle), [ReviOS Playbook](https://github.com/meetrevision/playbook), [GTweak](https://github.com/Greedeks/GTweak) ve [vacpp/windows11](https://github.com/vacpp/windows11) incelendi. Bildirim alanı menüsünün arayüzünde [TranslucentTB](https://github.com/TranslucentTB/TranslucentTB) örnek alındı.

© 2026 BLGN Studios. Kullanım koşulları [LICENSE](LICENSE) dosyasındadır. Katalogdaki markalar ve uygulamalar kendi sahiplerine aittir.

made by ❤️ [yefeblgn](https://yefeblgn.net)  
Powered by BLGN Studios
