using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

internal sealed class WindowsOturumu
{
    internal readonly string Sid;
    internal readonly bool AyniHesap;
    internal readonly bool KullaniciKaydiVar;
    internal readonly int Derleme;
    internal readonly string Surum;
    internal readonly string Indirmeler;
    internal static readonly RegistryView Gorunum = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;

    internal WindowsOturumu()
    {
        using (var kimlik = WindowsIdentity.GetCurrent())
        {
            string aktif = MasaustuKimligi();
            Sid = aktif ?? kimlik.User.Value;
            AyniHesap = Sid == kimlik.User.Value;
            using (var anahtar = Registry.Users.OpenSubKey(Sid)) KullaniciKaydiVar = anahtar != null;
        }
        const string yol = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        int.TryParse(Convert.ToString(Oku(false, yol, "CurrentBuildNumber")), out Derleme);
        string gorunen = Convert.ToString(Oku(false, yol, "DisplayVersion"));
        Surum = (Derleme >= 22000 ? "Windows 11" : Derleme >= 10240 ? "Windows 10" : "Windows") +
            (String.IsNullOrEmpty(gorunen) ? "" : " · " + gorunen) + " · " + Derleme;
        Indirmeler = IndirmeKlasoru();
    }

    internal object Oku(bool kullanici, string yol, string ad)
    {
        if (kullanici && !KullaniciKaydiVar) return null;
        string tamYol = kullanici ? Sid + "\\" + yol : yol;
        using (var kok = RegistryKey.OpenBaseKey(kullanici ? RegistryHive.Users : RegistryHive.LocalMachine, Gorunum))
        using (var anahtar = kok.OpenSubKey(tamYol))
            return anahtar == null ? null : anahtar.GetValue(ad, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
    }

    internal void Yaz(bool kullanici, string yol, string ad, object deger, bool bildir = true)
    {
        if (kullanici && !KullaniciKaydiVar) throw new InvalidOperationException("Windows kullanıcı profili kullanılamıyor.");
        string tamYol = kullanici ? Sid + "\\" + yol : yol;
        using (var kok = RegistryKey.OpenBaseKey(kullanici ? RegistryHive.Users : RegistryHive.LocalMachine, Gorunum))
        using (var anahtar = kok.CreateSubKey(tamYol))
            anahtar.SetValue(ad, deger, deger is string ? RegistryValueKind.String : RegistryValueKind.DWord);
        if (bildir)
        {
            IntPtr sonuc;
            SendMessageTimeout(new IntPtr(0xffff), 0x001a, IntPtr.Zero, yol, 2, 100, out sonuc);
        }
    }

    private string IndirmeKlasoru()
    {
        Guid indirmeler = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        IntPtr metin;
        if (AyniHesap && SHGetKnownFolderPath(ref indirmeler, 0, IntPtr.Zero, out metin) == 0)
        {
            try { return Marshal.PtrToStringUni(metin); }
            finally { Marshal.FreeCoTaskMem(metin); }
        }
        string profil = Convert.ToString(Oku(false, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList\" + Sid, "ProfileImagePath"));
        profil = Environment.ExpandEnvironmentVariables(profil);
        if (String.IsNullOrWhiteSpace(profil)) profil = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string ozel = Convert.ToString(Oku(true, @"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", "{" + indirmeler.ToString().ToUpperInvariant() + "}"));
        return String.IsNullOrWhiteSpace(ozel) ? Path.Combine(profil, "Downloads") :
            Environment.ExpandEnvironmentVariables(ozel.Replace("%USERPROFILE%", profil));
    }

    private static string MasaustuKimligi()
    {
        uint pid;
        GetWindowThreadProcessId(GetShellWindow(), out pid);
        if (pid == 0) return null;
        IntPtr surec = OpenProcess(0x1000, false, pid);
        if (surec == IntPtr.Zero) return null;
        try
        {
            IntPtr token;
            if (!OpenProcessToken(surec, 8, out token)) return null;
            try { using (var kimlik = new WindowsIdentity(token)) return kimlik.User.Value; }
            finally { CloseHandle(token); }
        }
        finally { CloseHandle(surec); }
    }

    [DllImport("user32.dll")] private static extern IntPtr GetShellWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    [DllImport("kernel32.dll")] private static extern IntPtr OpenProcess(uint access, bool inherit, uint processId);
    [DllImport("advapi32.dll")] private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);
    [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);
    [DllImport("shell32.dll")] private static extern int SHGetKnownFolderPath(ref Guid id, uint flags, IntPtr token, out IntPtr path);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern IntPtr SendMessageTimeout(IntPtr window, uint message, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);
}

internal sealed record IlkeDurumu(bool Acik, string Kaynak);

internal sealed class WindowsAyari
{
    internal readonly string Ad, Aciklama, Grup;
    internal readonly Func<bool?> Oku;
    internal readonly Func<IlkeDurumu> Ilke;
    internal readonly Action<bool> Yaz;
    internal readonly bool Destekli;

