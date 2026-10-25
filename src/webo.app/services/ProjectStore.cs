using System.Text;
using System.Text.Json;
using Webo.App.Models;

namespace Webo.App;

public static class ProjectStore
{
    public const string FileName = "project.webo.json";
    public static readonly string[] SiteTypes = { "business", "portfolio", "restaurant", "education", "landing", "blog" };

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public static string DefaultParentDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Webo Projects");

    private static string RecentFile => Path.Combine(Settings.Dir, "recent.json");

    public static bool IsValidFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;
        if (name.TrimEnd().EndsWith('.')) return false;
        var reserved = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "LPT1", "LPT2", "LPT3" };
        return !reserved.Contains(name.Trim().ToUpperInvariant());
    }

    /// <summary>Creates a new project folder. Never writes into an existing folder (no silent overwrite).</summary>
    public static string Create(string name, string siteType, string language, string parentDir)
    {
        name = name.Trim();
        var folder = Path.Combine(parentDir, name);
        if (Directory.Exists(folder) || File.Exists(folder))
            throw new IOException("exists");

        Directory.CreateDirectory(folder);
        var doc = new ProjectDoc
        {
            Name = name,
            SiteType = siteType,
            Language = language,
            Direction = language == "ar" ? "rtl" : "ltr",
        };
        doc.Pages.Add(new PageDoc { Title = language == "ar" ? "الرئيسية" : "Home", Slug = "index", IsHome = true });

        var file = Path.Combine(folder, FileName);
        Write(file, doc, makeBackup: false);
        AddRecent(file, doc);
        return file;
    }

    public static ProjectDoc Load(string file)
    {
        var doc = JsonSerializer.Deserialize<ProjectDoc>(File.ReadAllText(file, Encoding.UTF8))
                  ?? throw new InvalidDataException("empty project file");
        if (doc.Pages.Count == 0)
            doc.Pages.Add(new PageDoc { Title = "Home", Slug = "index", IsHome = true });
        if (!doc.Pages.Any(p => p.IsHome)) doc.Pages[0].IsHome = true;
        return doc;
    }

    /// <summary>Atomic save: write a temp file, keep the previous version as .bak, then swap.</summary>
    public static void Save(string file, ProjectDoc doc)
    {
        doc.ModifiedAt = DateTime.Now;
        Write(file, doc, makeBackup: true);
        AddRecent(file, doc);
    }

    private static void Write(string file, ProjectDoc doc, bool makeBackup)
    {
        var tmp = file + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(doc, Json), new UTF8Encoding(false));
        if (File.Exists(file))
        {
            if (makeBackup) File.Copy(file, file + ".bak", overwrite: true);
            File.Move(tmp, file, overwrite: true);
        }
        else
        {
            File.Move(tmp, file);
        }
    }

    public static List<RecentProject> LoadRecent()
    {
        try
        {
            if (File.Exists(RecentFile))
            {
                var list = JsonSerializer.Deserialize<List<RecentProject>>(File.ReadAllText(RecentFile));
                if (list != null) return list.OrderByDescending(r => r.OpenedAt).ToList();
            }
        }
        catch (Exception ex)
        {
            AppLog.Write("recent load failed: " + ex.Message);
        }
        return new List<RecentProject>();
    }

    private static void SaveRecent(List<RecentProject> list)
    {
        try
        {
            Directory.CreateDirectory(Settings.Dir);
            File.WriteAllText(RecentFile, JsonSerializer.Serialize(list.Take(30).ToList(), Json));
        }
        catch (Exception ex)
        {
            AppLog.Write("recent save failed: " + ex.Message);
        }
    }

    public static void AddRecent(string file, ProjectDoc doc)
    {
        var list = LoadRecent();
        list.RemoveAll(r => string.Equals(r.FilePath, file, StringComparison.OrdinalIgnoreCase));
        list.Insert(0, new RecentProject { FilePath = file, Name = doc.Name, SiteType = doc.SiteType, OpenedAt = DateTime.Now });
        SaveRecent(list);
    }

    /// <summary>Removes the entry from the recent list only. Project files are never deleted.</summary>
    public static void RemoveRecent(string file)
    {
        var list = LoadRecent();
        list.RemoveAll(r => string.Equals(r.FilePath, file, StringComparison.OrdinalIgnoreCase));
        SaveRecent(list);
    }

    public static string MakeSlug(string title, ISet<string> taken)
    {
        var sb = new StringBuilder();
        foreach (var ch in title.Trim().ToLowerInvariant())
        {
            if (ch is >= 'a' and <= 'z' or >= '0' and <= '9') sb.Append(ch);
            else if (ch is ' ' or '-' or '_') sb.Append('-');
        }
        var slug = sb.ToString().Trim('-');
        if (slug.Length == 0) slug = "page";
        var candidate = slug;
        var n = 2;
        while (taken.Contains(candidate)) candidate = $"{slug}-{n++}";
        return candidate;
    }
}
