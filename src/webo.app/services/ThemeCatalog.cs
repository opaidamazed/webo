using Webo.App.Models;

namespace Webo.App;

public sealed record PaletteDef(string Id, string Accent, string Dark);

/// <summary>The choices offered for site identity, and how a choice becomes concrete CSS values.</summary>
public static class ThemeCatalog
{
    public const string Custom = "custom";

    public static readonly PaletteDef[] Palettes =
    {
        new("ocean", "#2090F0", "#0B1033"),
        new("emerald", "#12A06B", "#07261B"),
        new("sunset", "#F2711C", "#2B1204"),
        new("royal", "#6D4AE8", "#1A1040"),
        new("rose", "#D6336C", "#2E0A18"),
        new("slate", "#334155", "#0F172A"),
    };

    /// <summary>Ids in the order shown in the UI: the presets, then "custom".</summary>
    public static readonly string[] PaletteIds = Palettes.Select(p => p.Id).Append(Custom).ToArray();
    public static readonly string[] Fonts = { "plex", "system", "serif" };
    public static readonly string[] Radii = { "sharp", "medium", "round" };

    public static string FontStack(string id) => id switch
    {
        "system" => "\"Segoe UI\",Tahoma,Arial,sans-serif",
        "serif" => "\"Sakkal Majalla\",\"Traditional Arabic\",Georgia,\"Times New Roman\",serif",
        _ => "\"IBM Plex Sans Arabic\",\"Segoe UI\",Tahoma,Arial,sans-serif",
    };

    public static string RadiusPx(string id) => id switch
    {
        "sharp" => "4px",
        "round" => "22px",
        _ => "12px",
    };

    /// <summary>Accent and dark colour for a theme. A custom accent that is not a valid #RRGGBB falls back to the first preset.</summary>
    public static (string Accent, string Dark) Resolve(ThemeDoc theme)
    {
        if (theme.Palette == Custom)
        {
            var hex = NormalizeHex(theme.Accent);
            if (hex != null) return (hex, Mix(hex, "#0B1033", 0.18));
        }
        var preset = Palettes.FirstOrDefault(p => p.Id == theme.Palette) ?? Palettes[0];
        return (preset.Accent, preset.Dark);
    }

    /// <summary>Accepts "2090f0" or "#2090F0"; returns "#2090F0", or null when it is not a 6-digit hex colour.</summary>
    public static string? NormalizeHex(string? text)
    {
        var t = (text ?? "").Trim();
        if (t.StartsWith('#')) t = t.Substring(1);
        if (t.Length != 6) return null;
        foreach (var c in t)
            if (!(c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F')) return null;
        return "#" + t.ToUpperInvariant();
    }

    /// <summary>Text colour to put on top of the accent: white, or the dark navy when the accent is light.</summary>
    public static string InkFor(string accentHex)
    {
        var (r, g, b) = Rgb(accentHex);
        double L(int v)
        {
            var c = v / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }
        var luminance = 0.2126 * L(r) + 0.7152 * L(g) + 0.0722 * L(b);
        return luminance > 0.45 ? "#0B1033" : "#FFFFFF";
    }

    private static (int R, int G, int B) Rgb(string hex) =>
        (Convert.ToInt32(hex.Substring(1, 2), 16), Convert.ToInt32(hex.Substring(3, 2), 16), Convert.ToInt32(hex.Substring(5, 2), 16));

    /// <summary>Mixes two #RRGGBB colours; <paramref name="amountOfA"/> is the share of the first one (0..1).</summary>
    private static string Mix(string a, string b, double amountOfA)
    {
        var (ar, ag, ab) = Rgb(a);
        var (br, bg, bb) = Rgb(b);
        int M(int x, int y) => (int)Math.Round(x * amountOfA + y * (1 - amountOfA));
        return "#" + M(ar, br).ToString("X2") + M(ag, bg).ToString("X2") + M(ab, bb).ToString("X2");
    }
}
