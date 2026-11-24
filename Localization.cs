using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Efla;

internal static class Localization
{
    private static readonly string[] desteklenen = { "tr", "en", "es", "ru", "de", "zh", "ja" };
    private static readonly Lazy<Dictionary<string, Dictionary<string, string>>> paket = new(PaketiOku);

    internal static string Tercih { get; private set; } = "auto";
    internal static string Dil { get; private set; } = SistemDili();

    internal static void Sec(string tercih)
    {
        Tercih = tercih == "auto" || Array.IndexOf(desteklenen, tercih) >= 0 ? tercih : "auto";
        Dil = Tercih == "auto" ? SistemDili() : Tercih;
    }

    internal static string Cevir(string metin)
    {
        if (String.IsNullOrEmpty(metin) || Dil == "tr") return metin;
        return paket.Value.TryGetValue(Dil, out var sozluk) && sozluk.TryGetValue(metin, out var ceviri) ? ceviri : metin;
    }

    private static string SistemDili()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
        {
            "en" => "en",
            "es" => "es",
            "ru" => "ru",
            "de" => "de",
            "zh" => "zh",
            "ja" => "ja",
            _ => "tr"
        };
    }

    private static Dictionary<string, Dictionary<string, string>> PaketiOku()
    {
        try
        {
            using var dosya = typeof(Localization).Assembly.GetManifestResourceStream("Efla.Languages.ui.json");
            if (dosya == null) return new();
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(dosya) ?? new();
        }
        catch (JsonException) { return new(); }
    }
}
