using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

internal sealed class DonanimBilgisi
{
    internal string Islemci = "Algılanamadı", EkranKarti = "Algılanamadı";
    internal string[] Diskler = new string[0];
    internal readonly List<string> Suruculer = new List<string>();

    internal bool Intel { get { return (Islemci + " " + EkranKarti).IndexOf("Intel", StringComparison.OrdinalIgnoreCase) >= 0; } }
    internal bool Nvidia { get { return EkranKarti.IndexOf("NVIDIA", StringComparison.OrdinalIgnoreCase) >= 0; } }
    internal bool Amd { get { return (Islemci + " " + EkranKarti).IndexOf("AMD", StringComparison.OrdinalIgnoreCase) >= 0 ||
        EkranKarti.IndexOf("Radeon", StringComparison.OrdinalIgnoreCase) >= 0; } }

    internal static DonanimBilgisi Oku()
    {
        var veri = new DonanimBilgisi();
        try { veri.Islemci = WmiAdlari("Win32_Processor"); } catch (ManagementException) { } catch (UnauthorizedAccessException) { }
        try { veri.EkranKarti = WmiAdlari("Win32_VideoController"); } catch (ManagementException) { } catch (UnauthorizedAccessException) { }
        var diskler = new List<string>();
        foreach (var disk in DriveInfo.GetDrives())
        {
            try
            {
                if (disk.DriveType == DriveType.Fixed && disk.IsReady)
                    diskler.Add(disk.Name + "   " + Gb(disk.TotalSize - disk.TotalFreeSpace) + " dolu   ·   " + Gb(disk.TotalFreeSpace) + " boş");
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        veri.Diskler = diskler.ToArray();
        try
        {
            using (var arama = new ManagementObjectSearcher("SELECT DeviceName, DriverVersion FROM Win32_PnPSignedDriver WHERE DeviceClass='DISPLAY' OR DeviceClass='NET' OR DeviceClass='MEDIA'"))
            using (var liste = arama.Get())
                foreach (ManagementObject girdi in liste)
                    using (girdi)
                    {
                        string ad = Convert.ToString(girdi["DeviceName"]);
                        string surum = Convert.ToString(girdi["DriverVersion"]);
                        if (!String.IsNullOrWhiteSpace(ad) && !String.IsNullOrWhiteSpace(surum)) veri.Suruculer.Add(ad + "  ·  " + surum);
                    }
        }
        catch (ManagementException) { }
        catch (UnauthorizedAccessException) { }
        return veri;
    }

    private static string WmiAdlari(string sinif)
    {
        var adlar = new List<string>();
        using (var arama = new ManagementObjectSearcher("SELECT Name FROM " + sinif))
        using (var liste = arama.Get())
            foreach (ManagementObject girdi in liste)
                using (girdi)
                {
                    string ad = Convert.ToString(girdi["Name"]).Trim();
                    if (ad.Length > 0) adlar.Add(ad);
                }
        return adlar.Count == 0 ? "Algılanamadı" : String.Join("  ·  ", adlar.Distinct());
    }

    internal static string Gb(long miktar) { return (miktar / 1073741824.0).ToString("0.0") + " GB"; }
}

internal static class Bellek
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Durum
    {
        internal uint Boyut, Yuk;
        internal ulong Toplam, Bos, Page, PageBos, Sanal, SanalBos, Uzatilmis;
    }

    internal static Durum Oku()
    {
        var durum = new Durum { Boyut = (uint)Marshal.SizeOf(typeof(Durum)) };
        if (!GlobalMemoryStatusEx(ref durum)) throw new System.ComponentModel.Win32Exception();
        return durum;
    }

    internal static int Temizle(string kullaniciSid, int? hedef = null)
    {
        int oturum, kendi;
        using (var surec = Process.GetCurrentProcess()) { oturum = surec.SessionId; kendi = surec.Id; }
        int sayi = 0;
        var haric = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "explorer", "dwm", "csrss", "winlogon", "sihost", "ctfmon", "lsass", "services", "fontdrvhost", "StartMenuExperienceHost", "ShellExperienceHost", "SearchHost", "SearchApp", "Taskmgr", "System", "Registry" };
        foreach (var surec in Process.GetProcesses())
            using (surec)
            {
                try
                {
                    if ((hedef.HasValue && surec.Id != hedef.Value) || surec.Id == kendi || surec.SessionId != oturum || surec.WorkingSet64 < 32 * 1048576L || haric.Contains(surec.ProcessName)) continue;
                    IntPtr kol = OpenProcess(0x1100, false, (uint)surec.Id);
                    if (kol == IntPtr.Zero) continue;
                    try
                    {
                        IntPtr token;
                        if (!OpenProcessToken(kol, 8, out token)) continue;
                        try
                        {
                            if (KullaniciIslemi(token, kullaniciSid) && EmptyWorkingSet(kol)) sayi++;
                        }
                        finally { CloseHandle(token); }
                    }
                    finally { CloseHandle(kol); }
                }
                catch (System.ComponentModel.Win32Exception) { }
                catch (InvalidOperationException) { }
                catch (UnauthorizedAccessException) { }
                catch (System.Security.SecurityException) { }
            }
        return sayi;
    }

