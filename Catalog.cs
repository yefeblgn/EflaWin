using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace Efla;

internal sealed partial class MainWindow
{
    private sealed class KurucuDurumu
    {
        internal bool Secili, Indiriliyor;
        internal string Dosya, Mesaj;
    }

    private sealed record KartDenetimi(Button Islem, CheckBox Secim, TextBlock Aciklama);

    private readonly IndirmeMotoru indirmeMotoru = new();
    private readonly Dictionary<Uygulama, KurucuDurumu> kurucular = new();
    private readonly Dictionary<Uygulama, KartDenetimi> gorunenKartlar = new();
    private Dictionary<Uygulama, KuruluUygulama> kurulu = new();
    private StackPanel katalogListe;
    private Button topluDugme;
    private TextBlock topluDurum;
    private CancellationTokenSource kuyruk;
    private bool kuruluTaraniyor;
    private string klasor, kuyrukMesaji;
    private string kategori = "__all__";

    private void IndirmeleriHazirla()
    {
        klasor = Path.Combine(oturum.Indirmeler, "Efla Win");
        foreach (var uygulama in Uygulama.Katalog)
            kurucular.Add(uygulama, new KurucuDurumu());
        foreach (var uygulama in Uygulama.Katalog.Take(Uygulama.OnerilenSayisi))
            kurucular[uygulama].Secili = true;
        KuruculariBul();
    }

    private void KataloguYap(bool onerilen)
    {
        Baslik(onerilen ? "Önerilenler" : "Tüm uygulamalar");
        if (!onerilen)
        {
            var filtre = new ComboBox { MinWidth = 185, Margin = new Thickness(0, 0, 0, 8) };
            var gruplar = Uygulama.Katalog.Select(u => u.Grup).Distinct().ToArray();
            filtre.ItemsSource = new[] { T("Tüm kategoriler") }.Concat(gruplar.Select(T)).ToArray();
            filtre.SelectedItem = kategori == "__all__" ? T("Tüm kategoriler") : T(kategori);
            AutomationProperties.SetName(filtre, T("Uygulama kategorisi"));
            filtre.SelectionChanged += (_, _) =>
            {
                if (filtre.SelectedItem is not string secili) return;
                kategori = secili == T("Tüm kategoriler") ? "__all__" : gruplar.FirstOrDefault(grup => T(grup) == secili) ?? "__all__";
                KataloguGuncelle();
            };
            liste.Children.Add(filtre);
        }
        katalogListe = new StackPanel { Spacing = 5 };
        liste.Children.Add(katalogListe);
        KataloguGuncelle();
    }

    private void KataloguGuncelle()
    {
        if (katalogListe == null) return;
        string sorgu = arama.Text.Trim();
        IEnumerable<Uygulama> uygulamalar = sayfa == PageId.Baslangic ? Uygulama.Katalog.Take(Uygulama.OnerilenSayisi) : Uygulama.Katalog;
        if (sayfa == PageId.Uygulamalar)
        {
            if (sorgu.Length > 0)
                uygulamalar = uygulamalar.Where(u => (u.Ad + " " + T(u.Kategori) + " " + T(u.Grup)).Contains(sorgu, StringComparison.CurrentCultureIgnoreCase));
            else if (kategori != "__all__") uygulamalar = uygulamalar.Where(u => u.Grup == kategori);
        }
        gorunenKartlar.Clear();
        katalogListe.Children.Clear();
        foreach (var uygulama in uygulamalar) katalogListe.Children.Add(UygulamaKarti(uygulama));
        if (katalogListe.Children.Count == 0) katalogListe.Children.Add(Metin("Eşleşen uygulama bulunamadı.", 14, true));
    }

