using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Efla;

internal sealed partial class MainWindow
{
    private TextBlock bellekMetni;
    private ProgressBar bellekCubugu;
    private string bellekSonucu, surucuSonucu;
    private bool bellekTemizleniyor, surucuTaraniyor, ayarOkunuyor;
    private CancellationTokenSource surucuIptal;
    private Button surucuTaraDugmesi;
    private TextBlock surucuTaraSonucu;
    private InfoBar acikAyar;

    private void WindowsuYap()
    {
        Baslik("Windows ayarları");
        Bolum("Hızlı erişim");
        AyarBaglantisi("Başlangıç uygulamaları", "Oturum açıldığında çalışan uygulamalar", "\uE74C", "ms-settings:startupapps");
        AyarBaglantisi("Depolama Algısı", "Geçici dosyalar ve boş alan", "\uEDA2", "ms-settings:storagesense");
        foreach (var grup in new WindowsAyarlari(oturum).Liste().GroupBy(a => a.Grup))
        {
            Bolum(grup.Key);
            foreach (var ayar in grup) AyarSatiri(ayar);
        }
        Bolum("Ağ");
        AyarBaglantisi("Ağ ve internet", "Bağdaştırıcılar ve bağlantı ayarları", "\uE774", "ms-settings:network");
        AyarBaglantisi("Teslim İyileştirme", "Windows Update’in indirme ve paylaşım ayarları", "\uE895", "ms-settings:delivery-optimization");
        AyarBaglantisi("Proxy", "Otomatik algılama ve el ile kurulum", "\uE968", "ms-settings:network-proxy");
        var sonuc = Metin("", 12, true);
        var temizle = Dugme("Temizle", () => sonuc.Text = T(Bellek.DnsFlushResolverCache() ? "DNS önbelleği temizlendi." : "DNS önbelleği temizlenemedi."));
        liste.Children.Add(Satir("DNS önbelleği", "Bu cihazdaki DNS çözümleme önbelleğini temizle", temizle, Simge("\uE895")));
        liste.Children.Add(sonuc);
    }

    private void OyunuYap()
    {
        Baslik("Oyun ayarları");
        var ayarlar = new WindowsAyarlari(oturum).Liste().ToArray();
        Bolum("Oyun ve kayıt");
        foreach (var ad in new[] { "Oyun Modu", "Arka planda oyun kaydı", "Kumandayla Xbox Game Bar’ı aç" }) AyarSatiri(ayarlar.First(a => a.Ad == ad));
        AyarBaglantisi("Xbox Game Bar", "Oyun çubuğu ve denetleyici kısayolu", "\uE7FC", "ms-settings:gaming-gamebar");
        AyarBaglantisi("Yakalamalar", "Klipler, kayıt kalitesi ve dosya konumu", "\uE722", "ms-settings:gaming-gamedvr");
        Bolum("Grafik ve güç");
        AyarBaglantisi("Grafik tercihleri", "Uygulama başına GPU ve grafik seçenekleri", "\uE7F4", "ms-settings:display-advancedgraphics");
        AyarSatiri(ayarlar.First(a => a.Ad == "Yüksek performans güç planı"));
    }

    private void AyarBaglantisi(string ad, string bilgi, string glyph, string uri)
    {
        var ac = Dugme("Aç", () => Ac(uri));
        AutomationProperties.SetName(ac, T(ad) + " " + T("ayarlarını aç"));
        liste.Children.Add(Satir(ad, bilgi, ac, Simge(glyph)));
    }