    internal WindowsAyari(string grup, string ad, string aciklama, bool destekli, Func<bool?> oku, Action<bool> yaz, Func<IlkeDurumu> ilke)
    {
        Grup = grup; Ad = ad; Aciklama = aciklama; Destekli = destekli; Oku = oku; Yaz = yaz; Ilke = ilke;
    }
}

internal sealed class WindowsAyarlari
{
    private const string Cv = @"SOFTWARE\Microsoft\Windows\CurrentVersion\";
    private const string Ilke = @"SOFTWARE\Policies\Microsoft\Windows\";
    private const string Mdm = @"SOFTWARE\Microsoft\PolicyManager\current\device\";
    private const string Advanced = Cv + @"Explorer\Advanced";
    private readonly WindowsOturumu oturum;
    internal WindowsAyarlari(WindowsOturumu oturum) { this.oturum = oturum; }

    internal IEnumerable<WindowsAyari> Liste()
    {
        const string arayuz = "Arama ve görev çubuğu", gizlilik = "Gizlilik", performans = "Performans", gorunum = "Görünüm", gezgin = "Dosya Gezgini", giris = "Klavye ve fare";
        bool w11 = oturum.Derleme >= 22000;
        yield return Ayar(arayuz, "Arama öne çıkanları", "Arama panelindeki haberler, görseller ve gündem kartları.",
            true, Cv + "SearchSettings", "IsDynamicSearchBoxEnabled", 1, 0, true,
            delegate { return Yasak(Ilke + "Windows Search", "EnableDynamicContentInWSB", 0) ?? Yasak(Mdm + "Search", "AllowSearchHighlights", 0); }, oturum.Derleme >= 19045);
        yield return w11
            ? Ayar(arayuz, "Görev çubuğu widget’ları", "Görev çubuğundaki hava durumu ve haberler düğmesi.", true, Cv + @"Explorer\Advanced", "TaskbarDa", 1, 0, true,
                delegate { return Yasak(@"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0) ?? Yasak(@"SOFTWARE\Policies\Microsoft\Dsh", "DisableWidgetsBoard", 1) ?? Yasak(Mdm + "NewsAndInterests", "AllowNewsAndInterests", 0); })
            : Ayar(arayuz, "Haberler ve ilgi alanları", "Windows 10 görev çubuğundaki hava durumu ve haberler.", true, Cv + "Feeds", "ShellFeedsTaskbarViewMode", 0, 2, true,
                delegate { return Yasak(Ilke + "Windows Feeds", "EnableFeeds", 0); }, oturum.Derleme >= 19043);
        yield return Ayar(arayuz, "Cihazda arama geçmişi", "Bu cihazdaki önceki Windows aramalarını kullanır.",
            true, Cv + "SearchSettings", "IsDeviceSearchHistoryEnabled", 1, 0, true, null);
        yield return Ayar(arayuz, "Son açılan öğeler", "Başlat ve uygulamaların atlama listelerinde son açılan dosyaları gösterir.",
            true, Cv + @"Explorer\Advanced", "Start_TrackDocs", 1, 0, true,
            delegate { return Yasak(Cv + @"Policies\Explorer", "NoRecentDocsHistory", 1) ?? Yasak(Cv + @"Policies\Explorer", "NoRecentDocsHistory", 1, true); });
        if (w11)
        {
            yield return Ayar(arayuz, "Görev çubuğunu sola hizala", "Başlat düğmesini ve sabitlenmiş simgeleri sol kenara taşır.", true, Advanced, "TaskbarAl", 0, 1, false, null);
            yield return Ayar(arayuz, "Görev Görünümü düğmesi", "Sanal masaüstleri için görev çubuğunda düğme gösterir.", true, Advanced, "ShowTaskViewButton", 1, 0, true,
                () => Yasak(Ilke + "Explorer", "HideTaskViewButton", 1) ?? Yasak(Ilke + "Explorer", "HideTaskViewButton", 1, true));
            yield return Ayar(arayuz, "Sağ köşeden masaüstünü göster", "Görev çubuğunun en sağ kenarına tıklayınca masaüstünü gösterir.", true, Advanced, "TaskbarSd", 1, 0, null, null);
            yield return Ayar(arayuz, "Dikkat isteyen uygulama simgeleri", "Dikkat isteyen uygulamalar görev çubuğunda yanıp söner.", true, Advanced, "TaskbarFlashing", 1, 0, true, null);
            yield return Ayar(arayuz, "Görev çubuğu saatinde saniye", "Sistem tepsisi saatine saniye göstergesini ekler.", true, Advanced, "ShowSecondsInSystemClock", 1, 0, false, null, oturum.Derleme >= 22631);
            yield return Ayar(arayuz, "Başlat menüsünde uygulama ipuçları", "Başlat menüsünde ipuçları, kısayollar ve uygulama önerileri gösterir.", true, Advanced, "Start_IrisRecommendations", 1, 0, true, null, oturum.Derleme >= 22621);
            yield return Ayar(arayuz, "Görev çubuğu menüsünde Görevi sonlandır", "Uygulama simgesinin sağ tık menüsünde Görevi sonlandır komutunu gösterir.",
                true, Advanced + @"\TaskbarDeveloperSettings", "TaskbarEndTask", 1, 0, false, null, oturum.Derleme >= 22631);
        }
        yield return Ayar(gezgin, "Dosya uzantıları", "Bilinen dosya türlerinin uzantılarını gösterir.", true, Advanced, "HideFileExt", 0, 1, false, null);
        yield return Ayar(gezgin, "Gizli öğeler", "Gizli işaretli dosya ve klasörleri gösterir.", true, Advanced, "Hidden", 1, 2, false, null);
        yield return Ayar(gezgin, "Başlıkta tam klasör yolu", "Dosya Gezgini başlık çubuğunda klasörün tam yolunu gösterir.", true, Cv + @"Explorer\CabinetState", "FullPath", 1, 0, false, null);
        yield return Ayar(gezgin, "Son kullanılan dosyalar", "Giriş veya Hızlı Erişim sayfasında son dosyaları gösterir.", true, Cv + "Explorer", "ShowRecent", 1, 0, true,
            () => Yasak(Cv + @"Policies\Explorer", "NoRecentDocsHistory", 1) ?? Yasak(Cv + @"Policies\Explorer", "NoRecentDocsHistory", 1, true));
        yield return Ayar(gezgin, "Sık kullanılan klasörler", "Giriş veya Hızlı Erişim sayfasında sık giriş yapılan klasörleri gösterir.", true, Cv + "Explorer", "ShowFrequent", 1, 0, true, null);
        yield return Ayar(gezgin, "Eşitleme sağlayıcısı uyarıları", "Dosya Gezgini’nde OneDrive gibi eşitleme sağlayıcılarının bildirimlerini gösterir.", true, Advanced, "ShowSyncProviderNotifications", 1, 0, true, null);

        yield return Ayar(gizlilik, "Konum hizmetleri", "Haritalar, otomatik saat dilimi ve konumu kullanan uygulamaları etkiler.",
            false, Cv + @"CapabilityAccessManager\ConsentStore\location", "Value", "Allow", "Deny", null,
            delegate { return Yasak(Ilke + "LocationAndSensors", "DisableLocation", 1) ?? Yasak(Ilke + "LocationAndSensors", "DisableLocation", 1, true); });
        yield return new WindowsAyari(gizlilik, "İsteğe bağlı tanılama", "Microsoft’a gönderilen isteğe bağlı tanılama verileri. Zorunlu tanılama düzeyi korunur.", true,
            delegate { return (VarSayi(oturum.Oku(false, Cv + @"Policies\DataCollection", "AllowTelemetry")) ?? 1) >= 3; },
            delegate(bool acik) { oturum.Yaz(false, Cv + @"Policies\DataCollection", "AllowTelemetry", acik ? 3 : 1); },
            delegate
            {
                return KisitiOku(Ilke + "DataCollection", "AllowTelemetry", value => value is >= 0 and < 3) ??
                    KisitiOku(Ilke + "DataCollection", "AllowTelemetry", value => value is >= 0 and < 3, true) ??
                    KisitiOku(Mdm + "System", "AllowTelemetry", value => value is >= 0 and < 3) ??
                    IlkeOku(Ilke + "DataCollection", "DisableTelemetryOptInSettingsUx", 1, (VarSayi(oturum.Oku(false, Cv + @"Policies\DataCollection", "AllowTelemetry")) ?? 1) >= 3) ??
                    IlkeOku(Mdm + "System", "ConfigureTelemetryOptInSettingsUx", 1, (VarSayi(oturum.Oku(false, Cv + @"Policies\DataCollection", "AllowTelemetry")) ?? 1) >= 3);
            });
        yield return Ayar(gizlilik, "Reklam kimliği", "Uygulamaların Windows reklam kimliğini kullanmasına izin verir.", true, Cv + "AdvertisingInfo", "Enabled", 1, 0, null,
            delegate { return Yasak(Ilke + "AdvertisingInfo", "DisabledByGroupPolicy", 1); });
        yield return Ayar(gizlilik, w11 ? "Kişiselleştirilmiş teklifler" : "Kişiselleştirilmiş deneyimler", "Windows önerilerinin tanılama verilerine göre kişiselleştirilmesi.",
            true, Cv + "Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 1, 0, null,
            delegate { return Yasak(Ilke + "CloudContent", "DisableTailoredExperiencesWithDiagnosticData", 1, true); });
        yield return Ayar(gizlilik, "Pano geçmişi", "Windows + V ile son kopyalanan öğeleri saklar.", true, Cv + "Clipboard", "EnableClipboardHistory", 1, 0, false,
            delegate { return Yasak(Ilke + "System", "AllowClipboardHistory", 0); });
        yield return UygulamaIzni(gizlilik, "Uygulamalara kamera izni", "Windows uygulamaları için varsayılan kamera erişimi. Uygulama başına izinler ayrıca uygulanır.", "webcam", "LetAppsAccessCamera");
        yield return UygulamaIzni(gizlilik, "Uygulamalara mikrofon izni", "Windows uygulamaları için varsayılan mikrofon erişimi. Uygulama başına izinler ayrıca uygulanır.", "microphone", "LetAppsAccessMicrophone");
        const string klavye = @"SOFTWARE\Microsoft\input\Settings";
        yield return Ayar(giris, "Fiziksel klavye metin önerileri", "Fiziksel klavyede yazarken Windows metin önerileri gösterir.", true, klavye, "EnableHwkbTextPrediction", 1, 0, false, null);
        yield return Ayar(giris, "Çok dilli metin önerileri", "Yüklü giriş dillerinde birden fazla dil için öneri üretir.", true, klavye, "MultilingualEnabled", 1, 0, false, null);
        yield return new WindowsAyari(giris, "İşaretçi hassasiyetini artır", "Fare hızına göre işaretçinin katettiği mesafeyi değiştiren Windows fare ivmesini kullanır.", oturum.AyniHesap,
            () => MouseOku() is int[] deger ? deger[2] != 0 : null, MouseYaz, null);

        yield return Ayar(performans, "Arka planda oyun kaydı", "Xbox Game Bar’ın oynanışı arka planda kaydetme seçeneği.",
            true, Cv + "GameDVR", "HistoricalCaptureEnabled", 1, 0, false,
            delegate { return Yasak(Ilke + "GameDVR", "AllowGameDVR", 0); });
        yield return Ayar(performans, "Oyun Modu", "Windows oyun oynanırken arka plan etkinliğini azaltabilir.",
            true, @"SOFTWARE\Microsoft\GameBar", "AutoGameModeEnabled", 1, 0, true, null);
        yield return Ayar(performans, "Kumandayla Xbox Game Bar’ı aç", "Desteklenen denetleyicide Xbox düğmesiyle oyun çubuğunu açar.",
            true, @"SOFTWARE\Microsoft\GameBar", "UseNexusForGameBarEnabled", 1, 0, null, () => Yasak(Ilke + "GameDVR", "AllowGameDVR", 0));
        if (!w11)
            yield return Ayar(performans, "Arka plan mağaza uygulamaları", "Windows 10’da Store uygulamalarının arka planda çalışması.",
                true, Cv + "BackgroundAccessApplications", "GlobalUserDisabled", 0, 1, true,
                delegate { return Yasak(Ilke + "AppPrivacy", "LetAppsRunInBackground", 2); });
        yield return new WindowsAyari(gorunum, "Arayüz animasyonları", "Windows menülerindeki ve pencerelerindeki geçiş efektleri.", oturum.AyniHesap,
            AnimasyonuOku, AnimasyonuYaz, null);
        yield return Ayar(gorunum, "Saydamlık efektleri", "Görev çubuğu ve Windows arayüzündeki saydam yüzeyler.", true,
            Cv + @"Themes\Personalize", "EnableTransparency", 1, 0, true, null);
        yield return Ayar(gorunum, "Uygulamalarda koyu mod", "Windows’un uygulamalar için kullandığı varsayılan renk modunu değiştirir.", true, Cv + @"Themes\Personalize", "AppsUseLightTheme", 0, 1, false, null);
        yield return Ayar(gorunum, "Windows yüzeylerinde koyu mod", "Görev çubuğu ve Başlat gibi Windows yüzeylerinin renk modunu değiştirir.", true, Cv + @"Themes\Personalize", "SystemUsesLightTheme", 0, 1, null, null);
        yield return new WindowsAyari(performans, "Yüksek performans güç planı", "Varsa yerleşik Yüksek performans planını seçer. Kapatmak Dengeli plana döner.",
            GucPlani.Varmi(GucPlani.Yuksek) && GucPlani.Varmi(GucPlani.Dengeli), GucPlani.Oku, GucPlani.Yaz, null);
    }

    private WindowsAyari Ayar(string grup, string ad, string aciklama, bool kullanici, string yol, string deger,
        object acik, object kapali, bool? varsayilan, Func<IlkeDurumu> ilke, bool destekli = true)
    {
        return new WindowsAyari(grup, ad, aciklama, destekli && (!kullanici || oturum.KullaniciKaydiVar),
            delegate
            {
                var veri = oturum.Oku(kullanici, yol, deger);
                if (veri == null) return varsayilan;
                return String.Equals(Convert.ToString(veri), Convert.ToString(acik), StringComparison.OrdinalIgnoreCase) ? true :
                    String.Equals(Convert.ToString(veri), Convert.ToString(kapali), StringComparison.OrdinalIgnoreCase) ? false : varsayilan;
            }, delegate(bool durum) { oturum.Yaz(kullanici, yol, deger, durum ? acik : kapali); }, ilke);
    }

    private WindowsAyari UygulamaIzni(string grup, string ad, string aciklama, string capability, string ilke)
    {
        string yol = Cv + @"CapabilityAccessManager\ConsentStore\" + capability;
        return Ayar(grup, ad, aciklama, true, yol, "Value", "Allow", "Deny", null, () =>
            IlkeOku(Ilke + "AppPrivacy", ilke, 2, false) ?? IlkeOku(Ilke + "AppPrivacy", ilke, 1, true) ??
            IlkeOku(Mdm + "Privacy", ilke, 2, false) ?? IlkeOku(Mdm + "Privacy", ilke, 1, true) ??
            (Equals(oturum.Oku(false, yol, "Value"), "Deny") ? new IlkeDurumu(false, $"Cihaz erişimi kapalı · HKLM\\{yol}\\Value = Deny") : null));
    }

    private IlkeDurumu Yasak(string yol, string ad, int yasak, bool kullanici = false) => IlkeOku(yol, ad, yasak, false, kullanici);

    private IlkeDurumu IlkeOku(string yol, string ad, int deger, bool acik, bool kullanici = false) =>
        VarSayi(oturum.Oku(kullanici, yol, ad)) == deger ? new IlkeDurumu(acik, IlkeYolu(kullanici, yol, ad, deger)) : null;

    private IlkeDurumu KisitiOku(string yol, string ad, Func<int, bool> kisit, bool kullanici = false)
    {
        var deger = VarSayi(oturum.Oku(kullanici, yol, ad));
        return deger.HasValue && kisit(deger.Value) ? new IlkeDurumu(false, IlkeYolu(kullanici, yol, ad, deger.Value)) : null;
    }

    private static string IlkeYolu(bool kullanici, string yol, string ad, int deger) =>
        $"{(kullanici ? "HKCU" : "HKLM")}\\{yol}\\{ad} = {deger}";

    private static int[] MouseOku()
    {
        var degerler = new int[3];
        return MouseSpi(3, 0, degerler, 0) ? degerler : null;
    }

    private static void MouseYaz(bool acik)
    {
        var degerler = MouseOku() ?? throw new Win32Exception();
        degerler[2] = acik ? 1 : 0;
        if (!MouseSpi(4, 0, degerler, 3)) throw new Win32Exception();
    }

    private static int? VarSayi(object nesne) { return nesne is int ? (int?)nesne : null; }

    private static bool? AnimasyonuOku()
    {
        int acik;
        return GetSpi(0x1042, 0, out acik, 0) ? (bool?)(acik != 0) : null;
    }

    private static void AnimasyonuYaz(bool acik)
    {
        if (!SetSpi(0x1043, 0, new IntPtr(acik ? 1 : 0), 3)) throw new Win32Exception();
    }

    [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)] private static extern bool GetSpi(uint action, uint param, out int value, uint flags);
    [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)] private static extern bool MouseSpi(uint action, uint param, [In, Out] int[] value, uint flags);
    [DllImport("user32.dll", EntryPoint = "SystemParametersInfoW", SetLastError = true)] private static extern bool SetSpi(uint action, uint param, IntPtr value, uint flags);
}

internal static class GucPlani
{
    internal static readonly Guid Yuksek = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
    internal static readonly Guid Dengeli = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");

