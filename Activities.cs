using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Efla;

internal sealed partial class MainWindow
{
    private sealed class AktifIslem(string ad, Action durdur)
    {
        internal readonly string Ad = ad;
        internal readonly Action Durdur = durdur;
        internal string Durum;
        internal bool Iptal;
        internal TextBlock GorunenDurum;
    }

    private readonly Flyout islemFlyout = new() { Placement = FlyoutPlacementMode.TopEdgeAlignedLeft };
    private readonly List<AktifIslem> aktifIslemler = new();
    private readonly NavigationViewItem islemMenusu = new() { Content = T("Mevcut İşlemler") };
    private readonly StackPanel islemListesi = new() { MinWidth = 265, MaxWidth = 335, Spacing = 9 };

    private void IslemMenusunuKur()
    {
        islemMenusu.Icon = Simge("\uE916", 16);
        menu.FooterMenuItems.Add(islemMenusu);
        var kaydir = new ScrollViewer { Content = islemListesi, MaxHeight = 330, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled };
        islemFlyout.Content = kaydir;
        islemFlyout.Opening += (_, _) => IslemleriGoster();
    }

    private AktifIslem IslemBaslat(string ad, Action durdur)
    {
        var islem = new AktifIslem(ad, durdur);
        aktifIslemler.Add(islem);
        IslemleriGoster();
        return islem;
    }

    private void IslemIlerle(AktifIslem islem, string durum)
    {
        if (islem.Iptal) return;
        islem.Durum = durum;
        if (islem.GorunenDurum != null) islem.GorunenDurum.Text = T(durum);
    }

    private void IslemBitti(AktifIslem islem)
    {
        aktifIslemler.Remove(islem);
        if (!kapaniyor) IslemleriGoster();
    }

    private void IslemleriGoster()
    {
        string etiket = T("Mevcut İşlemler") + (aktifIslemler.Count == 0 ? "" : $" · {aktifIslemler.Count}");
        islemMenusu.Content = etiket;
        islemListesi.Children.Clear();
        islemListesi.Children.Add(Metin("Mevcut İşlemler", 15, agirlik: true));
        if (aktifIslemler.Count == 0) islemListesi.Children.Add(Metin("Devam eden işlem yok.", 13, true));
        foreach (var islem in aktifIslemler)
        {
            var satir = new Grid { ColumnSpacing = 10 };
            satir.ColumnDefinitions.Add(new ColumnDefinition());
            satir.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var metinler = new StackPanel { Spacing = 3, VerticalAlignment = VerticalAlignment.Center };
            metinler.Children.Add(Metin(islem.Ad, 13, agirlik: true));
            islem.GorunenDurum = Metin(islem.Iptal ? "Durduruluyor…" : islem.Durum ?? "Başlatılıyor…", 12, true);
            metinler.Children.Add(islem.GorunenDurum);
            satir.Children.Add(metinler);
            var durdur = Dugme("", () => DurdurIslem(islem));
            durdur.Content = Simge("\uE711", 13);
            durdur.IsEnabled = !islem.Iptal;
            ToolTipService.SetToolTip(durdur, T("Durdur"));
            AutomationProperties.SetName(durdur, islem.Ad + T(" durdur"));
            Grid.SetColumn(durdur, 1); satir.Children.Add(durdur);
            islemListesi.Children.Add(satir);
        }
    }

    private void DurdurIslem(AktifIslem islem)
    {
        if (islem.Iptal) return;
        islem.Iptal = true;
        islem.Durdur();
        IslemleriGoster();
    }
}
