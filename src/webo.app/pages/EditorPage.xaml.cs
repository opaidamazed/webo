using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using Webo.App.Models;

namespace Webo.App.Pages;

/// <summary>
/// Editor (v0.3). What works: page list, sections (add / reorder / duplicate / delete), a REAL page preview rendered
/// by <see cref="SiteRenderer"/> inside WebView2 with click-to-select, a field panel per section type, undo/redo,
/// device widths, atomic save with .bak and autosave. NOT implemented yet: templates, export, testing, images.
/// </summary>
public sealed partial class EditorPage : Page
{
    private ProjectDoc? _doc;
    private string? _file;
    private readonly ObservableCollection<PageDoc> _pages = new();
    private bool _dirty;
    private bool _updatingUi;
    private bool _restoring;   // true while undo/redo swaps the page list, so selection events are ignored
    private bool _closed;
    private DispatcherTimerHolder? _timer;
    private EditHistory? _history;

    // preview state
    private bool _previewLoaded;
    private bool _previewFailed;
    private bool _pendingRefresh;

    public string? ProjectFile => _file;

    public EditorPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _file = e.Parameter as string;

        ApplyStrings();

        if (_file == null)
        {
            App.Shell?.GoHome();
            return;
        }

        try
        {
            _doc = ProjectStore.Load(_file);
        }
        catch (Exception ex)
        {
            AppLog.Write("open project failed: " + ex);
            await Dialogs.InfoAsync(XamlRoot, Loc.T("home.open"), Loc.F("ed.error.load", ex.Message));
            App.Shell?.GoHome();
            return;
        }

        ProjectStore.AddRecent(_file, _doc);
        ProjectName.Text = _doc.Name;
        FolderText.Text = Loc.F("ed.folder", Path.GetDirectoryName(_file) ?? "");

        _doc.Theme ??= new ThemeDoc();
        SyncIdentityUi();

        foreach (var p in _doc.Pages) _pages.Add(p);
        PagesList.ItemsSource = _pages;
        _history = new EditHistory(Snapshot());

        PagesList.SelectedIndex = Math.Max(0, _pages.ToList().FindIndex(p => p.IsHome));
        UpdateSaveState();
        UpdateUndoRedo();

        _timer = new DispatcherTimerHolder(TimeSpan.FromSeconds(15), () => { if (_dirty) TrySave(); });
        if (App.Shell != null) App.Shell.Closed += Shell_Closed;

