using System.Text;
using System.Text.Json;
using Webo.App.Models;

namespace Webo.App;

public sealed class ExportResult
{
    public string Folder { get; set; } = "";
    public int PageCount { get; set; }
    public List<string> Warnings { get; } = new();
}

/// <summary>
/// Writes the finished site as plain static files (HTML + one CSS file + fonts) into the "export" folder inside the
/// project folder. Safety rules: it only ever deletes files that a previous export of THIS project wrote (listed in a
/// manifest), never anything else, and every file is written to a temp name first and then moved into place.
/// </summary>
public static class SiteExporter
{
    private const string ManifestName = ".webo-export.json";

    public static string ExportFolder(string projectFile) =>
        Path.Combine(Path.GetDirectoryName(projectFile) ?? throw new InvalidOperationException("project folder not found"), "export");

    public static ExportResult Export(ProjectDoc doc, IReadOnlyList<PageDoc> pages, string projectFile)
    {
        var folder = ExportFolder(projectFile);
        var assets = Path.Combine(folder, "assets");
        var fontsDir = Path.Combine(assets, "fonts");
        Directory.CreateDirectory(fontsDir);

        // 1) Remove only what the previous export wrote (pages that were renamed or deleted since then disappear).
        var manifestPath = Path.Combine(folder, ManifestName);
        foreach (var old in ReadManifest(manifestPath))
        {
            var full = InsideFolder(folder, old);
            if (full != null && File.Exists(full)) File.Delete(full);
        }

        var written = new List<string>();

        // 2) Fonts, then the shared stylesheet.
        foreach (var fontFile in FontAssets.Files)
        {
            var name = Path.GetFileName(fontFile);
            File.Copy(fontFile, Path.Combine(fontsDir, name), overwrite: true);
            written.Add("assets/fonts/" + name);
        }
        WriteText(Path.Combine(assets, "site.css"), SiteRenderer.BuildStylesheet(doc, FontAssets.FileCss("fonts/")));
        written.Add("assets/site.css");

        // 3) Pages. No script is included: the exported site works without JavaScript.
        var options = new RenderOptions { ExternalCssPath = "assets/site.css" };
        foreach (var page in pages)
        {
            var file = SiteRenderer.PageFileName(page);
            WriteText(Path.Combine(folder, file), SiteRenderer.RenderDocument(doc, pages, page, options));
            written.Add(file);
        }

        WriteText(manifestPath, JsonSerializer.Serialize(written, new JsonSerializerOptions { WriteIndented = true }));

        var result = new ExportResult { Folder = folder, PageCount = pages.Count };
        result.Warnings.AddRange(Check(doc, pages));
        return result;
    }

    /// <summary>Things worth fixing before the site is published. Never blocks the export: it only informs.</summary>
    public static List<string> Check(ProjectDoc doc, IReadOnlyList<PageDoc> pages)
    {
        var warnings = new List<string>();

        if (!pages.Any(p => p.IsHome)) warnings.Add(Loc.T("chk.nohome"));

        foreach (var group in pages.GroupBy(p => SiteRenderer.PageFileName(p), StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            warnings.Add(Loc.F("chk.dupfile", group.Key));

        foreach (var page in pages)
        {
            if (page.Sections.Count == 0)
            {
                warnings.Add(Loc.F("chk.empty", page.Title));
                continue;
            }

            foreach (var section in page.Sections)
            {
                if (SectionCatalog.IsStillDefault(section, doc.Language))
                {
                    // An untouched contact section is reported by the more specific message below.
                    if (section.Type != "contact") warnings.Add(Loc.F("chk.default", page.Title, section.TypeLabel));
                }

                if ((section.Type == "hero" || section.Type == "cta") &&
                    section.Get("btn").Trim().Length > 0 && SiteRenderer.SafeHref(section.Get("link")) == "#")
                    warnings.Add(Loc.F("chk.link", page.Title, section.TypeLabel));

                if (section.Type == "contact" &&
                    SiteRenderer.WhatsAppHref(section.Get("whatsapp")) == null &&
                    SiteRenderer.PhoneHref(section.Get("phone")) == null &&
                    SiteRenderer.EmailHref(section.Get("email")) == null)
                    warnings.Add(Loc.F("chk.contact", page.Title));
            }
        }
        return warnings;
    }

    private static List<string> ReadManifest(string path)
    {
        try
        {
            if (File.Exists(path))
                return JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path, Encoding.UTF8)) ?? new List<string>();
        }
        catch (Exception ex)
        {
            AppLog.Write("export manifest unreadable (ignored): " + ex.Message);
        }
        return new List<string>();
    }

    /// <summary>Resolves a relative manifest entry, refusing anything that would point outside the export folder.</summary>
    private static string? InsideFolder(string folder, string relative)
    {
        var root = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(folder, relative));
        return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? full : null;
    }

    private static void WriteText(string path, string content)
    {
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, content, new UTF8Encoding(false));
        File.Move(tmp, path, overwrite: true);
    }
}