    private void AyarSatiri(WindowsAyari ayar)
    {
        var durum = Metin("", 12, true);
        durum.VerticalAlignment = VerticalAlignment.Center;
        var anahtar = new ToggleSwitch { OnContent = "", OffContent = "", MinWidth = 0, VerticalAlignment = VerticalAlignment.Center };
        AutomationProperties.SetName(anahtar, T(ayar.Ad));
        AutomationProperties.SetHelpText(anahtar, T(ayar.Aciklama));
        var detay = new InfoBar { Message = T(ayar.Aciklama), Severity = InfoBarSeverity.Informational, IsClosable = false, IsOpen = false };
        var bilgi = new Button { Content = Simge("\uE946", 16), Padding = new Thickness(6), MinHeight = 30, BorderThickness = new Thickness(0) };
        bilgi.Resources["ButtonBackgroundPointerOver"] = KartRengi();
        bilgi.Resources["ButtonBackground"] = KartRengi();
        ToolTipService.SetToolTip(bilgi, T(ayar.Aciklama));
        AutomationProperties.SetName(bilgi, T(ayar.Ad) + " " + T("açıklaması"));
        bilgi.Click += (_, _) =>
        {
            bool ac = !detay.IsOpen;
            if (acikAyar != null) acikAyar.IsOpen = false;
            detay.IsOpen = ac;
            acikAyar = ac ? detay : null;
        };
        var kontroller = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 9 };
        kontroller.Children.Add(durum);
        kontroller.Children.Add(anahtar);
        kontroller.Children.Add(bilgi);
        string glyph = ayar.Grup switch { "Gizlilik" => "\uE72E", "Performans" => "\uEC4A", "Görünüm" => "\uE790", "Dosya Gezgini" => "\uE838", "Klavye ve fare" => "\uE765", _ => "\uE721" };
        liste.Children.Add(Satir(ayar.Ad, null, kontroller, Simge(glyph)));
        liste.Children.Add(detay);

        void Yenile()
        {
            ayarOkunuyor = true;
            try
            {
                var ilke = ayar.Ilke?.Invoke();
                bool? deger = ayar.Destekli ? ilke?.Acik ?? ayar.Oku() : null;
                anahtar.IsOn = deger == true;
                anahtar.IsEnabled = ayar.Destekli && ilke == null;
                durum.Text = !ayar.Destekli ? T("Kullanılamıyor") : deger == null ? T("Belirlenemedi") : T(deger.Value ? "Açık" : "Kapalı") + (ilke == null ? "" : " · " + T("Kilitli"));
                detay.Message = ilke == null ? T(ayar.Aciklama) : T(ayar.Aciklama) + "\n\n" + T("Windows’un uyguladığı kural kullanıcı ayarına öncelik veriyor.") + "\n" + ilke.Kaynak;
                ToolTipService.SetToolTip(bilgi, ilke == null ? T(ayar.Aciklama) : T("Kilidin kaynağını görmek için aç."));
            }
            catch (Exception) { anahtar.IsEnabled = false; durum.Text = T("Okunamadı"); }
            finally { ayarOkunuyor = false; }
        }

