using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage.Pickers;

namespace Webo.App;

public static class NewProjectDialog
{
    /// <summary>Returns the path of the created project file, or null if cancelled.</summary>
    public static async Task<string?> ShowAsync(XamlRoot root)
    {
        var name = new TextBox { Header = Loc.T("np.name"), MaxLength = 80 };
        var type = new ComboBox
        {
            Header = Loc.T("np.type"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource = ProjectStore.SiteTypes.Select(t => Loc.T("type." + t)).ToList(),
            SelectedIndex = 0,
        };
        var lang = new ComboBox
        {
            Header = Loc.T("np.lang"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            ItemsSource = new List<string> { Loc.T("np.site.rtl"), Loc.T("np.site.ltr") },
            SelectedIndex = Loc.IsArabic ? 0 : 1,
        };
        var location = new TextBox
        {
            Header = Loc.T("np.location"),
            Text = ProjectStore.DefaultParentDir,
            FlowDirection = FlowDirection.LeftToRight,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        var browse = new Button { Content = Loc.T("np.browse"), VerticalAlignment = VerticalAlignment.Bottom };
        var error = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Foreground = new SolidColorBrush(Colors.IndianRed),
            Visibility = Visibility.Collapsed,
        };

        browse.Click += async (_, _) =>
        {
            try
            {
                var picker = new FolderPicker();
                picker.FileTypeFilter.Add("*");
                WinRT.Interop.InitializeWithWindow.Initialize(picker, Dialogs.WindowHandle);
                var folder = await picker.PickSingleFolderAsync();
                if (folder != null) location.Text = folder.Path;
            }
            catch (Exception ex)
            {
                AppLog.Write("folder picker failed: " + ex.Message);
            }
        };

        var locRow = new Grid { ColumnSpacing = 8 };
        locRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        locRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        locRow.Children.Add(location);
        Grid.SetColumn(browse, 1);
        locRow.Children.Add(browse);

        var panel = new StackPanel { Spacing = 14, MinWidth = 440 };
        panel.Children.Add(name);
        panel.Children.Add(type);
        panel.Children.Add(lang);
        panel.Children.Add(locRow);
        panel.Children.Add(error);

        var dialog = new ContentDialog
        {
            XamlRoot = root,
            Title = Loc.T("np.title"),
            Content = panel,
            PrimaryButtonText = Loc.T("np.create"),
            CloseButtonText = Loc.T("np.cancel"),
            DefaultButton = ContentDialogButton.Primary,
            FlowDirection = Loc.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
        };

        string? created = null;

        void Fail(string message)
        {
            error.Text = message;
            error.Visibility = Visibility.Visible;
        }

        dialog.PrimaryButtonClick += (_, e) =>
        {
            error.Visibility = Visibility.Collapsed;
            var n = name.Text.Trim();
            if (n.Length == 0) { Fail(Loc.T("np.err.name")); e.Cancel = true; return; }
            if (!ProjectStore.IsValidFolderName(n)) { Fail(Loc.T("np.err.chars")); e.Cancel = true; return; }

            var parent = location.Text.Trim();
            if (parent.Length == 0 || parent.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || !Path.IsPathRooted(parent))
            {
                Fail(Loc.T("np.err.loc")); e.Cancel = true; return;
            }

            try
            {
                Directory.CreateDirectory(parent);
                var siteType = ProjectStore.SiteTypes[Math.Max(0, type.SelectedIndex)];
                var siteLang = lang.SelectedIndex == 1 ? "en" : "ar";
                created = ProjectStore.Create(n, siteType, siteLang, parent);
            }
            catch (IOException ex) when (ex.Message == "exists")
            {
                Fail(Loc.T("np.err.exists")); e.Cancel = true;
            }
            catch (Exception ex)
            {
                AppLog.Write("create project failed: " + ex);
                Fail(Loc.F("np.err.fail", ex.Message)); e.Cancel = true;
            }
        };

        await dialog.ShowAsync();
        return created;
    }
}