        await InitPreviewAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _closed = true;
        _timer?.Stop();
        if (App.Shell != null) App.Shell.Closed -= Shell_Closed;
        if (_dirty) TrySave();
        try { Preview.Close(); }
        catch (Exception ex) { AppLog.Write("preview close failed: " + ex.Message); }
    }

    private void Shell_Closed(object sender, Microsoft.UI.Xaml.WindowEventArgs args)
    {
        if (_dirty) TrySave();
    }

    private void ApplyStrings()
    {
        DevDesktopText.Text = Loc.T("ed.desktop");
        DevTabletText.Text = Loc.T("ed.tablet");
        DevMobileText.Text = Loc.T("ed.mobile");
        Tip(UndoBtn, Loc.T("ed.undo"));
        Tip(RedoBtn, Loc.T("ed.redo"));
        TestBtn.Content = Loc.T("ed.test");
        ExportBtn.Content = Loc.T("ed.export");
        SaveBtn.Content = Loc.T("ed.save");
        PagesHeader.Text = Loc.T("ed.pages");
        Tip(AddPageBtn, Loc.T("ed.addpage"));
        PropsHeader.Text = Loc.T("ed.props");
        TitleBox.Header = Loc.T("ed.pagetitle");
        SlugBox.Header = Loc.T("ed.slug");
        SetHomeBtn.Content = Loc.T("ed.sethome");
        DeleteBtn.Content = Loc.T("ed.delete");

        IdentityExpander.Header = Loc.T("ed.identity");
        PaletteBox.Header = Loc.T("ed.palette");
        AccentBox.Header = Loc.T("ed.accent");
        FontBox.Header = Loc.T("ed.font");
        RadiusBox.Header = Loc.T("ed.radius");
        _updatingUi = true;
        PaletteBox.Items.Clear();
        foreach (var id in ThemeCatalog.PaletteIds) PaletteBox.Items.Add(Loc.T("pal." + id));
        FontBox.Items.Clear();
        foreach (var id in ThemeCatalog.Fonts) FontBox.Items.Add(Loc.T("font." + id));
        RadiusBox.Items.Clear();
        foreach (var id in ThemeCatalog.Radii) RadiusBox.Items.Add(Loc.T("radius." + id));
        _updatingUi = false;

        Dialogs.ApplyFont(PaletteBox, FontBox, RadiusBox, AccentBox);
        Tip(RenameBtn, Loc.T("ed.rename"));
        SectionsHeader.Text = Loc.T("ed.sections");
        Tip(AddSectionBtn, Loc.T("ed.addsection"));
        Tip(MoveUpBtn, Loc.T("ed.sec.up"));
        Tip(MoveDownBtn, Loc.T("ed.sec.down"));
        Tip(DupSectionBtn, Loc.T("ed.sec.dup"));
        Tip(DelSectionBtn, Loc.T("ed.sec.del"));

        AddSectionMenu.Items.Clear();
        foreach (var type in SectionCatalog.Types)
        {
            var item = new MenuFlyoutItem { Text = Loc.T("sec." + type), Tag = type, FontFamily = Dialogs.WeboFont };
            item.Click += AddSection_Click;
            AddSectionMenu.Items.Add(item);
        }
    }

    /// <summary>A tooltip in the app font (tooltips are pop-ups and would otherwise use the system font).</summary>
    private static void Tip(DependencyObject target, string text) =>
        ToolTipService.SetToolTip(target, new ToolTip { Content = text, FontFamily = Dialogs.WeboFont });

    // ---- state --------------------------------------------------------------------------------

    private PageDoc? Current => PagesList.SelectedItem as PageDoc;
    private SectionDoc? CurrentSection => SectionsList.SelectedItem as SectionDoc;

    private void MarkDirty()
    {
        _dirty = true;
        UpdateSaveState();
    }

    private void UpdateSaveState() =>
        SaveState.Text = Loc.T(_dirty ? "ed.unsaved" : "ed.saved");

    private void UpdateUndoRedo()
    {
        UndoBtn.IsEnabled = _history?.CanUndo ?? false;
        RedoBtn.IsEnabled = _history?.CanRedo ?? false;
    }

    private bool TrySave()
    {
        if (_doc == null || _file == null) return false;
        try
        {
            _doc.Pages = _pages.ToList();
            ProjectStore.Save(_file, _doc);
            _dirty = false;
            UpdateSaveState();
            return true;
        }
        catch (Exception ex)
        {
            AppLog.Write("save failed: " + ex);
            SaveState.Text = Loc.F("ed.error.save", ex.Message);
            return false;
        }
    }

    // ---- undo / redo --------------------------------------------------------------------------

    private string Snapshot() =>
        JsonSerializer.Serialize(new EditorSnapshot
        {
            Name = _doc?.Name ?? "",
            Pages = _pages.ToList(),
            Theme = _doc?.Theme ?? new ThemeDoc(),
        });

    /// <summary>Call after every real edit. Records an undo step (merging rapid edits with the same key) and refreshes the preview.</summary>
    private void CommitEdit(string? mergeKey = null, bool css = false)
    {
        if (_history == null) return;
        if (!_history.Commit(Snapshot(), mergeKey)) return;
        MarkDirty();
        UpdateUndoRedo();
        RefreshPreview(includeCss: css);
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        var snapshot = _history?.Undo();
        if (snapshot != null) ApplySnapshot(snapshot);
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        var snapshot = _history?.Redo();
        if (snapshot != null) ApplySnapshot(snapshot);
    }

    private void ApplySnapshot(string json)
    {
        EditorSnapshot? restored;
        try
        {
            restored = JsonSerializer.Deserialize<EditorSnapshot>(json);
        }
        catch (Exception ex)
        {
            AppLog.Write("undo restore failed: " + ex.Message);
            return;
        }
        if (restored == null || restored.Pages.Count == 0) return;

        var pageId = Current?.Id;
        var sectionId = CurrentSection?.Id;

        _restoring = true;
        try
        {
            _pages.Clear();
            foreach (var p in restored.Pages) _pages.Add(p);
        }
        finally
        {
            _restoring = false;
        }

        if (_doc != null)
        {
            _doc.Theme = restored.Theme ?? new ThemeDoc();
            if (!string.IsNullOrWhiteSpace(restored.Name)) _doc.Name = restored.Name;
            ProjectName.Text = _doc.Name;
            SyncIdentityUi();
        }

        var page = _pages.FirstOrDefault(p => p.Id == pageId) ?? _pages.FirstOrDefault(p => p.IsHome) ?? _pages[0];
        PagesList.SelectedItem = page;
        SectionsList.SelectedItem = page.Sections.FirstOrDefault(s => s.Id == sectionId);

        MarkDirty();
        UpdateUndoRedo();
        RefreshPreview(includeCss: true);
    }

    // ---- toolbar ------------------------------------------------------------------------------

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (_dirty && !TrySave()) return; // never leave with unsaved changes if saving failed
        App.Shell?.GoHome();
    }

    private void Save_Click(object sender, RoutedEventArgs e) => TrySave();

    private void Device_Click(object sender, RoutedEventArgs e)
    {
        var tag = (sender as ToggleButton)?.Tag as string ?? "desktop";
        DevDesktop.IsChecked = tag == "desktop";
        DevTablet.IsChecked = tag == "tablet";
        DevMobile.IsChecked = tag == "mobile";
        // The page inside WebView2 is responsive (CSS media queries), so a narrower frame is a real mobile/tablet layout.
        DeviceFrame.Width = tag switch { "tablet" => 768, "mobile" => 375, _ => 1000 };
    }

    // ---- preview (WebView2) -------------------------------------------------------------------

    private RenderOptions RenderOpts() => new()
    {
        EditMode = true,
        SelectedSectionId = CurrentSection?.Id,
        EmptyHint = Loc.T("ed.preview.empty"),
        FontCss = FontAssets.EmbeddedCss(),
    };

    private async Task InitPreviewAsync()
    {
        try
        {
            // Unpackaged apps would otherwise put WebView2's profile next to the exe (often not writable): use LocalAppData.
            // The documented environment variable is used instead of CoreWebView2Environment.CreateAsync(...), whose overloads
            // differ between WebView2 SDK versions. It must be set before the first WebView2 of the process is created.
            var dataDir = Path.Combine(Settings.Dir, "WebView2");
            Directory.CreateDirectory(dataDir);
            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", dataDir);
            await Preview.EnsureCoreWebView2Async();
            if (_closed) return;

            var core = Preview.CoreWebView2;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.IsZoomControlEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.IsBuiltInErrorPageEnabled = false;
            core.WebMessageReceived += Preview_WebMessageReceived;
            core.NavigationStarting += Preview_NavigationStarting;
            core.NavigationCompleted += Preview_NavigationCompleted;
            core.NewWindowRequested += (_, args) => args.Handled = true;

            LoadPreviewDocument();
        }
        catch (Exception ex)
        {
            // Most likely the WebView2 runtime is missing. Editing and saving must keep working without it.
            AppLog.Write("preview init failed: " + ex);
            _previewFailed = true;
            Preview.Visibility = Visibility.Collapsed;
            PreviewError.Text = Loc.T("ed.preview.fail");
            PreviewError.Visibility = Visibility.Visible;
        }
    }

    /// <summary>Full (re)load of the preview document. Normal edits use <see cref="RefreshPreview"/> instead.</summary>
    private void LoadPreviewDocument()
    {
        if (_doc == null || Current is not { } page || Preview.CoreWebView2 == null || _closed) return;
        _previewLoaded = false;
        var html = SiteRenderer.RenderDocument(_doc, _pages, page, RenderOpts());
        Preview.CoreWebView2.NavigateToString(html);
    }

    private async void RefreshPreview(bool resetScroll = false, bool scrollToSelected = false, bool includeCss = false)
    {
        if (_doc == null || Current is not { } page || _previewFailed || _closed) return;
        if (Preview.CoreWebView2 == null || !_previewLoaded)
        {
            _pendingRefresh = true; // applied as soon as the document finishes loading
            return;
        }

        try
        {
            var html = SiteRenderer.RenderBody(_doc, _pages, page, RenderOpts());
            var scrollTo = scrollToSelected && CurrentSection != null ? JsonSerializer.Serialize(CurrentSection.Id) : "null";
            // The (small) theme CSS is only re-sent when the identity changed; the page body is swapped in place.
            var js = includeCss
                ? "window.weboSetTheme(" + JsonSerializer.Serialize(SiteRenderer.BuildThemeCss(_doc.Theme)) + ");"
                : "";
            js += "window.weboUpdate(" + JsonSerializer.Serialize(html) + "," + (resetScroll ? "true" : "false") + "," + scrollTo + ");";
            await Preview.CoreWebView2.ExecuteScriptAsync(js);
        }
        catch (Exception ex)
        {
            AppLog.Write("preview update failed: " + ex.Message);
        }
    }

    private void Preview_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
    {
        // The preview is a closed document: it must never navigate to the web or to local files.
        var uri = args.Uri ?? "";
        if (uri.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
            uri.StartsWith("file", StringComparison.OrdinalIgnoreCase) ||
            uri.StartsWith("ftp", StringComparison.OrdinalIgnoreCase))
            args.Cancel = true;
    }

    private void Preview_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
        _previewLoaded = args.IsSuccess;
        if (!args.IsSuccess) AppLog.Write("preview navigation failed: " + args.WebErrorStatus);
        if (_previewLoaded && _pendingRefresh)
        {
            _pendingRefresh = false;
            RefreshPreview();
        }
    }

    private void Preview_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        string message;
        try { message = args.TryGetWebMessageAsString(); }
        catch { return; }

        if (message.StartsWith("select:", StringComparison.Ordinal))
        {
            var id = message.Substring("select:".Length);
            // An empty or unknown id clears the selection (click on empty space).
            SectionsList.SelectedItem = Current?.Sections.FirstOrDefault(s => s.Id == id);
        }
        else if (message.StartsWith("page:", StringComparison.Ordinal))
        {
            var id = message.Substring("page:".Length);
            var page = _pages.FirstOrDefault(p => p.Id == id);
            if (page != null) PagesList.SelectedItem = page;
        }
    }

    // ---- pages --------------------------------------------------------------------------------

    private void PagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_restoring) return;
        var page = Current;
        _updatingUi = true;
        TitleBox.IsEnabled = SlugBox.IsEnabled = SetHomeBtn.IsEnabled = DeleteBtn.IsEnabled = page != null;
        TitleBox.Text = page?.Title ?? "";
        SlugBox.Text = page?.Slug ?? "";
        SetHomeBtn.IsEnabled = page != null && !page.IsHome;
        SectionsList.ItemsSource = page?.Sections; // also clears the section selection
        AddSectionBtn.IsEnabled = page != null;
        _updatingUi = false;

        UpdateSectionButtons();
        BuildSectionFields(CurrentSection);
        RefreshPreview(resetScroll: true);
    }

    private void AddPage_Click(object sender, RoutedEventArgs e)
    {
        var language = _doc?.Language ?? "ar";
        var taken = new HashSet<string>(_pages.Select(p => p.Slug), StringComparer.OrdinalIgnoreCase);
        var n = _pages.Count + 1;
        var page = new PageDoc { Title = $"{Loc.T("ed.newpage")} {n}", Slug = ProjectStore.MakeSlug($"page-{n}", taken) };
        page.Sections.Add(SectionCatalog.Create("text", language));
        page.Sections.Add(SectionCatalog.Create("footer", language));
        _pages.Add(page);
        PagesList.SelectedItem = page;
        CommitEdit();
    }

    private void TitleBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updatingUi || Current is not { } page) return;
        page.Title = TitleBox.Text;
        CommitEdit("title:" + page.Id);
    }

    private void SlugBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (Current is not { } page) return;
        var taken = new HashSet<string>(_pages.Where(p => p != page).Select(p => p.Slug), StringComparer.OrdinalIgnoreCase);
        var slug = ProjectStore.MakeSlug(SlugBox.Text, taken);
        if (page.IsHome && string.IsNullOrWhiteSpace(SlugBox.Text)) slug = "index";
        if (slug != page.Slug)
        {
            page.Slug = slug;
            CommitEdit();
        }
        SlugBox.Text = page.Slug;
    }

    private void SetHome_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } page) return;
        foreach (var p in _pages) p.IsHome = p == page;
        SetHomeBtn.IsEnabled = false;
        CommitEdit();
    }

    private async void DeletePage_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } page) return;
        if (_pages.Count <= 1)
        {
            await Dialogs.InfoAsync(XamlRoot, Loc.T("ed.delete"), Loc.T("ed.delete.last"));
            return;
        }
        if (!await Dialogs.ConfirmAsync(XamlRoot, Loc.T("ed.delete"), Loc.F("ed.delete.confirm", page.Title))) return;

        var index = _pages.IndexOf(page);
        var wasHome = page.IsHome;
        _pages.Remove(page);
        if (wasHome) _pages[0].IsHome = true;
        PagesList.SelectedIndex = Math.Min(index, _pages.Count - 1);
        CommitEdit();
    }

    // ---- site identity ------------------------------------------------------------------------

    private void SyncIdentityUi()
    {
        if (_doc == null) return;
        var t = _doc.Theme;
        _updatingUi = true;
        PaletteBox.SelectedIndex = Math.Max(0, Array.IndexOf(ThemeCatalog.PaletteIds, t.Palette));
        FontBox.SelectedIndex = Math.Max(0, Array.IndexOf(ThemeCatalog.Fonts, t.Font));
        RadiusBox.SelectedIndex = Math.Max(0, Array.IndexOf(ThemeCatalog.Radii, t.Radius));
        AccentBox.Text = t.Accent;
        AccentBox.IsEnabled = t.Palette == ThemeCatalog.Custom;
        _updatingUi = false;
    }

    private void Identity_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingUi || _doc == null) return;
        var t = _doc.Theme;
        var palette = PaletteBox.SelectedIndex >= 0 ? ThemeCatalog.PaletteIds[PaletteBox.SelectedIndex] : t.Palette;
        var font = FontBox.SelectedIndex >= 0 ? ThemeCatalog.Fonts[FontBox.SelectedIndex] : t.Font;
        var radius = RadiusBox.SelectedIndex >= 0 ? ThemeCatalog.Radii[RadiusBox.SelectedIndex] : t.Radius;
        if (palette == t.Palette && font == t.Font && radius == t.Radius) return; // late event, nothing changed

        t.Palette = palette;
        t.Font = font;
        t.Radius = radius;
        AccentBox.IsEnabled = palette == ThemeCatalog.Custom;
        CommitEdit(css: true);
    }

    private void AccentBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (_doc == null) return;
        var hex = ThemeCatalog.NormalizeHex(AccentBox.Text);
        if (hex == null)
        {
            AccentBox.Text = _doc.Theme.Accent; // not a valid #RRGGBB: put the last good value back
            return;
        }
        AccentBox.Text = hex;
        if (hex == _doc.Theme.Accent) return;
        _doc.Theme.Accent = hex;
        CommitEdit(css: true);
    }

    // ---- rename -------------------------------------------------------------------------------

    private async void Rename_Click(object sender, RoutedEventArgs e)
    {
        if (_doc == null) return;
        var answer = await Dialogs.PromptAsync(XamlRoot, Loc.T("ed.rename"), Loc.T("ed.rename.label"), _doc.Name, 80, Loc.T("ed.rename.note"));
        if (answer == null) return; // cancelled

        var name = answer.Trim();
        if (name.Length == 0)
        {
            await Dialogs.InfoAsync(XamlRoot, Loc.T("ed.rename"), Loc.T("ed.rename.empty"));
            return;
        }
        if (name == _doc.Name) return;

        _doc.Name = name;
        ProjectName.Text = name;
        CommitEdit();   // undoable, and the site name in the preview header updates
        TrySave();      // so the Home page's recent list shows the new name right away
    }

    // ---- export -------------------------------------------------------------------------------

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (_doc == null || _file == null) return;
        if (_dirty && !TrySave()) return; // export what is on disk, so the project and the site never disagree

        ExportResult result;
        try
        {
            result = SiteExporter.Export(_doc, _pages.ToList(), _file);
        }
        catch (Exception ex)
        {
            AppLog.Write("export failed: " + ex);
            await Dialogs.InfoAsync(XamlRoot, Loc.T("ed.export"), Loc.F("ed.export.fail", ex.Message));
            return;
        }

        var message = Loc.F("ed.export.done", result.PageCount, result.Folder);
        if (result.Warnings.Count > 0)
        {
            const int max = 10;
            message += "\n\n" + Loc.T("ed.export.warn") + "\n• " + string.Join("\n• ", result.Warnings.Take(max));
            if (result.Warnings.Count > max) message += "\n" + Loc.F("ed.export.more", result.Warnings.Count - max);
        }
        message += "\n\n" + Loc.T("ed.export.open");

        if (!await Dialogs.ConfirmAsync(XamlRoot, Loc.T("ed.export"), message)) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = result.Folder, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            AppLog.Write("open export folder failed: " + ex.Message);
        }
    }

    // ---- sections -----------------------------------------------------------------------------

    private void SectionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_restoring) return;
        UpdateSectionButtons();
        BuildSectionFields(CurrentSection);
        RefreshPreview(scrollToSelected: true);
    }

    private void UpdateSectionButtons()
    {
        var page = Current;
        var sec = CurrentSection;
        var index = page != null && sec != null ? page.Sections.IndexOf(sec) : -1;
        MoveUpBtn.IsEnabled = index > 0;
        MoveDownBtn.IsEnabled = page != null && index >= 0 && index < page.Sections.Count - 1;
        DupSectionBtn.IsEnabled = DelSectionBtn.IsEnabled = sec != null;
    }

    /// <summary>Builds one text box per field of the selected section's type. Edits go straight into the model.</summary>
    private void BuildSectionFields(SectionDoc? sec)
    {
        SectionFields.Children.Clear();
        SectionPropsPanel.Visibility = sec == null ? Visibility.Collapsed : Visibility.Visible;
        if (sec == null) return;

        SectionPropsHeader.Text = Loc.T("ed.sec.props") + " — " + sec.TypeLabel;
        foreach (var field in SectionCatalog.Fields(sec.Type))
        {
            var key = field.Key;
            var box = new TextBox
            {
                Header = Loc.T(field.LabelKey) + (field.Number > 0 ? " " + field.Number : ""),
                Text = sec.Get(key),
                MaxLength = 2000,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = field.Multiline,
                MinHeight = field.Multiline ? 96 : 0,
            };
            if (field.IsLink) box.FlowDirection = FlowDirection.LeftToRight;
            // The handler is attached after Text is set, so building the panel never counts as an edit.
            box.TextChanged += (_, _) =>
            {
                if (_restoring) return;
                sec.Set(key, box.Text);
                CommitEdit("sec:" + sec.Id + ":" + key);
            };
            SectionFields.Children.Add(box);
        }
    }

    private void AddSection_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } page) return;
        if ((sender as FrameworkElement)?.Tag is not string type) return;

        var sec = SectionCatalog.Create(type, _doc?.Language ?? "ar");
        var index = CurrentSection != null ? page.Sections.IndexOf(CurrentSection) + 1 : page.Sections.Count;
        page.Sections.Insert(index, sec);
        SectionsList.SelectedItem = sec;
        CommitEdit();
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e) => MoveSection(-1);
    private void MoveDown_Click(object sender, RoutedEventArgs e) => MoveSection(1);

    private void MoveSection(int delta)
    {
        if (Current is not { } page || CurrentSection is not { } sec) return;
        var from = page.Sections.IndexOf(sec);
        var to = from + delta;
        if (from < 0 || to < 0 || to >= page.Sections.Count) return;

        page.Sections.Move(from, to);
        SectionsList.SelectedItem = sec;
        UpdateSectionButtons();
        CommitEdit();
    }

    private void DupSection_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } page || CurrentSection is not { } sec) return;
        SectionDoc? copy;
        try
        {
            copy = JsonSerializer.Deserialize<SectionDoc>(JsonSerializer.Serialize(sec));
        }
        catch (Exception ex)
        {
            AppLog.Write("duplicate section failed: " + ex.Message);
            return;
        }
        if (copy == null) return;

        copy.Id = Guid.NewGuid().ToString("N");
        page.Sections.Insert(page.Sections.IndexOf(sec) + 1, copy);
        SectionsList.SelectedItem = copy;
        CommitEdit();
    }

    private async void DelSection_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } page || CurrentSection is not { } sec) return;
        if (!await Dialogs.ConfirmAsync(XamlRoot, Loc.T("ed.sec.del"), Loc.T("ed.sec.del.confirm"))) return;

        var index = page.Sections.IndexOf(sec);
        page.Sections.Remove(sec);
        if (page.Sections.Count > 0) SectionsList.SelectedIndex = Math.Min(index, page.Sections.Count - 1);
        CommitEdit();
    }

    // ---- autosave timer wrapper ---------------------------------------------------------------

    private sealed class DispatcherTimerHolder
    {
        private readonly Microsoft.UI.Xaml.DispatcherTimer _t = new();

        public DispatcherTimerHolder(TimeSpan interval, Action tick)
        {
            _t.Interval = interval;
            _t.Tick += (_, _) => tick();
            _t.Start();
        }

        public void Stop() => _t.Stop();
    }
}
