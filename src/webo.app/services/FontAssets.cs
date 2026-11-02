using System.Text;

namespace Webo.App;

/// <summary>
/// The bundled IBM Plex Sans Arabic (SIL OFL) as web fonts. The editor preview embeds it as base64 so it works offline;
/// the export copies the files next to the site and references them by relative URL.
/// </summary>
public static class FontAssets
{
    private const string Family = "IBM Plex Sans Arabic";

    // Regular + Bold only: keeps the preview document small. Weights in between map to these two.
    private static readonly (string File, int Weight)[] Faces =
    {
        ("IBMPlexSansArabic-Regular.ttf", 400),
        ("IBMPlexSansArabic-Bold.ttf", 700),
    };

    private static string? _embedded;

    private static string Dir => Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts");

    /// <summary>Full paths of the font files that exist (used by the export to copy them).</summary>
    public static IReadOnlyList<string> Files =>
        Faces.Select(f => Path.Combine(Dir, f.File)).Where(File.Exists).ToList();

    /// <summary>@font-face rules with the fonts inlined as data: URLs (preview). Empty when the files are missing.</summary>
    public static string EmbeddedCss()
    {
        if (_embedded != null) return _embedded;
        var sb = new StringBuilder();
        foreach (var (file, weight) in Faces)
        {
            var path = Path.Combine(Dir, file);
            if (!File.Exists(path)) { AppLog.Write("font missing: " + path); continue; }
            try
            {
                var base64 = Convert.ToBase64String(File.ReadAllBytes(path));
                sb.Append("@font-face{font-family:\"").Append(Family).Append("\";font-weight:").Append(weight)
                  .Append(";font-style:normal;font-display:swap;src:url(\"data:font/ttf;base64,").Append(base64)
                  .Append("\") format(\"truetype\")}\n");
            }
            catch (Exception ex)
            {
                AppLog.Write("font embed failed: " + ex.Message);
            }
        }
        _embedded = sb.ToString();
        return _embedded;
    }

    /// <summary>@font-face rules that point at copied files, e.g. relativeDir = "fonts/" (export).</summary>
    public static string FileCss(string relativeDir)
    {
        var sb = new StringBuilder();
        foreach (var (file, weight) in Faces)
        {
            if (!File.Exists(Path.Combine(Dir, file))) continue;
            sb.Append("@font-face{font-family:\"").Append(Family).Append("\";font-weight:").Append(weight)
              .Append(";font-style:normal;font-display:swap;src:url(\"").Append(relativeDir).Append(file)
              .Append("\") format(\"truetype\")}\n");
        }
        return sb.ToString();
    }
}
