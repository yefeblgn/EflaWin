using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

internal sealed class KuruluUygulama
{
    internal string Ad, KaldirmaKomutu;
}

internal static class UygulamaYonetimi
{
    private const string KaldirmaYolu = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    internal static Dictionary<Uygulama, KuruluUygulama> KuruluOlanlar(WindowsOturumu oturum)
    {
        var sonuc = new Dictionary<Uygulama, KuruluUygulama>();
        foreach (RegistryView gorunum in Environment.Is64BitOperatingSystem ?
            new[] { RegistryView.Registry64, RegistryView.Registry32 } : new[] { RegistryView.Registry32 })
        {
            Tara(sonuc, RegistryHive.Users, gorunum, oturum.Sid + "\\" + KaldirmaYolu);
            Tara(sonuc, RegistryHive.LocalMachine, gorunum, KaldirmaYolu);
        }
        return sonuc;
    }

    private static void Tara(Dictionary<Uygulama, KuruluUygulama> sonuc, RegistryHive kok, RegistryView gorunum, string yol)
    {
        using (var kayit = RegistryKey.OpenBaseKey(kok, gorunum))
        using (var anahtar = kayit.OpenSubKey(yol))
        {
            if (anahtar == null) return;
            foreach (string alt in anahtar.GetSubKeyNames())
            {
                try
                {
                    using (var girdi = anahtar.OpenSubKey(alt))
                    {
                        if (girdi == null || Equals(girdi.GetValue("SystemComponent"), 1)) continue;
                        string ad = girdi.GetValue("DisplayName") as string;
                        if (String.IsNullOrWhiteSpace(ad)) continue;
                        foreach (var uygulama in Uygulama.Katalog)
                        {
                            if (!uygulama.KuruluAdi(ad) || sonuc.ContainsKey(uygulama)) continue;
                            sonuc[uygulama] = new KuruluUygulama { Ad = ad, KaldirmaKomutu = girdi.GetValue("UninstallString") as string };
                        }
                    }
                }
                catch (System.Security.SecurityException) { }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }
    }

    internal static bool YerelKurucu(string yol)
    {
        try
        {
            using (var dosya = File.OpenRead(yol))
            {
                if (dosya.Length < 64) return false;
                var baslik = new byte[8];
                return dosya.Read(baslik, 0, baslik.Length) == baslik.Length && IndirmeMotoru.KurucuBasligi(baslik, yol);
            }
        }
        catch (IOException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    internal static void Kaldir(KuruluUygulama kayit)
    {
        string dosya, arguman;
        if (!KaldirmayiCoz(kayit.KaldirmaKomutu, out dosya, out arguman))
            GezginleAc("ms-settings:appsfeatures");
        else
            GezginleAc(dosya, arguman);
    }

    internal static bool KaldirmayiCoz(string komut, out string dosya, out string arguman)
    {
        dosya = null;
        arguman = "";
        if (String.IsNullOrWhiteSpace(komut)) return false;
        var msi = Regex.Match(komut, @"\A\s*(?:""[^""]*\\)?msiexec(?:\.exe)?""?\s+[/-][ix]\s*(\{[0-9a-f-]{36}\})\s*\z", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        Guid urun;
        if (msi.Success && Guid.TryParse(msi.Groups[1].Value, out urun))
        {
            dosya = Path.Combine(Environment.SystemDirectory, "msiexec.exe");
            arguman = "/x " + urun.ToString("B");
            return true;
        }

        var exe = Regex.Match(komut, @"\A\s*(?:""([^""]+\.exe)""|([^""\r\n]+?\.exe))(?=\s|\z)(.*)\z", RegexOptions.IgnoreCase);
        if (!exe.Success) return false;
        dosya = exe.Groups[1].Success ? exe.Groups[1].Value : exe.Groups[2].Value;
        arguman = exe.Groups[3].Value.Trim();
        return Path.IsPathRooted(dosya) && !dosya.StartsWith(@"\\", StringComparison.Ordinal) && File.Exists(dosya);
    }

    internal static void GezginleAc(string dosya, string arguman = "")
    {
        dynamic kabukPencereleri = Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39")));
        dynamic masaustu = null;
        dynamic belge = null;
        dynamic kabuk = null;
        try
        {
            object konum = 0, kok = 0;
            int pencere;
            masaustu = kabukPencereleri.FindWindowSW(ref konum, ref kok, 8, out pencere, 1);
            if (masaustu == null) throw new InvalidOperationException("Windows Gezgini masaüstü bulunamadı.");
            belge = masaustu.Document;
            kabuk = belge.Application;
            string klasor = File.Exists(dosya) ? Path.GetDirectoryName(dosya) : "";
            kabuk.ShellExecute(dosya, arguman, klasor, "open", 1);
        }
        finally
        {
            Birak(kabuk); Birak(belge); Birak(masaustu); Birak(kabukPencereleri);
        }
    }

    private static void Birak(object nesne)
    {
        if (nesne != null && Marshal.IsComObject(nesne)) Marshal.ReleaseComObject(nesne);
    }
}
