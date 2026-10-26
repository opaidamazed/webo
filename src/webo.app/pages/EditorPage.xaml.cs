using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;
using Webo.App.Models;

namespace Webo.App.Pages;

/// <summary>
/// Editor shell (early build). What really works: page list, add/rename/delete/set-home, device widths,
/// atomic save with a .bak backup, and autosave. The live page preview, element selection, undo/redo,
/// testing and export are NOT implemented yet and are labelled that way in the UI.
/// </summary>
public sealed partial class EditorPage : Page
{
    private ProjectDoc? _doc;
    private string? _file;
    private readonly List<PageDoc> _pages = new();
    private readonly Dictionary<PageDoc, TextBlock> _titleBlocks = new();
    private bool _rebuilding;
    private bool _dirty;
    private bool _updatingUi;
    private DispatcherTimerHolder? _timer;

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

        foreach (var p in _doc.Pages) _pages.Add(p);
        SitePreview.FlowDirection = _doc.Direction == "rtl" ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        CanvasDir.Text = Loc.T(_doc.Direction == "rtl" ? "ed.canvas.rtl" : "ed.canvas.ltr");
        CanvasNote.Text = Loc.T("ed.canvas.note");

        RebuildPages(_pages.FirstOrDefault(p => p.IsHome) ?? _pages[0]);
        UpdateSaveState();

        _timer = new DispatcherTimerHolder(TimeSpan.FromSeconds(15), () => { if (_dirty) TrySave(); });
        if (App.Shell != null) App.Shell.Closed += Shell_Closed;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _timer?.Stop();
        if (App.Shell != null) App.Shell.Closed -= Shell_Closed;
        if (_dirty) TrySave();
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
        ToolTipService.SetToolTip(UndoBtn, Loc.T("ed.undo"));
        ToolTipService.SetToolTip(RedoBtn, Loc.T("ed.redo"));
        TestBtn.Content = Loc.T("ed.test");
        ExportBtn.Content = Loc.T("ed.export");
        SaveBtn.Content = Loc.T("ed.save");
        PagesHeader.Text = Loc.T("ed.pages");
        ToolTipService.SetToolTip(AddPageBtn, Loc.T("ed.addpage"));
        PropsHeader.Text = Loc.T("ed.props");
        TitleBox.Header = Loc.T("ed.pagetitle");
        SlugBox.Header = Loc.T("ed.slug");
        SetHomeBtn.Content = Loc.T("ed.sethome");
        DeleteBtn.Content = Loc.T("ed.delete");
    }

    // ---- state --------------------------------------------------------------------------------

    private PageDoc? Current => (PagesList.SelectedItem as ListViewItem)?.Tag as PageDoc;

    private void MarkDirty()
    {
        _dirty = true;
        UpdateSaveState();
    }

    private void UpdateSaveState() =>
        SaveState.Text = Loc.T(_dirty ? "ed.unsaved" : "ed.saved");

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
        DeviceFrame.Width = tag switch { "tablet" => 768, "mobile" => 375, _ => 1000 };
    }

    // ---- pages --------------------------------------------------------------------------------

    /// <summary>Builds the page list and the preview nav chips from code (no data binding).</summary>
    private void RebuildPages(PageDoc? select)
    {
        _rebuilding = true;
        PagesList.Items.Clear();
        NavChips.Children.Clear();
        _titleBlocks.Clear();

        foreach (var p in _pages)
        {
            var title = new TextBlock { Text = p.Title, TextTrimming = TextTrimming.CharacterEllipsis };
            _titleBlocks[p] = title;

            var row = new Grid { ColumnSpacing = 8, Padding = new Thickness(2, 6, 2, 6) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var home = new FontIcon
            {
                Glyph = "\uE80F",
                FontSize = 14,
                Visibility = p.IsHome ? Visibility.Visible : Visibility.Collapsed,
                Style = (Style)Application.Current.Resources["WeboAccentIcon"],
            };
            row.Children.Add(home);
            Grid.SetColumn(title, 1);
            row.Children.Add(title);

            PagesList.Items.Add(new ListViewItem { Content = row, Tag = p });
            NavChips.Children.Add(new TextBlock
            {
                Text = p.Title,
                FontSize = 14,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0x20, 0x90, 0xF0)),
            });
        }
        _rebuilding = false;

        PagesList.SelectedIndex = select == null ? -1 : _pages.IndexOf(select);
    }

    private void PagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_rebuilding) return;
        var page = Current;
        _updatingUi = true;
        TitleBox.IsEnabled = SlugBox.IsEnabled = SetHomeBtn.IsEnabled = DeleteBtn.IsEnabled = page != null;
        TitleBox.Text = page?.Title ?? "";
        SlugBox.Text = page?.Slug ?? "";
        CanvasTitle.Text = page?.Title ?? "";
        SetHomeBtn.IsEnabled = page != null && !page.IsHome;
        _updatingUi = false;
    }

    private void AddPage_Click(object sender, RoutedEventArgs e)
    {
        var taken = new HashSet<string>(_pages.Select(p => p.Slug), StringComparer.OrdinalIgnoreCase);
        var n = _pages.Count + 1;
        var page = new PageDoc { Title = $"{Loc.T("ed.newpage")} {n}", Slug = ProjectStore.MakeSlug($"page-{n}", taken) };
        _pages.Add(page);
        RebuildPages(page);
        MarkDirty();
    }

    private void TitleBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_updatingUi || Current is not { } page) return;
        page.Title = TitleBox.Text;
        CanvasTitle.Text = TitleBox.Text;
        if (_titleBlocks.TryGetValue(page, out var tb)) tb.Text = page.Title;
        var i = _pages.IndexOf(page);
        if (i >= 0 && i < NavChips.Children.Count && NavChips.Children[i] is TextBlock chip) chip.Text = page.Title;
        MarkDirty();
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
            MarkDirty();
        }
        SlugBox.Text = page.Slug;
    }

    private void SetHome_Click(object sender, RoutedEventArgs e)
    {
        if (Current is not { } page) return;
        foreach (var p in _pages) p.IsHome = p == page;
        RebuildPages(page);
        MarkDirty();
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
        RebuildPages(_pages[Math.Min(index, _pages.Count - 1)]);
        MarkDirty();
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
