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
        Reload();
    }

    private void Reload()
    {
        var list = ProjectStore.LoadRecent();
        RecentList.ItemsSource = list;
        var has = list.Count > 0;
        RecentList.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
        RemoveNote.Visibility = has ? Visibility.Visible : Visibility.Collapsed;
        EmptyState.Visibility = has ? Visibility.Collapsed : Visibility.Visible;
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

    private async void RecentList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is RecentProject r) await OpenFileAsync(r.FilePath);
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