    private Border UygulamaKarti(Uygulama uygulama)
    {
        var sec = new CheckBox { MinWidth = 30, Padding = new Thickness(0), VerticalAlignment = VerticalAlignment.Center };
        var islem = Dugme("İndir");
        islem.MinWidth = 86;
        islem.Click += async (_, _) => await UygulamaEylemi(uygulama);
        sec.Click += (_, _) => { kurucular[uygulama].Secili = sec.IsChecked == true; KatalogDurumunuGuncelle(); };
        AutomationProperties.SetName(sec, uygulama.Ad + T(" toplu indirme seçimi"));
        var kontroller = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 9 };
        kontroller.Children.Add(islem);
        var logo = new Image { Source = new BitmapImage(AssetUri(uygulama.Logo + ".png")), Width = 34, Height = 34, Stretch = Stretch.Uniform };
        var sol = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, VerticalAlignment = VerticalAlignment.Center };
        sol.Children.Add(sec);
        sol.Children.Add(logo);
        var kart = Satir(uygulama.Ad, uygulama.Kategori, out var aciklama, kontroller, sol);
        var normalArkaPlan = kart.Background;
        var imlecArkaPlani = Firca(karanlik ? (kontrast ? "#1A1A1A" : "#303030") : "#F7F7F7");
        kart.PointerEntered += (_, _) => kart.Background = imlecArkaPlani;
        kart.PointerExited += (_, _) => kart.Background = normalArkaPlan;
        var baglam = new MenuFlyout();
        var kaynak = new MenuFlyoutItem { Text = T("Kaynağı aç"), Icon = Simge("\uE8A7", 15) };
        kaynak.Click += (_, _) => Ac(uygulama.Kaynak);
        baglam.Items.Add(kaynak);
        kart.ContextFlyout = baglam;
        ToolTipService.SetToolTip(logo, uygulama.Ad);
        gorunenKartlar.Add(uygulama, new KartDenetimi(islem, sec, aciklama));
        KartiGuncelle(uygulama);
        return kart;
    }

    private Border KatalogAltCubugu()
    {
        var panel = new Grid { ColumnSpacing = 10 };
        panel.ColumnDefinitions.Add(new ColumnDefinition());
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var sol = new StackPanel { Spacing = 6 };
        topluDurum = Metin("", 12, true);
        sol.Children.Add(topluDurum);
        var klasorSecenekleri = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 7 };
        var klasorAc = Dugme("Klasörü aç", KlasoruAc);
        ToolTipService.SetToolTip(klasorAc, klasor);
        klasorSecenekleri.Children.Add(klasorAc);
        var degistir = Dugme("Değiştir");
        degistir.Click += async (_, _) =>
        {
            if (kuyruk != null) return;
            try
            {
                var sec = await new Microsoft.Windows.Storage.Pickers.FolderPicker(AppWindow.Id).PickSingleFolderAsync();
                if (sec == null || kuyruk != null) return;
                klasor = sec.Path;
                ToolTipService.SetToolTip(klasorAc, klasor);
                KuruculariBul();
                KatalogDurumunuGuncelle();
            }
            catch (Exception ex) { await Bildir("Klasör seçilemedi", ex.GetBaseException().Message); }
        };
        klasorSecenekleri.Children.Add(degistir);
        sol.Children.Add(klasorSecenekleri);
        panel.Children.Add(sol);
        topluDugme = Dugme("Seçilenleri indir", ana: true);
        topluDugme.VerticalAlignment = VerticalAlignment.Center;
        topluDugme.Click += async (_, _) =>
        {
            if (kuyruk != null) kuyruk.Cancel();
            else await Indir(Uygulama.Katalog.Where(u => kurucular[u].Secili && Indirilebilir(u)).ToArray());
        };
        Grid.SetColumn(topluDugme, 1); panel.Children.Add(topluDugme);
        KatalogDurumunuGuncelle();
        return new Border { Padding = new Thickness(30, 8, 30, 15), Child = panel };
    }

    internal void KlasoruAc() => _ = Eylem(() => { Directory.CreateDirectory(klasor); UygulamaYonetimi.GezginleAc(klasor); });

    private bool Indirilebilir(Uygulama u) => !kurulu.ContainsKey(u) && !File.Exists(kurucular[u].Dosya);

    private void KatalogDurumunuGuncelle()
    {
        foreach (var u in gorunenKartlar.Keys) KartiGuncelle(u);
        if (topluDugme == null) return;
        int secim = Uygulama.Katalog.Count(u => kurucular[u].Secili && Indirilebilir(u));
        topluDugme.Content = T(kuyruk == null ? "Seçilenleri indir" : "Durdur");
        topluDugme.IsEnabled = kuyruk != null || secim > 0;
        topluDurum.Text = kuyrukMesaji ?? $"{secim} {T("seçili")}";
    }

    private void KartiGuncelle(Uygulama u)
    {
        if (!gorunenKartlar.TryGetValue(u, out var kart)) return;
        var durum = kurucular[u];
        bool yuklu = kurulu.ContainsKey(u), hazir = File.Exists(durum.Dosya);
        kart.Islem.Content = T(yuklu ? "Kaldır" : hazir ? "Kur" : durum.Indiriliyor ? "İndiriliyor" : "İndir");
        kart.Islem.IsEnabled = !durum.Indiriliyor && (kuyruk == null || yuklu || hazir);
        kart.Secim.IsChecked = !yuklu && !hazir && durum.Secili;
        kart.Secim.IsEnabled = !yuklu && !hazir && kuyruk == null;
        kart.Secim.Opacity = yuklu || hazir ? 0 : 1;
        kart.Aciklama.Text = durum.Indiriliyor ? T(durum.Mesaj) : yuklu ? T(u.Kategori) + " · " + T("Kurulu") : hazir ? T(u.Kategori) + " · " + T("Kurucu hazır") : T(u.Kategori);
    }

    private void KuruculariBul()
    {
        foreach (var u in Uygulama.Katalog)
        {
            string dosya = Path.Combine(klasor, u.DosyaAdi);
            kurucular[u].Dosya = UygulamaYonetimi.YerelKurucu(dosya) ? dosya : null;
            kurucular[u].Mesaj = null;
        }
    }

    private async Task KurululariAlgila()
    {
        if (kuruluTaraniyor || kapaniyor) return;
        kuruluTaraniyor = true;
        try
        {
            kurulu = await Task.Run(() => UygulamaYonetimi.KuruluOlanlar(oturum));
            if (!kapaniyor) KatalogDurumunuGuncelle();
        }
        catch (UnauthorizedAccessException) { }
        catch (System.Security.SecurityException) { }
        catch (IOException) { }
        finally { kuruluTaraniyor = false; }
    }

    private async Task UygulamaEylemi(Uygulama u)
    {
        if (kurulu.TryGetValue(u, out var kayit))
        {
            var dialog = new ContentDialog { XamlRoot = kabuk.XamlRoot, Title = kayit.Ad, Content = T("Kaldırma işlemi açılsın mı?"), PrimaryButtonText = T("Kaldır"), CloseButtonText = T("Vazgeç"), DefaultButton = ContentDialogButton.Close };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary) await Eylem(() => UygulamaYonetimi.Kaldir(kayit));
        }
        else if (kurucular[u].Dosya is string dosya && UygulamaYonetimi.YerelKurucu(dosya))
            await Eylem(() => UygulamaYonetimi.GezginleAc(dosya));
        else if (kuyruk == null)
        {
            kurucular[u].Dosya = null;
            await Indir(new[] { u });
        }
    }

    private async Task Indir(Uygulama[] uygulamalar)
    {
        if (uygulamalar.Length == 0 || kuyruk != null) return;
        using var iptal = new CancellationTokenSource();
        kuyruk = iptal;
        var aktif = IslemBaslat(uygulamalar.Length > 1 ? "İndirme kuyruğu" : uygulamalar[0].Ad, iptal.Cancel);
        int tamam = 0, hatali = 0;
        try
        {
            foreach (var u in uygulamalar)
            {
                if (iptal.IsCancellationRequested) break;
                var durum = kurucular[u];
                durum.Indiriliyor = true;
                durum.Mesaj = "Bağlanıyor…";
                kuyrukMesaji = u.Ad + " " + T("indiriliyor");
                IslemIlerle(aktif, $"{u.Ad} · {tamam + hatali + 1}/{uygulamalar.Length} · Bağlanıyor…");
                KatalogDurumunuGuncelle();
                try
                {
                    var ilerleme = new Progress<string>(metin =>
                    {
                        if (!durum.Indiriliyor || kapaniyor) return;
                        durum.Mesaj = metin;
                        IslemIlerle(aktif, u.Ad + " · " + metin);
                        KartiGuncelle(u);
                    });
                    durum.Dosya = await indirmeMotoru.Indir(u, klasor, ilerleme, iptal.Token);
                    durum.Mesaj = null;
                    durum.Secili = false;
                    tamam++;
                }
                catch (OperationCanceledException) when (iptal.IsCancellationRequested) { durum.Mesaj = T("Durduruldu"); }
                catch (OperationCanceledException) { durum.Mesaj = T("Bağlantı zaman aşımına uğradı"); hatali++; }
                catch (Exception ex) { durum.Mesaj = T(ex.GetBaseException().Message); hatali++; }
                finally { durum.Indiriliyor = false; }
            }
        }
        finally
        {
            kuyruk = null;
            IslemBitti(aktif);
            kuyrukMesaji = (iptal.IsCancellationRequested ? T("Durduruldu") + " · " : "") + $"{tamam} {T("indirildi")}" + (hatali == 0 ? "" : $" · {hatali} {T("hata")}");
            if (!kapaniyor) KatalogDurumunuGuncelle();
        }
    }
}
