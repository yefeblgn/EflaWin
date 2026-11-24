using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

internal sealed class Uygulama
{
    internal readonly string Ad;
    internal readonly string Kategori;
    internal readonly string Logo;
    internal readonly Uri Baglanti;
    internal readonly string DosyaAdi;
    internal readonly bool WinRar;
    internal readonly string Grup;
    internal readonly string Kaynak;
    private readonly Regex kuruluAdi;

    internal Uygulama(string ad, string kategori, string logo, string baglanti, string dosyaAdi, string grup,
        string kurulu, bool winRar = false, string kaynak = null)
    {
        Ad = ad;
        Kategori = kategori;
        Logo = logo;
        Baglanti = new Uri(baglanti);
        DosyaAdi = dosyaAdi;
        WinRar = winRar;
        Grup = grup;
        Kaynak = kaynak ?? baglanti;
        kuruluAdi = new Regex(kurulu, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    internal bool KuruluAdi(string ad) { return kuruluAdi.IsMatch(ad); }

    internal static readonly Uygulama[] Katalog =
    {
        new Uygulama("Solaris Launcher", "Minecraft başlatıcısı", "solaris", "https://solarislauncher.com/download/latest", "Solaris Launcher Setup.exe", "Oyun", "^Solaris Launcher(?:$| [0-9])"),
        new Uygulama("SplitWire-Turkey", "Ağ aracı · 1.5.7", "splitwire", "https://github.com/cagritaskn/SplitWire-Turkey/releases/download/1.5.7/SplitWire-Turkey-Setup-Windows-1.5.7.exe", "SplitWire-Turkey-Setup-Windows-1.5.7.exe", "Araçlar", "^SplitWire[- ]Turkey(?:$| [0-9])"),
        new Uygulama("Steam", "Oyun platformu", "steam", "https://cdn.fastly.steamstatic.com/client/installer/SteamSetup.exe", "SteamSetup.exe", "Oyun", "^Steam$"),
        new Uygulama("Epic Games Launcher", "Oyun platformu", "epic", "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/installer/download/EpicGamesLauncherSetup-x86_64.exe", "EpicGamesLauncherSetup.exe", "Oyun", "^Epic Games Launcher$"),
        new Uygulama("Spotify", "Müzik", "spotify", "https://download.scdn.co/SpotifySetup.exe", "SpotifySetup.exe", "Araçlar", "^Spotify(?: Music)?$"),
        new Uygulama("FireAlpaca", "Dijital çizim", "firealpaca", "https://firealpaca.com/download/win64", "FireAlpaca64Setup.exe", "Çizim ve tasarım", "^FireAlpaca(?: ?[0-9]| version|$)"),
        new Uygulama("VALORANT", "Oyun · Avrupa", "valorant", "https://valorant.secure.dyn.riotcdn.net/channels/public/x/installer/current/live.live.eu.exe", "Install VALORANT.exe", "Oyun", "^VALORANT$"),
        new Uygulama("WinRAR", "Arşiv · Türkçe", "winrar", "https://www.win-rar.com/postdownload.html?&L=5", "WinRAR-x64-tr.exe", "Araçlar", "^WinRAR(?: |$)", true),
        new Uygulama("Brave", "Web tarayıcısı", "brave", "https://laptop-updates.brave.com/latest/winx64", "BraveBrowserSetup.exe", "Tarayıcılar", "^Brave(?: Browser)?$", false, "https://brave.com/tr/download/"),
        new Uygulama("ValorantRPC", "yefeblgn · Discord Rich Presence", "vrpc", "https://github.com/yefeblgn/valorantrpc/releases/download/v3.1.1/ValorantRPC-Setup-3.1.1.exe", "ValorantRPC-Setup-3.1.1.exe", "Oyun", "^ValorantRPC(?:$| [0-9])"),
        new Uygulama("Google Chrome", "Web tarayıcısı", "chrome", "https://dl.google.com/dl/chrome/install/googlechromestandaloneenterprise64.msi", "GoogleChrome-x64.msi", "Tarayıcılar", "^Google Chrome$"),
        new Uygulama("Opera", "Web tarayıcısı", "opera", "https://net.geo.opera.com/opera/stable/windows?arch=x64", "OperaSetup.exe", "Tarayıcılar", "^Opera(?: Stable)?(?:$| [0-9])"),
        new Uygulama("Opera GX", "Oyuncu tarayıcısı", "operagx", "https://net.geo.opera.com/opera_gx/stable/windows?arch=x64", "OperaGXSetup.exe", "Tarayıcılar", "^Opera GX(?: Stable)?(?:$| [0-9])"),
        new Uygulama("Battle.net", "Blizzard oyun platformu", "battlenet", "https://downloader.battle.net/download/getInstaller?os=win&installer=Battle.net-Setup.exe", "Battle.net-Setup.exe", "Oyun", @"^(?:Battle\.net|Blizzard Battle\.net)(?:$| App$)"),
        new Uygulama("Ubisoft Connect", "Ubisoft oyun platformu", "ubisoft", "https://static3.cdn.ubi.com/orbit/launcher_installer/UbisoftConnectInstaller.exe", "UbisoftConnectInstaller.exe", "Oyun", "^(?:Ubisoft Connect|Uplay)$"),
        new Uygulama("Krita", "Dijital boyama", "krita", "https://download.kde.org/stable/krita/5.3.4.1/krita-x64-5.3.4.1-setup.exe", "KritaSetup.exe", "Çizim ve tasarım", @"^Krita(?:$| [0-9]| \(x64\))"),
        new Uygulama("Blender", "3B modelleme ve animasyon", "blender", "https://download.blender.org/release/Blender5.3/blender-5.3.0-windows-x64.msi", "Blender-x64.msi", "Çizim ve tasarım", "^Blender(?:$| [0-9])"),
        new Uygulama("GIMP", "Görsel düzenleme", "gimp", "https://download.gimp.org/gimp/v3.2/windows/gimp-3.2.6-setup.exe", "GIMPSetup.exe", "Çizim ve tasarım", "^GIMP(?:$| [0-9])"),
        new Uygulama("Inkscape", "Vektör çizimi", "inkscape", "https://media.inkscape.org/dl/resources/file/inkscape-1.4.4_2026-05-05_dcaf3e7-x64.signed_xMx7DJV.msi", "Inkscape-x64.msi", "Çizim ve tasarım", "^Inkscape(?:$| [0-9])"),
        new Uygulama("Visual Studio Code", "Kod editörü", "vscode", "https://update.code.visualstudio.com/latest/win32-x64-user/stable", "VSCodeUserSetup-x64.exe", "Kodlama", "^(?:Microsoft )?Visual Studio Code(?:$| \\(User\\))"),
        new Uygulama("Git", "Sürüm kontrolü", "git", "https://github.com/git-for-windows/git/releases/download/v2.56.1.windows.1/Git-2.56.1-64-bit.exe", "GitSetup-x64.exe", "Kodlama", "^Git(?:$| version | [0-9])"),
        new Uygulama("GitHub Desktop", "Git için masaüstü arayüzü", "github", "https://desktop.githubusercontent.com/releases/3.7.0-f13859b6/GitHubDesktopSetup-x64.exe", "GitHubDesktopSetup-x64.exe", "Kodlama", "^GitHub Desktop$"),
        new Uygulama("JetBrains Toolbox", "JetBrains IDE yöneticisi", "jetbrains", "https://download.jetbrains.com/toolbox/jetbrains-toolbox-3.9.1.89198.exe", "JetBrainsToolboxSetup.exe", "Kodlama", "^JetBrains Toolbox(?:$| [0-9])"),
        new Uygulama("Microsoft PowerToys", "Windows üretkenlik araçları", "powertoys", "https://github.com/microsoft/PowerToys/releases/download/v0.102.3101/PowerToysUserSetup-0.102.3101.0-x64.exe", "PowerToysUserSetup-x64.exe", "Araçlar", "^(?:Microsoft )?PowerToys(?:$| [0-9]| \\(Preview\\))"),
        new Uygulama("HWiNFO", "Donanım ve sensör izleme", "hwinfo", "https://www.sac.sk/download/utildiag/hwi_860x.exe", "HWiNFOSetup.exe", "Performans", "^HWiNFO(?:64|32)?(?:$|[® ])"),
        new Uygulama("CPU-Z", "İşlemci ve bellek bilgisi", "cpuz", "https://download.cpuid.com/cpu-z/cpu-z_3.02-en.exe", "CPU-ZSetup.exe", "Performans", "^(?:CPUID )?CPU-Z(?:$| [0-9])"),
        new Uygulama("Rainmeter", "Masaüstü bileşenleri", "rainmeter", "https://github.com/rainmeter/rainmeter/releases/download/v4.5.28.3936/Rainmeter-4.5.28.exe", "RainmeterSetup.exe", "Kişiselleştirme", "^Rainmeter(?:$| [0-9])"),
        new Uygulama("Lively Wallpaper", "Hareketli duvar kâğıtları", "lively", "https://github.com/rocksdanister/lively/releases/download/v2.2.1.0/lively_setup_x86_full_v2210.exe", "LivelyWallpaperSetup.exe", "Kişiselleştirme", "^Lively Wallpaper(?:$| [0-9])"),
        new Uygulama("StartAllBack", "Başlat ve görev çubuğu · deneme", "startallback", "https://startisback.sfo3.cdn.digitaloceanspaces.com/StartAllBack_3.9.29_setup.exe", "StartAllBackSetup.exe", "Kişiselleştirme", "^StartAllBack(?:$| [0-9])"),
        new Uygulama("Intel Driver & Support Assistant", "Intel sürücü asistanı", "intel", "https://dsadata.intel.com/installer", "Intel-Driver-Support-Assistant.exe", "Sürücüler", "^Intel.*Driver.*Support Assistant", false, "https://www.intel.com/content/www/us/en/support/detect.html"),
        new Uygulama("AMD Software", "AMD ekran kartı ve yonga seti taraması", "amd", "https://drivers.amd.com/drivers/installer/26.20/whql/amd-software-adrenalin-edition-26.11.2-minimalsetup-261118_web.exe", "AMD-Auto-Detect.exe", "Sürücüler", "^AMD Software(?:$|:)", false, "https://www.amd.com/en/support/download/drivers.html"),
        new Uygulama("NVIDIA App", "GeForce sürücü asistanı", "nvidia", "https://us.download.nvidia.com/nvapp/client/11.0.9.585/x64/NVIDIA_app_v11.0.9.585_x64.exe", "NVIDIA-App-x64.exe", "Sürücüler", "^NVIDIA App(?:$| [0-9])", false, "https://www.nvidia.com/en-us/software/nvidia-app/")
    };

    internal const int OnerilenSayisi = 10;
}

internal sealed class IndirmeMotoru : IDisposable
{
    private readonly HttpClient istemci;

    internal IndirmeMotoru(HttpMessageHandler tasiyici = null)
    {
        istemci = new HttpClient(tasiyici ?? new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate });
        istemci.DefaultRequestHeaders.UserAgent.ParseAdd("EflaWin/1.0.0");
        istemci.Timeout = TimeSpan.FromSeconds(60);
    }

    internal async Task<string> Indir(Uygulama uygulama, string klasor, IProgress<string> ilerleme, CancellationToken iptal)
    {
        Directory.CreateDirectory(klasor);
        Uri adres = uygulama.Baglanti;
        if (uygulama.WinRar || uygulama.Logo == "amd" || uygulama.Logo == "nvidia")
        {
            using (var sayfa = await Istek(new Uri(uygulama.Kaynak), iptal).ConfigureAwait(false))
            {
                string html = await sayfa.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (uygulama.WinRar) adres = WinRarBaglantisi(adres, html);
                else
                {
                    string desen = uygulama.Logo == "amd" ? @"https://drivers\.amd\.com/drivers/installer/[0-9.]+/whql/amd-software-[a-z0-9.-]+_web\.exe" :
                        @"https://us\.download\.nvidia\.com/nvapp/client/[0-9.]+/x64/NVIDIA_app_v[0-9.]+_x64\.exe";
                    var eslesme = Regex.Match(html, desen, RegexOptions.IgnoreCase);
                    if (eslesme.Success) adres = new Uri(eslesme.Value);
                }
            }
        }

        string gecici = Path.Combine(klasor, ".efla-" + Guid.NewGuid().ToString("N") + ".part");
        try
        {
            using (var yanit = await Istek(adres, iptal).ConfigureAwait(false))
            using (var akim = await yanit.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var dosya = new FileStream(gecici, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            {
                long? toplam = yanit.Content.Headers.ContentLength;
                byte[] tampon = new byte[81920];
                int baslik = 0;
                while (baslik < 8)
                {
                    int sayi = await Oku(akim, tampon, baslik, 8 - baslik, iptal).ConfigureAwait(false);
                    if (sayi == 0) throw new IOException("Kurulum dosyası eksik indi.");
                    baslik += sayi;
                }
                if (!KurucuBasligi(tampon, uygulama.DosyaAdi)) throw new IOException("Sunucu geçerli bir Windows kurucusu göndermedi.");
                await dosya.WriteAsync(tampon, 0, baslik, iptal).ConfigureAwait(false);
                long indirilen = baslik;
                DateTime guncelleme = DateTime.UtcNow;
                while (true)
                {
                    int miktar = await Oku(akim, tampon, 0, tampon.Length, iptal).ConfigureAwait(false);
                    if (miktar == 0) break;

                    await dosya.WriteAsync(tampon, 0, miktar, iptal).ConfigureAwait(false);
                    indirilen += miktar;
                    if (DateTime.UtcNow >= guncelleme)
                    {
                        ilerleme.Report(toplam > 0 ? $"{Boyut(indirilen)} / {Boyut(toplam.Value)}" : $"{Boyut(indirilen)} indirildi · Toplam bilinmiyor");
                        guncelleme = DateTime.UtcNow.AddMilliseconds(150);
                    }
                }
                if (indirilen < 64 || (toplam.HasValue && toplam.Value != indirilen))
                    throw new IOException("Kurulum dosyası eksik indi.");
            }

            iptal.ThrowIfCancellationRequested();
            try { KaynagiIsaretle(gecici, adres); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return Kaydet(gecici, klasor, uygulama.DosyaAdi);
        }
        finally
        {
            try { File.Delete(gecici); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private static string Boyut(long bayt) => bayt >= 1073741824L ? $"{bayt / 1073741824.0:0.00} GB" :
        bayt >= 1048576L ? $"{bayt / 1048576.0:0.0} MB" : $"{bayt / 1024.0:0} KB";

    private static void KaynagiIsaretle(string dosya, Uri adres)
    {
        using (var akim = CreateFile(dosya + ":Zone.Identifier", 0x40000000, 0, IntPtr.Zero, 2, 0, IntPtr.Zero))
        {
            if (akim.IsInvalid) return;
            using (var yazici = new StreamWriter(new FileStream(akim, FileAccess.Write), new UTF8Encoding(false)))
                yazici.Write("[ZoneTransfer]\r\nZoneId=3\r\nHostUrl=" + adres.AbsoluteUri + "\r\n");
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(string ad, uint erisim, uint paylasim, IntPtr guvenlik,
        uint acilis, uint ozellikler, IntPtr sablon);

    private static async Task<int> Oku(Stream akim, byte[] tampon, int baslangic, int boyut, CancellationToken iptal)
    {
        using (var sure = CancellationTokenSource.CreateLinkedTokenSource(iptal))
        {
            sure.CancelAfter(TimeSpan.FromSeconds(60));
            using (sure.Token.Register(akim.Dispose))
            {
                try { return await akim.ReadAsync(tampon, baslangic, boyut, sure.Token).ConfigureAwait(false); }
                catch (Exception) when (sure.IsCancellationRequested)
                {
                    iptal.ThrowIfCancellationRequested();
                    throw new IOException("Sunucudan veri alınamadı (zaman aşımı).");
                }
            }
        }
    }

    internal static bool KurucuBasligi(byte[] baslik, string dosya)
    {
        if (Path.GetExtension(dosya).Equals(".msi", StringComparison.OrdinalIgnoreCase))
        {
            byte[] imza = { 0xd0, 0xcf, 0x11, 0xe0, 0xa1, 0xb1, 0x1a, 0xe1 };
            for (int i = 0; i < imza.Length; i++) if (baslik[i] != imza[i]) return false;
            return true;
        }
        return baslik[0] == 'M' && baslik[1] == 'Z';
    }

    private async Task<HttpResponseMessage> Istek(Uri adres, CancellationToken iptal)
    {
        for (int gecis = 0; gecis < 10; gecis++)
        {
            if (adres.Scheme != Uri.UriSchemeHttps || adres.UserInfo.Length != 0)
                throw new IOException("İndirme için HTTPS bağlantısı gerekli.");
            using (var istek = new HttpRequestMessage(HttpMethod.Get, adres))
            {
                if (String.Equals(adres.Host, "drivers.amd.com", StringComparison.OrdinalIgnoreCase))
                    istek.Headers.Referrer = new Uri("https://www.amd.com/");
                var yanit = await istemci.SendAsync(istek, HttpCompletionOption.ResponseHeadersRead, iptal).ConfigureAwait(false);
            int kod = (int)yanit.StatusCode;
            if (kod == 200) return yanit;
            using (yanit)
            {
                if ((kod == 301 || kod == 302 || kod == 303 || kod == 307 || kod == 308) && yanit.Headers.Location != null)
                    adres = new Uri(adres, yanit.Headers.Location);
                else
                    throw new IOException("Sunucu HTTP " + kod + " yanıtı verdi.");
            }
            }
        }
        throw new IOException("Çok fazla yönlendirme yapıldı.");
    }

    internal static Uri WinRarBaglantisi(Uri sayfa, string html)
    {
        var eslesme = Regex.Match(html, "(?:https://www\\.win-rar\\.com)?/fileadmin/winrar-versions/winrar/winrar-x64-[0-9]+tr\\.exe(?=[\"'])", RegexOptions.IgnoreCase);
        if (!eslesme.Success) throw new IOException("Türkçe WinRAR kurucusu sayfada bulunamadı.");
        return new Uri(sayfa, eslesme.Value);
    }

    private static string Kaydet(string gecici, string klasor, string ad)
    {
        for (int sira = 1; ; sira++)
        {
            string dosya = sira == 1 ? ad : Path.GetFileNameWithoutExtension(ad) + " (" + sira + ")" + Path.GetExtension(ad);
            string hedef = Path.Combine(klasor, dosya);
            try { File.Move(gecici, hedef); return hedef; }
            catch (IOException) when (File.Exists(hedef) || Directory.Exists(hedef)) { }
        }
    }

    public void Dispose() { istemci.Dispose(); }
}
