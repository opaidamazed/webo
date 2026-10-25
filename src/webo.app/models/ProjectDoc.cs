namespace Webo.App.Models;

/// <summary>
/// Structured project model (requirements section 8): the project is data, not hand-edited HTML.
/// Stored as project.webo.json inside the project folder.
/// </summary>
public sealed class ProjectDoc
{
    public int Schema { get; set; } = 1;
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string SiteType { get; set; } = "business";
    public string Language { get; set; } = "ar";      // site language, independent of the app UI language
    public string Direction { get; set; } = "rtl";    // rtl | ltr
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    public List<PageDoc> Pages { get; set; } = new();
}

public sealed class PageDoc : System.ComponentModel.INotifyPropertyChanged
{
    private string _title = "";
    private bool _isHome;

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Slug { get; set; } = "";

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