    private static bool KullaniciIslemi(IntPtr token, string sid)
    {
        int boyut;
        IntPtr bellek = Marshal.AllocHGlobal(sizeof(int));
        try
        {
            if (!GetTokenInformation(token, 20, bellek, sizeof(int), out boyut) || Marshal.ReadInt32(bellek) != 0) return false;
        }
        finally { Marshal.FreeHGlobal(bellek); }
        GetTokenInformation(token, 1, IntPtr.Zero, 0, out boyut);
        if (boyut <= 0) return false;
        bellek = Marshal.AllocHGlobal(boyut);
        try
        {
            if (!GetTokenInformation(token, 1, bellek, boyut, out boyut)) return false;
            var kullanici = new SecurityIdentifier(Marshal.ReadIntPtr(bellek));
            return kullanici.Value == sid;
        }
        finally { Marshal.FreeHGlobal(bellek); }
    }

    [DllImport("kernel32.dll", SetLastError = true)] private static extern bool GlobalMemoryStatusEx(ref Durum durum);
    [DllImport("kernel32.dll")] private static extern IntPtr OpenProcess(uint access, bool inherit, uint processId);
    [DllImport("advapi32.dll")] private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);
    [DllImport("advapi32.dll")] private static extern bool GetTokenInformation(IntPtr token, int bilgiTuru, IntPtr bilgi, int uzunluk, out int yazilan);
    [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr handle);
    [DllImport("psapi.dll")] private static extern bool EmptyWorkingSet(IntPtr process);
    [DllImport("dnsapi.dll")] internal static extern bool DnsFlushResolverCache();
}

internal static class SurucuGuncellemeleri
{
    internal static string[] WindowsUpdateTara(CancellationToken iptal)
    {
        iptal.ThrowIfCancellationRequested();
        var basliklar = new List<string>();
        dynamic oturum = null, arama = null, islem = null, sonuc = null, liste = null;
        var callback = new WindowsUpdateSearchCompleted();
        try
        {
            oturum = Activator.CreateInstance(Type.GetTypeFromProgID("Microsoft.Update.Session"));
            arama = oturum.CreateUpdateSearcher();
            islem = arama.BeginSearch("IsInstalled=0 and IsHidden=0 and Type='Driver'", callback, null);
            while (!(bool)islem.IsCompleted)
            {
                if (iptal.WaitHandle.WaitOne(150))
                {
                    islem.RequestAbort();
                    throw new OperationCanceledException(iptal);
                }
            }
            iptal.ThrowIfCancellationRequested();
            sonuc = arama.EndSearch(islem);
            liste = sonuc.Updates;
            for (int i = 0; i < (int)liste.Count; i++)
            {
                dynamic guncelleme = liste.Item(i);
                try { basliklar.Add((string)guncelleme.Title); }
                finally { Birak(guncelleme); }
            }
            return basliklar.Distinct().ToArray();
        }
        finally
        {
            if (islem != null)
            {
                try { islem.CleanUp(); }
                catch (COMException) { }
            }
            Birak(liste); Birak(sonuc); Birak(islem); Birak(arama); Birak(oturum);
            GC.KeepAlive(callback);
        }
    }

    private static void Birak(object nesne)
    {
        if (nesne != null && Marshal.IsComObject(nesne)) Marshal.ReleaseComObject(nesne);
    }
}

[ComVisible(true)]
[Guid("88aee058-d4b0-4725-a2f1-814a67ae964c")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IWindowsUpdateSearchCompleted
{
    void Invoke(nint job, nint args);
}

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public sealed class WindowsUpdateSearchCompleted : IWindowsUpdateSearchCompleted
{
    public void Invoke(nint job, nint args) { }
}