    internal static bool Varmi(Guid aranan)
    {
        for (uint sira = 0; ; sira++)
        {
            uint boyut = 16;
            Guid plan;
            if (PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 16, sira, out plan, ref boyut) != 0) return false;
            if (plan == aranan) return true;
        }
    }

    internal static bool? Oku()
    {
        IntPtr plan;
        if (PowerGetActiveScheme(IntPtr.Zero, out plan) != 0) return null;
        try { return (Guid)Marshal.PtrToStructure(plan, typeof(Guid)) == Yuksek; }
        finally { LocalFree(plan); }
    }

    internal static void Yaz(bool acik)
    {
        Guid plan = acik ? Yuksek : Dengeli;
        uint hata = PowerSetActiveScheme(IntPtr.Zero, ref plan);
        if (hata != 0) throw new Win32Exception((int)hata);
    }

    [DllImport("powrprof.dll")] private static extern uint PowerEnumerate(IntPtr key, IntPtr plan, IntPtr sub, uint mode, uint index, out Guid value, ref uint bytes);
    [DllImport("powrprof.dll")] private static extern uint PowerGetActiveScheme(IntPtr key, out IntPtr plan);
    [DllImport("powrprof.dll")] private static extern uint PowerSetActiveScheme(IntPtr key, ref Guid plan);
    [DllImport("kernel32.dll")] private static extern IntPtr LocalFree(IntPtr memory);
}
