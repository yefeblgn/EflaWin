using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace Efla;

internal sealed partial class MainWindow : Window
{
    private enum PageId { Baslangic, Uygulamalar, Windows, Oyun, Donanim, Suruculer, Hafiza, Tercihler, Hakkinda }

    private const string TercihYolu = @"SOFTWARE\BLGN Studios\Efla Win";
    private readonly WindowsOturumu oturum = new();
    private readonly UISettings windowsGorunumu = new();
    private readonly Grid kabuk = new();
    private readonly Grid sayfaAlani = new();
    private readonly NavigationView menu = new();
    private readonly AutoSuggestBox arama = new() { QueryIcon = new SymbolIcon(Symbol.Find) };
    private readonly DispatcherQueueTimer sayac;
    private readonly Dictionary<PageId, NavigationViewItem> menuler = new();
    private readonly Dictionary<PageId, string> menuAdlari = new();
    private readonly List<(NavigationViewItemHeader Baslik, string Ad)> menuBasliklari = new();
    private readonly List<Action> ayarYenile = new();
    private Storyboard sayfaAnimasyonu;
    private StackPanel liste;
    private PageId sayfa;
    private bool karanlik, kontrast, kalin, hareketAz, kapaniyor;
    private int yaziYuzdesi = 100;
    private string tema = "Sistem";
    private Color? ozelVurgu = Renk("#25649A");
    private DonanimBilgisi donanim;
    private Tray tray;

