namespace Webo.App.Models;

/// <summary>
/// Structured project model (requirements section 8): the project is data, not hand-edited HTML.
/// Stored as project.webo.json inside the project folder.
/// </summary>
public sealed class ProjectDoc
{
    public int Schema { get; set; } = 3;
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string SiteType { get; set; } = "business";
    public string Language { get; set; } = "ar";      // site language, independent of the app UI language
    public string Direction { get; set; } = "rtl";    // rtl | ltr
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    public List<PageDoc> Pages { get; set; } = new();

    /// <summary>Site-wide look (schema 3). Projects from older versions simply get the defaults.</summary>
    public ThemeDoc Theme { get; set; } = new();
}

/// <summary>Site identity: one accent palette, one font and one corner style for the whole site.</summary>
public sealed class ThemeDoc
{
    public string Palette { get; set; } = "ocean";   // ocean | emerald | sunset | royal | rose | slate | custom
    public string Accent { get; set; } = "#2090F0";  // only used when Palette == "custom"
    public string Font { get; set; } = "plex";       // plex | system | serif
    public string Radius { get; set; } = "medium";   // sharp | medium | round
}

/// <summary>What undo/redo stores: everything the editor can change.</summary>
public sealed class EditorSnapshot
{
    public string Name { get; set; } = "";
    public List<PageDoc> Pages { get; set; } = new();
    public ThemeDoc Theme { get; set; } = new();
}

public sealed class PageDoc : System.ComponentModel.INotifyPropertyChanged
{
    private string _title = "";
    private bool _isHome;

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Slug { get; set; } = "";

    /// <summary>The page content: an ordered list of sections (schema 2). Older projects simply have none.</summary>
    public System.Collections.ObjectModel.ObservableCollection<SectionDoc> Sections { get; set; } = new();

    public string Title
    {
        get => _title;
        set { if (_title != value) { _title = value; Raise(nameof(Title)); } }
    }

    public bool IsHome
    {
        get => _isHome;
        set { if (_isHome != value) { _isHome = value; Raise(nameof(IsHome)); Raise(nameof(HomeVisibility)); } }
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public Microsoft.UI.Xaml.Visibility HomeVisibility =>
        IsHome ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;

    private void Raise(string name) =>
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
}

/// <summary>
/// One block of a page (hero, text, features, call-to-action, footer). Content lives in <see cref="Props"/>
/// as plain strings; the renderer turns the section into HTML, never the other way round.
/// </summary>
public sealed class SectionDoc : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Type { get; set; } = "text";
    public Dictionary<string, string> Props { get; set; } = new();

    [System.Text.Json.Serialization.JsonIgnore] public string TypeLabel => Loc.T("sec." + Type);

    [System.Text.Json.Serialization.JsonIgnore]
    public string Glyph => Type switch
    {
        "hero" => "\uE91B",
        "text" => "\uE8D2",
        "features" => "\uE8FD",
        "cta" => "\uE71B",
        "testimonials" => "\uE734",
        "faq" => "\uE897",
        "contact" => "\uE715",
        _ => "\uE7C3",
    };

    /// <summary>Short one-line preview of the content, shown under the type name in the sections list.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string Summary
    {
        get
        {
            foreach (var key in new[] { "title", "heading", "text" })
            {
                var v = Get(key).Replace("\r", " ").Replace("\n", " ").Trim();
                if (v.Length == 0) continue;
                return v.Length > 40 ? v.Substring(0, 40) + "…" : v;
            }
            return "";
        }
    }

    public string Get(string key) => Props != null && Props.TryGetValue(key, out var v) ? v ?? "" : "";

    public void Set(string key, string value)
    {
        Props ??= new Dictionary<string, string>();
        Props[key] = value;
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Summary)));
    }
}

public sealed class RecentProject
{
    public string FilePath { get; set; } = "";
    public string Name { get; set; } = "";
    public string SiteType { get; set; } = "business";
    public DateTime OpenedAt { get; set; } = DateTime.Now;

    // Display-only helpers (not serialized meaningfully)
    [System.Text.Json.Serialization.JsonIgnore] public string TypeLabel => Loc.T("type." + SiteType);
    [System.Text.Json.Serialization.JsonIgnore] public string Folder => System.IO.Path.GetDirectoryName(FilePath) ?? FilePath;
    [System.Text.Json.Serialization.JsonIgnore] public string ModifiedText
    {
        get
        {
            try
            {
                var t = File.Exists(FilePath) ? File.GetLastWriteTime(FilePath) : OpenedAt;
                return Loc.F("home.modified", t.ToString("d MMM yyyy، HH:mm", Loc.Culture));
            }
            catch { return ""; }
        }
    }
}
