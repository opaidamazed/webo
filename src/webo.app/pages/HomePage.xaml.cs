using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Webo.App.Models;
using Windows.Storage.Pickers;

namespace Webo.App.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        WelcomeText.Text = Loc.T("home.welcome");
        SubText.Text = Loc.T("home.sub");
        NewText.Text = Loc.T("home.new");
        OpenText.Text = Loc.T("home.open");
        HonestText.Text = Loc.T("home.honest");
        RecentHeader.Text = Loc.T("home.recent");
        EmptyTitle.Text = Loc.T("home.empty.title");
        EmptyBody.Text = Loc.T("home.empty.body");
        RemoveNote.Text = Loc.T("home.remove.note");
        Reload();
    }

    private void Reload()
    {
        var list = ProjectStore.LoadRecent();
        RecentHost.Children.Clear();
        foreach (var r in list) RecentHost.Children.Add(BuildCard(r));

        var has = list.Count > 0;
        RecentHost.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
        RemoveNote.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
        EmptyState.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
    }

    private UIElement BuildCard(RecentProject r)
    {
        var res = Application.Current.Resources;

        var icon = new FontIcon { Glyph = "\uE774", FontSize = 20, Style = (Style)res["WeboAccentIcon"] };
        var tile = new Border { Style = (Style)res["WeboIconTile"], Child = icon };

        var texts = new StackPanel { Spacing = 2, VerticalAlignment = VerticalAlignment.Center };
        texts.Children.Add(new TextBlock { Text = r.Name, FontSize = 16, Style = (Style)res["WeboLabelText"] });
        texts.Children.Add(new TextBlock { Text = r.TypeLabel, Style = (Style)res["WeboMutedText"] });
        texts.Children.Add(new TextBlock
        {
            Text = r.Folder,
            Style = (Style)res["WeboMutedText"],
            FlowDirection = FlowDirection.LeftToRight,
            TextTrimming = TextTrimming.CharacterEllipsis,
            TextWrapping = TextWrapping.NoWrap,
        });

        var modified = new TextBlock { Text = r.ModifiedText, Style = (Style)res["WeboMutedText"], VerticalAlignment = VerticalAlignment.Center };

        var inner = new Grid { ColumnSpacing = 14 };
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        inner.Children.Add(tile);
        Grid.SetColumn(texts, 1);
        inner.Children.Add(texts);
        Grid.SetColumn(modified, 2);
        inner.Children.Add(modified);

        var open = new Button
        {
            Content = inner,
            Tag = r.FilePath,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent),
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
        };
        open.Click += async (_, _) => await OpenFileAsync(r.FilePath);

        var remove = new Button
        {
            Content = new FontIcon { Glyph = "\uE711", FontSize = 12 },
            Tag = r.FilePath,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(10, 8, 10, 8),
        };
        ToolTipService.SetToolTip(remove, Loc.T("home.remove"));
        remove.Click += Remove_Click;

        var row = new Grid { ColumnSpacing = 10 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.Children.Add(open);
        Grid.SetColumn(remove, 1);
        row.Children.Add(remove);

        return new Border { Style = (Style)res["WeboCard"], Padding = new Thickness(16, 14, 16, 14), Child = row };
    }

    private async void New_Click(object sender, RoutedEventArgs e)
    {
        var file = await NewProjectDialog.ShowAsync(XamlRoot);
        if (file != null) App.Shell?.OpenProject(file);
    }

    private async void Open_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".json");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, Dialogs.WindowHandle);
            var f = await picker.PickSingleFileAsync();
            if (f == null) return;
            await OpenFileAsync(f.Path);
        }
        catch (Exception ex)
        {
            AppLog.Write("open picker failed: " + ex);
            await Dialogs.InfoAsync(XamlRoot, Loc.T("home.open"), ex.Message);
        }
    }

    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string path })
        {
            ProjectStore.RemoveRecent(path); // list entry only; project files are never deleted
            Reload();
        }
    }

    private async Task OpenFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            await Dialogs.InfoAsync(XamlRoot, Loc.T("home.open"), Loc.F("home.missing", path));
            return;
        }
        App.Shell?.OpenProject(path);
    }
}