    internal MainWindow()
    {
        Title = "Efla Win";
        AppWindow.Resize(new Windows.Graphics.SizeInt32(1088, 770));
        AppWindow.SetIcon(AssetPath("panel.ico"));
        ExtendsContentIntoTitleBar = true;
        TercihleriOku();
        IndirmeleriHazirla();

        kabuk.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        kabuk.RowDefinitions.Add(new RowDefinition());
        var baslikCubugu = new TitleBar
        {
            Title = "Efla Win",
            IconSource = new ImageIconSource { ImageSource = new BitmapImage(AssetUri("efla.png")) },
            Content = arama
        };
        arama.Width = 490;
        arama.HorizontalAlignment = HorizontalAlignment.Center;
        kabuk.SizeChanged += (_, e) => arama.Width = Math.Clamp(e.NewSize.Width - 470, 150, 495);
        AutomationProperties.SetName(arama, T("Katalogda uygulama ara"));
        arama.QuerySubmitted += (_, _) => { if (arama.Text.Trim().Length > 0) SayfaAc(PageId.Uygulamalar); };
        arama.TextChanged += (_, e) =>
        {
            if (e.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
            if (arama.Text.Trim().Length > 0 && sayfa != PageId.Uygulamalar) SayfaAc(PageId.Uygulamalar);
            else if (sayfa == PageId.Uygulamalar) KataloguGuncelle();
        };
        kabuk.Children.Add(baslikCubugu);

        menu.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
        menu.OpenPaneLength = 244;
        menu.IsPaneToggleButtonVisible = false;
        menu.IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed;
        menu.IsSettingsVisible = false;
        menu.AlwaysShowHeader = false;
        menu.ExpandedModeThresholdWidth = 0;
        menu.IsPaneOpen = true;
        menu.Content = sayfaAlani;
        MenuBasligi("Katalog");
        MenuEkle(PageId.Baslangic, "Önerilenler", "\uE80F");
        MenuEkle(PageId.Uygulamalar, "Tüm uygulamalar", "\uE71D");
        MenuBasligi("Bu bilgisayar");
        MenuEkle(PageId.Windows, "Windows ayarları", "\uE713");
        MenuEkle(PageId.Oyun, "Oyun ayarları", "\uE7FC");
        MenuEkle(PageId.Donanim, "Donanım", "\uE7F8");
        MenuEkle(PageId.Suruculer, "Sürücüler", "\uE950");
        MenuEkle(PageId.Hafiza, "Hafıza", "\uE964");
        IslemMenusunuKur();
        MenuEkle(PageId.Tercihler, "Uygulama ayarları", "\uE790", true);
        MenuEkle(PageId.Hakkinda, "Hakkında", "\uE946", true);
        menu.ItemInvoked += (_, e) =>
        {
            if (e.InvokedItemContainer == islemMenusu) { menu.SelectedItem = menuler[sayfa]; islemFlyout.ShowAt(islemMenusu); return; }
            if (e.InvokedItemContainer?.Tag is not PageId hedef) return;
            if (hedef != PageId.Uygulamalar) arama.Text = "";
            SayfaAc(hedef);
        };
        Grid.SetRow(menu, 1);
        kabuk.Children.Add(menu);
        Content = kabuk;
        SetTitleBar(baslikCubugu);
        arama.PlaceholderText = T("Uygulama ara");
        kabuk.KeyboardAcceleratorPlacementMode = KeyboardAcceleratorPlacementMode.Hidden;
        var araKisayolu = new KeyboardAccelerator { Key = global::Windows.System.VirtualKey.F, Modifiers = global::Windows.System.VirtualKeyModifiers.Control };
        araKisayolu.Invoked += (_, e) => { arama.Focus(FocusState.Programmatic); e.Handled = true; };
        kabuk.KeyboardAccelerators.Add(araKisayolu);
        TemayiUygula();
        SayfaAc(PageId.Baslangic, false);

        sayac = DispatcherQueue.CreateTimer();
        sayac.Interval = TimeSpan.FromSeconds(3);
        sayac.Tick += async (_, _) => { BellekGoster(); await KurululariAlgila(); };
        sayac.Start();
        kabuk.Loaded += KabukYuklendi;
        Activated += PencereEtkinlesti;
        windowsGorunumu.ColorValuesChanged += SistemRengiDegisti;
        Closed += PencereKapandi;
    }

    private async void KabukYuklendi(object sender, RoutedEventArgs e)
    {
        kabuk.Loaded -= KabukYuklendi;
        await KurululariAlgila();
        try { donanim = await Task.Run(DonanimBilgisi.Oku); }
        catch (Exception) { donanim = new DonanimBilgisi(); }
        if (!kapaniyor && sayfa is PageId.Donanim or PageId.Suruculer) SayfaAc(sayfa, false);
    }

    private async void PencereEtkinlesti(object sender, WindowActivatedEventArgs e)
    {
        if (kapaniyor || e.WindowActivationState == WindowActivationState.Deactivated) return;
        foreach (var yenile in ayarYenile) yenile();
        await KurululariAlgila();
    }

    private void PencereKapandi(object sender, WindowEventArgs e)
    {
        kapaniyor = true;
        sayac.Stop();
        windowsGorunumu.ColorValuesChanged -= SistemRengiDegisti;
        tray?.Dispose();
        kuyruk?.Cancel();
        surucuIptal?.Cancel();
        indirmeMotoru.Dispose();
    }

    internal void TepsiyiBagla() => tray = new Tray(this, AssetPath("panel.ico"));

    internal void TepsidenAc() => Activate();

    internal void TepsidenHafiza() { Activate(); SayfaAc(PageId.Hafiza); }

    private void MenuBasligi(string metin)
    {
        var baslik = new NavigationViewItemHeader { Content = T(metin) };
        menu.MenuItems.Add(baslik);
        menuBasliklari.Add((baslik, metin));
    }

    private void MenuEkle(PageId hedef, string ad, string glyph, bool alt = false)
    {
        var oge = new NavigationViewItem { Content = T(ad), Icon = Simge(glyph, 16), Tag = hedef };
        menuler.Add(hedef, oge);
        menuAdlari.Add(hedef, ad);
        if (alt) menu.FooterMenuItems.Add(oge);
        else menu.MenuItems.Add(oge);
    }

    private static string T(string metin) => Localization.Cevir(metin);

    private void DiliYenile()
    {
        foreach (var baslik in menuBasliklari) baslik.Baslik.Content = T(baslik.Ad);
        foreach (var ad in menuAdlari) menuler[ad.Key].Content = T(ad.Value);
        arama.PlaceholderText = T("Uygulama ara");
        AutomationProperties.SetName(arama, T("Katalogda uygulama ara"));
        tray?.DiliYenile();
        IslemleriGoster();
        SayfaAc(sayfa, false);
    }

    private void SayfaAc(PageId hedef, bool animasyon = true)
    {
        if (kapaniyor) return;
        sayfa = hedef;
        menu.SelectedItem = menuler[hedef];
        ayarYenile.Clear();
        acikAyar = null;
        bellekMetni = null;
        bellekCubugu = null;
        gorunenKartlar.Clear();
        katalogListe = null;
        sayfaAnimasyonu?.Stop();

        var govde = new Grid();
        govde.RowDefinitions.Add(new RowDefinition());
        govde.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        liste = new StackPanel { Spacing = 6, MaxWidth = 1000, Padding = new Thickness(30, 17, 30, 26) };
        var kaydir = new ScrollViewer { Content = liste, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        govde.Children.Add(kaydir);
        switch (hedef)
        {
            case PageId.Baslangic: KataloguYap(true); break;
            case PageId.Uygulamalar: KataloguYap(false); break;
            case PageId.Windows: WindowsuYap(); break;
            case PageId.Oyun: OyunuYap(); break;
            case PageId.Donanim: DonanimiYap(); break;
            case PageId.Suruculer: SuruculeriYap(); break;
            case PageId.Hafiza: HafizayiYap(); break;
            case PageId.Tercihler: UygulamaAyarlariniYap(); break;
            case PageId.Hakkinda: HakkindaYap(); break;
        }
        if (hedef is PageId.Baslangic or PageId.Uygulamalar or PageId.Suruculer)
        {
            var alt = KatalogAltCubugu();
            Grid.SetRow(alt, 1); govde.Children.Add(alt);
        }
        else { topluDugme = null; topluDurum = null; }
        sayfaAlani.Children.Clear();
        sayfaAlani.Children.Add(govde);
        if (animasyon && !hareketAz && windowsGorunumu.AnimationsEnabled) AnimasyonBaslat(govde);
    }

    private void AnimasyonBaslat(Grid govde)
    {
        var hareket = new TranslateTransform();
        govde.RenderTransform = hareket;
        var sure = new Duration(TimeSpan.FromMilliseconds(200));
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var solma = new DoubleAnimation { From = 0, To = 1, Duration = sure, EasingFunction = ease };
        var inis = new DoubleAnimation { From = -12, To = 0, Duration = sure, EasingFunction = ease, EnableDependentAnimation = true };
        Storyboard.SetTarget(solma, govde);
        Storyboard.SetTargetProperty(solma, "Opacity");
        Storyboard.SetTarget(inis, hareket);
        Storyboard.SetTargetProperty(inis, "Y");
        sayfaAnimasyonu = new Storyboard();
        sayfaAnimasyonu.Children.Add(solma);
        sayfaAnimasyonu.Children.Add(inis);
        sayfaAnimasyonu.Begin();
    }

    private void Baslik(string ad)
    {
        var baslik = Metin(ad, 29, agirlik: true);
        baslik.Margin = new Thickness(0, 1, 0, 17);
        AutomationProperties.SetHeadingLevel(baslik, AutomationHeadingLevel.Level1);
        liste.Children.Add(baslik);
    }

    private void Bolum(string ad)
    {
        var baslik = Metin(ad, 14, agirlik: true);
        baslik.Margin = new Thickness(0, 15, 0, 3);
        AutomationProperties.SetHeadingLevel(baslik, AutomationHeadingLevel.Level2);
        liste.Children.Add(baslik);
    }

    private TextBlock Metin(string metin, double boy = 14, bool ikincil = false, bool agirlik = false)
    {
        var oge = new TextBlock { Text = T(metin), FontSize = boy * yaziYuzdesi / 100.0, TextWrapping = TextWrapping.Wrap };
        if (ikincil) oge.Foreground = Firca(karanlik ? "#C5C5C5" : "#555555");
        if (agirlik || kalin) oge.FontWeight = kalin ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.SemiBold;
        return oge;
    }

    private static FontIcon Simge(string isaret, double boy = 18) => new() { Glyph = isaret, FontSize = boy };

    private static string AssetPath(string ad) => Path.Combine(AppContext.BaseDirectory, "Assets", ad);

    private static Uri AssetUri(string ad) => new(AssetPath(ad));

    private Brush KartRengi() => Firca(karanlik ? (kontrast ? "#080808" : "#2A2A2A") : "#FFFFFF");

    private static SolidColorBrush Firca(string hex) => new(Renk(hex));

    private static Color Renk(string hex) => Color.FromArgb(255, Convert.ToByte(hex.Substring(1, 2), 16), Convert.ToByte(hex.Substring(3, 2), 16), Convert.ToByte(hex.Substring(5, 2), 16));

    private Border Panel(UIElement ogeler) => new()
    {
        Background = KartRengi(), BorderBrush = Firca(karanlik ? (kontrast ? "#CCCCCC" : "#2B2B2B") : "#E7E7E7"),
        BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(15, 11, 13, 11), Child = ogeler
    };

    private Border Satir(string ad, string aciklama, FrameworkElement islem = null, FrameworkElement simge = null) =>
        Satir(ad, aciklama, out _, islem, simge);

    private Border Satir(string ad, string aciklama, out TextBlock ozet, FrameworkElement islem, FrameworkElement simge)
    {
        var satir = new Grid { MinHeight = 39, ColumnSpacing = 13 };
        satir.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        satir.ColumnDefinitions.Add(new ColumnDefinition());
        satir.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        if (simge != null)
        {
            simge.VerticalAlignment = VerticalAlignment.Center;
            satir.Children.Add(simge);
        }
        else satir.ColumnSpacing = 0;
        var metinler = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        metinler.Children.Add(Metin(ad));
        ozet = String.IsNullOrWhiteSpace(aciklama) ? null : Metin(aciklama, 12, true);
        if (ozet != null) metinler.Children.Add(ozet);
        Grid.SetColumn(metinler, 1); satir.Children.Add(metinler);
        if (islem != null)
        {
            islem.VerticalAlignment = VerticalAlignment.Center;
            islem.Margin = new Thickness(12, 0, 0, 0);
            Grid.SetColumn(islem, 2); satir.Children.Add(islem);
        }
        return Panel(satir);
    }

    private Button Dugme(string ad, Action islem = null, bool ana = false)
    {
        var dugme = new Button { Content = T(ad), MinHeight = 32, Padding = new Thickness(13, 5, 13, 6), FontSize = 13 * yaziYuzdesi / 100.0, UseSystemFocusVisuals = true };
        if (kalin) dugme.FontWeight = Microsoft.UI.Text.FontWeights.Bold;
        if (ana && Application.Current.Resources.TryGetValue("AccentButtonStyle", out var stil) && stil is Style style) dugme.Style = style;
        else
        {
            string renk = karanlik ? "#363636" : "#FFFFFF";
            dugme.Resources["ButtonBackground"] = Firca(renk);
            dugme.Resources["ButtonBackgroundPointerOver"] = Firca(karanlik ? "#404040" : "#F5F5F5");
            dugme.Resources["ButtonBackgroundPressed"] = Firca(karanlik ? "#303030" : "#F0F0F0");
            dugme.Resources["ButtonForegroundPointerOver"] = Firca(karanlik ? "#FFFFFF" : "#1B1B1B");
        }
        if (islem != null) dugme.Click += (_, _) => islem();
        return dugme;
    }

    private async Task<bool> Eylem(Action islem)
    {
        try { islem(); return true; }
        catch (Exception ex) { await Bildir("İşlem tamamlanamadı", ex.GetBaseException().Message); return false; }
    }

    private void Ac(string adres) => _ = Eylem(() => UygulamaYonetimi.GezginleAc(adres));

    private async Task Bildir(string baslik, string mesaj)
    {
        if (kapaniyor || kabuk.XamlRoot == null) return;
        await new ContentDialog { Title = T(baslik), Content = T(mesaj), CloseButtonText = T("Tamam"), XamlRoot = kabuk.XamlRoot, RequestedTheme = kabuk.RequestedTheme }.ShowAsync();
    }
}