        anahtar.Toggled += async (_, _) =>
        {
            if (ayarOkunuyor || !anahtar.IsEnabled) return;
            bool ac = anahtar.IsOn;
            await Eylem(() => { if (ayar.Ilke?.Invoke() == null) ayar.Yaz(ac); });
            Yenile();
        };
        ayarYenile.Add(Yenile);
        Yenile();
    }

    private void DonanimiYap()
    {
        Baslik("Donanım");
        liste.Children.Add(Satir("İşlemci", donanim?.Islemci ?? "Algılanıyor…", simge: Simge("\uE950")));
        liste.Children.Add(Satir("Ekran kartı", donanim?.EkranKarti ?? "Algılanıyor…", simge: Simge("\uE7F4")));
        liste.Children.Add(Satir("İşletim sistemi", oturum.Surum + " · 64 bit", simge: Simge("\uE770")));
        Bolum("Bellek");
        liste.Children.Add(Panel(BellekBilgisi()));
        Bolum("Depolama");
        foreach (string disk in donanim?.Diskler ?? new[] { "Diskler okunuyor…" })
        {
            var ayir = disk.Split("   ", 2, StringSplitOptions.None);
            liste.Children.Add(Satir(ayir[0], ayir.Length == 2 ? ayir[1] : null, simge: Simge("\uEDA2")));
        }
    }

    private StackPanel BellekBilgisi()
    {
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(Metin("Fiziksel RAM", agirlik: true));
        bellekMetni = Metin("", 13, true);
        panel.Children.Add(bellekMetni);
        bellekCubugu = new ProgressBar { Maximum = 100 };
        AutomationProperties.SetName(bellekCubugu, T("RAM kullanım oranı"));
        panel.Children.Add(bellekCubugu);
        BellekGoster();
        return panel;
    }

    private void HafizayiYap()
    {
        Baslik("Hafıza");
        liste.Children.Add(Panel(BellekBilgisi()));
        var temizle = Dugme(bellekTemizleniyor ? "Temizleniyor…" : "Temizle");
        temizle.IsEnabled = !bellekTemizleniyor;
        var durum = Metin(bellekSonucu ?? "", 12, true);
        temizle.Click += async (_, _) =>
        {
            if (bellekTemizleniyor) return;
            bellekTemizleniyor = true;
            temizle.IsEnabled = false;
            temizle.Content = "Temizleniyor…";
            try
            {
                long once = (long)Bellek.Oku().Bos;
                int sayi = await Task.Run(() => Bellek.Temizle(oturum.Sid));
                long fark = Math.Max(0, (long)Bellek.Oku().Bos - once);
                bellekSonucu = sayi == 0 ? T("Temizlenebilecek işlem bulunamadı.") : $"{T("Temizlenen işlem sayısı")}: {sayi} · {(fark / 1048576.0):0} MB {T("anlık fark")}";
            }
            catch (Exception ex) { bellekSonucu = ex.GetBaseException().Message; }
            finally
            {
                bellekTemizleniyor = false;
                temizle.IsEnabled = true;
                temizle.Content = "Temizle";
                durum.Text = bellekSonucu;
                BellekGoster();
            }
        };
        liste.Children.Add(Satir("Hızlı hafıza temizliği", "Bu oturumdaki uygun uygulamaların çalışma kümelerini azaltır.", temizle, Simge("\uE964")));
        liste.Children.Add(durum);
    }

    private void BellekGoster()
    {
        if (bellekMetni == null || bellekCubugu == null) return;
        try
        {
            var durum = Bellek.Oku();
            bellekCubugu.Value = durum.Yuk;
            bellekMetni.Text = $"{DonanimBilgisi.Gb((long)(durum.Toplam - durum.Bos))} {T("dolu")} · {DonanimBilgisi.Gb((long)durum.Bos)} {T("boş")} · {DonanimBilgisi.Gb((long)durum.Toplam)} {T("toplam")}";
        }
        catch (Exception) { bellekMetni.Text = T("RAM bilgisi okunamadı."); }
    }

    private void SuruculeriYap()
    {
        Baslik("Sürücüler");
        string[] oneriler = donanim == null ? Array.Empty<string>() : new[] { donanim.Intel ? "intel" : "", donanim.Amd ? "amd" : "", donanim.Nvidia ? "nvidia" : "" };
        Bolum("Üretici araçları");
        var adaylar = Uygulama.Katalog.Where(u => u.Grup == "Sürücüler").OrderByDescending(u => oneriler.Contains(u.Logo));
        foreach (var u in adaylar)
        {
            var kart = UygulamaKarti(u);
            if (oneriler.Contains(u.Logo)) AutomationProperties.SetHelpText(kart, T("Bu bilgisayar için önerilir"));
            liste.Children.Add(kart);
        }
        Bolum("Windows Update");
        var tara = Dugme(surucuTaraniyor ? "Taranıyor…" : "Tara");
        tara.IsEnabled = !surucuTaraniyor;
        var bilgi = Metin(surucuSonucu ?? "", 12, true);
        surucuTaraDugmesi = tara;
        surucuTaraSonucu = bilgi;
        tara.Click += async (_, _) =>
        {
            if (surucuTaraniyor) return;
            surucuTaraniyor = true;
            using var iptal = new CancellationTokenSource();
            surucuIptal = iptal;
            var aktif = IslemBaslat("Windows Update · Sürücü taraması", iptal.Cancel);
            IslemIlerle(aktif, "Windows Update sürücüleri denetleniyor…");
            tara.Content = "Taranıyor…";
            tara.IsEnabled = false;
            try
            {
                string[] bulunan = await Task.Run(() => SurucuGuncellemeleri.WindowsUpdateTara(iptal.Token));
                surucuSonucu = bulunan.Length == 0 ? "Windows Update sürücü güncellemesi sunmadı." : String.Join("\n", bulunan);
            }
            catch (OperationCanceledException) when (iptal.IsCancellationRequested) { surucuSonucu = T("Sürücü taraması durduruldu."); }
            catch (Exception ex) { surucuSonucu = T(ex.GetBaseException().Message); }
            finally
            {
                surucuTaraniyor = false;
                surucuIptal = null;
                IslemBitti(aktif);
                if (!kapaniyor && sayfa == PageId.Suruculer)
                {
                    surucuTaraDugmesi.Content = "Tara";
                    surucuTaraDugmesi.IsEnabled = true;
                    surucuTaraSonucu.Text = surucuSonucu;
                }
            }
        };
        liste.Children.Add(Satir("Sürücü güncellemeleri", "Bu bilgisayara sunulan sürücüleri denetle", tara, Simge("\uE895")));
        liste.Children.Add(bilgi);
        AyarBaglantisi("İsteğe bağlı güncellemeler", "Güncellemeleri Windows’ta incele ve uygula", "\uE896", "ms-settings:windowsupdate-optionalupdates");
        if (donanim?.Suruculer.Count > 0)
        {
            Bolum("Kurulu sürücüler");
            foreach (string surucu in donanim.Suruculer.Distinct()) liste.Children.Add(Satir(surucu, null, simge: Simge("\uE950")));
        }
    }

    private void TercihleriOku()
    {
        Localization.Sec(oturum.Oku(true, TercihYolu, "Dil") as string ?? "auto");
        try
        {
            string secili = oturum.Oku(true, TercihYolu, "Tema") as string;
            if (secili is "Açık" or "Koyu") tema = secili;
            if (oturum.Oku(true, TercihYolu, "Vurgu") is string kod)
            {
                if (kod.Length == 0) ozelVurgu = null;
                else if (kod.Length == 7 && kod[0] == '#') ozelVurgu = Renk(kod);
            }
            if (oturum.Oku(true, TercihYolu, "YaziYuzdesi") is int boyut) yaziYuzdesi = Math.Clamp(boyut, 90, 135);
            kontrast = Equals(oturum.Oku(true, TercihYolu, "YuksekKontrast"), 1);
            kalin = Equals(oturum.Oku(true, TercihYolu, "KalinYazi"), 1);
            hareketAz = Equals(oturum.Oku(true, TercihYolu, "HareketAz"), 1);
        }
        catch (UnauthorizedAccessException) { }
        catch (System.Security.SecurityException) { }
        catch (FormatException) { }
    }

    private void TemayiUygula()
    {
        karanlik = tema == "Koyu" || (tema == "Sistem" && windowsGorunumu.GetColorValue(UIColorType.Background).R < 128);
        kabuk.RequestedTheme = karanlik ? ElementTheme.Dark : ElementTheme.Light;
        kabuk.Background = Firca(karanlik ? (kontrast ? "#000000" : "#202020") : "#F4F4F4");
        Color renk = ozelVurgu ?? windowsGorunumu.GetColorValue(UIColorType.Accent);
        AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        AppWindow.TitleBar.ButtonForegroundColor = karanlik ? Colors.White : Colors.Black;
        AppWindow.TitleBar.ButtonInactiveForegroundColor = karanlik ? Renk("#C5C5C5") : Renk("#555555");
        if (ozelVurgu.HasValue)
        {
            kabuk.Resources["SystemAccentColor"] = renk;
            kabuk.Resources["SystemAccentColorLight1"] = Karistir(renk, Colors.White, .30);
            kabuk.Resources["SystemAccentColorLight2"] = Karistir(renk, Colors.White, .46);
            kabuk.Resources["SystemAccentColorLight3"] = Karistir(renk, Colors.White, .60);
            kabuk.Resources["SystemAccentColorDark1"] = Karistir(renk, Colors.Black, .25);
            kabuk.Resources["SystemAccentColorDark2"] = Karistir(renk, Colors.Black, .40);
            kabuk.Resources["SystemAccentColorDark3"] = Karistir(renk, Colors.Black, .55);
            kabuk.Resources["AccentFillColorDefaultBrush"] = new SolidColorBrush(karanlik ? Karistir(renk, Colors.White, .36) : Karistir(renk, Colors.Black, .25));
        }
        else
        {
            foreach (string anahtar in new[] { "SystemAccentColor", "SystemAccentColorLight1", "SystemAccentColorLight2", "SystemAccentColorLight3", "SystemAccentColorDark1", "SystemAccentColorDark2", "SystemAccentColorDark3", "AccentFillColorDefaultBrush" })
                kabuk.Resources.Remove(anahtar);
        }
    }

    private static Color Karistir(Color asil, Color hedef, double oran) => Color.FromArgb(255,
        (byte)(asil.R + (hedef.R - asil.R) * oran), (byte)(asil.G + (hedef.G - asil.G) * oran), (byte)(asil.B + (hedef.B - asil.B) * oran));

    private void SistemRengiDegisti(UISettings sender, object args) => DispatcherQueue.TryEnqueue(() =>
    {
        if (kapaniyor || tema != "Sistem") return;
        TemayiUygula();
        SayfaAc(sayfa, false);
    });

    private void AyariKaydet(string ad, object deger) => _ = Eylem(() => oturum.Yaz(true, TercihYolu, ad, deger, false));

    private void UygulamaAyarlariniYap()
    {
        Baslik("Uygulama ayarları");
        Bolum("Dil");
        string[] kodlar = { "auto", "tr", "en", "es", "ru", "de", "zh", "ja" };
        string[] diller = { T("Windows varsayılanı"), "Türkçe", "English", "Español", "Русский", "Deutsch", "简体中文", "日本語" };
        var dilSec = new ComboBox { ItemsSource = diller, SelectedIndex = Array.IndexOf(kodlar, Localization.Tercih), MinWidth = 170 };
        AutomationProperties.SetName(dilSec, T("Uygulama dili"));
        dilSec.SelectionChanged += (_, _) =>
        {
            if (dilSec.SelectedIndex < 0 || kodlar[dilSec.SelectedIndex] == Localization.Tercih) return;
            Localization.Sec(kodlar[dilSec.SelectedIndex]);
            AyariKaydet("Dil", Localization.Tercih);
            DiliYenile();
        };
        liste.Children.Add(Satir("Uygulama dili", "Windows varsayılanı veya uygulama dili", dilSec, Simge("\uE774")));
        Bolum("Görünüm");
        string[] temaKodlari = { "Sistem", "Açık", "Koyu" };
        var temaEtiketleri = temaKodlari.Select(k => k == "Açık" ? T("Açık tema") : T(k)).ToArray();
        var temalar = new ComboBox { ItemsSource = temaEtiketleri, SelectedIndex = Array.IndexOf(temaKodlari, tema), MinWidth = 118 };
        AutomationProperties.SetName(temalar, T("Tema"));
        temalar.SelectionChanged += (_, _) =>
        {
            if (temalar.SelectedIndex < 0 || tema == temaKodlari[temalar.SelectedIndex]) return;
            tema = temaKodlari[temalar.SelectedIndex];
            AyariKaydet("Tema", tema);
            TemayiUygula();
            SayfaAc(PageId.Tercihler, false);
        };
        liste.Children.Add(Satir("Tema", "Sistem, açık veya koyu renk düzeni", temalar, Simge("\uE790")));
        var renkSec = Dugme("Renk seç");
        renkSec.Click += async (_, _) =>
        {
            var secenek = new ColorPicker { Color = ozelVurgu ?? windowsGorunumu.GetColorValue(UIColorType.Accent), IsAlphaEnabled = false };
            var pencere = new ContentDialog { XamlRoot = kabuk.XamlRoot, Title = T("Vurgu rengi"), Content = secenek, PrimaryButtonText = T("Uygula"), SecondaryButtonText = T("Sistem rengini kullan"), CloseButtonText = T("Vazgeç") };
            var karar = await pencere.ShowAsync();
            if (karar == ContentDialogResult.None) return;
            ozelVurgu = karar == ContentDialogResult.Primary ? secenek.Color : null;
            AyariKaydet("Vurgu", ozelVurgu.HasValue ? $"#{secenek.Color.R:X2}{secenek.Color.G:X2}{secenek.Color.B:X2}" : "");
            TemayiUygula();
            SayfaAc(PageId.Tercihler, false);
        };
        liste.Children.Add(Satir("Vurgu rengi", ozelVurgu.HasValue ? "Özel renk" : "Windows sistem rengi", renkSec, Simge("\uE791")));
        Bolum("Erişilebilirlik");
        var boyut = new ComboBox { ItemsSource = new[] { "%90", "%95", "%100", "%105", "%110", "%115", "%120", "%125", "%130", "%135" }, SelectedItem = "%" + yaziYuzdesi, MinWidth = 92 };
        AutomationProperties.SetName(boyut, T("Yazı boyutu"));
        boyut.SelectionChanged += (_, _) =>
        {
            if (boyut.SelectedItem is not string secili) return;
            yaziYuzdesi = Int32.Parse(secili.AsSpan(1), CultureInfo.InvariantCulture);
            AyariKaydet("YaziYuzdesi", yaziYuzdesi);
            SayfaAc(PageId.Tercihler, false);
        };
        liste.Children.Add(Satir("Yazı boyutu", null, boyut, Simge("\uE8E9")));
        TercihAnahtari("Yüksek kontrast", "Metin ve yüzeyleri daha belirgin göster", "\uE7F4", kontrast, "YuksekKontrast", acik => kontrast = acik);
        TercihAnahtari("Kalın yazı", "Uygulama metinlerinin ağırlığını artır", "\uE8DD", kalin, "KalinYazi", acik => kalin = acik);
        TercihAnahtari("Hareketi azalt", "Sayfa geçiş animasyonlarını kapat", "\uE7C8", hareketAz, "HareketAz", acik => hareketAz = acik);
    }

    private void TercihAnahtari(string ad, string bilgi, string glyph, bool deger, string anahtar, Action<bool> uygula)
    {
        var sec = new ToggleSwitch { IsOn = deger, OnContent = T("Açık"), OffContent = T("Kapalı"), MinWidth = 100 };
        AutomationProperties.SetName(sec, T(ad));
        sec.Toggled += (_, _) => { uygula(sec.IsOn); AyariKaydet(anahtar, sec.IsOn ? 1 : 0); TemayiUygula(); SayfaAc(PageId.Tercihler, false); };
        liste.Children.Add(Satir(ad, bilgi, sec, Simge(glyph)));
    }

    private void HakkindaYap()
    {
        Baslik("Hakkında");
        var metin = new StackPanel { Spacing = 7 };
        metin.Children.Add(Metin("Efla Win 1.0.0", 23, agirlik: true));
        metin.Children.Add(Metin("made by ❤️ yefeblgn"));
        metin.Children.Add(Metin("Powered by BLGN Studios", 13, true));
        var baglanti = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7, Margin = new Thickness(0, 5, 0, 1) };
        baglanti.Children.Add(Dugme("Web", () => Ac("https://yefeblgn.net")));
        baglanti.Children.Add(Dugme("GitHub", () => Ac("https://github.com/yefeblgn/eflawin")));
        metin.Children.Add(baglanti);
        var kimlik = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 15 };
        kimlik.Children.Add(new Image
        {
            Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(AssetUri("efla.png")),
            Width = 52, Height = 52, VerticalAlignment = VerticalAlignment.Top
        });
        kimlik.Children.Add(metin);
        liste.Children.Add(Panel(kimlik));
    }
}
