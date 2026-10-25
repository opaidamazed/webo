using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics;

namespace Webo.App;

public sealed partial class MainWindow : Window
{
    private bool _navigating;

    public MainWindow()
    {
        InitializeComponent();
        Settings.Load();

        AppWindow.Resize(new SizeInt32(1320, 820));

        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);
            AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
        else
        {
            TitleBar.Visibility = Visibility.Collapsed;
        }

        Root.ActualThemeChanged += (_, _) => UpdateCaptionButtons();

        ApplyTheme();
        ApplyLanguage();
        ApplyIcon();
        Nav.SelectedItem = NavHome;
    }

    // ---- public API used by pages -------------------------------------------------------------

    public void OpenProject(string file) => ContentFrame.Navigate(typeof(Pages.EditorPage), file);

    public void GoHome()
    {
        _navigating = true;
        Nav.SelectedItem = NavHome;
        _navigating = false;
        ContentFrame.Navigate(typeof(Pages.HomePage));
    }

    /// <summary>Applies the saved icon to the window and title bar. Returns null on success, else the error text.</summary>
    public string? ApplyIcon()
    {
        var id = Settings.Current.AppIcon;
        var err = IconService.ApplyToWindow(AppWindow, id);
        if (err != null) AppLog.Write("apply icon: " + err);
        try
        {
            var preview = IconService.PreviewPath(id);
            if (File.Exists(preview)) TitleIcon.Source = new BitmapImage(new Uri(preview));
        }
        catch (Exception ex)
        {
            AppLog.Write("title icon preview failed: " + ex.Message);
        }
        return err;
    }

    public void ApplyTheme()
    {
        Root.RequestedTheme = Settings.Current.Theme switch
        {
            "light" => ElementTheme.Light,
            "dark" => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };
        UpdateCaptionButtons();
    }

    public void ApplyLanguage()
    {
        Loc.Lang = Settings.Current.Language;
        Root.FlowDirection = Loc.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        Title = "Webo";
        TitleText.Text = "Webo";
        NavHome.Content = Loc.T("nav.home");
        NavTemplates.Content = Loc.T("nav.templates");
        NavSettings.Content = Loc.T("nav.settings");

        // Re-create the current page so every string is looked up again.
        if (ContentFrame.Content == null)
            ContentFrame.Navigate(typeof(Pages.HomePage));
        else
            ContentFrame.Navigate(ContentFrame.CurrentSourcePageType, ContentFrame.Content is Pages.EditorPage ep ? ep.ProjectFile : null);
    }

    // ---- internals ----------------------------------------------------------------------------

    private void UpdateCaptionButtons()
    {
        if (!AppWindowTitleBar.IsCustomizationSupported()) return;
        var dark = Root.ActualTheme == ElementTheme.Dark;
        AppWindow.TitleBar.ButtonForegroundColor = dark ? Colors.White : Colors.Black;
        AppWindow.TitleBar.ButtonHoverForegroundColor = dark ? Colors.White : Colors.Black;
        AppWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
    }

    private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_navigating) return;
        if (args.SelectedItem is not NavigationViewItem item) return;
        switch (item.Tag as string)
        {
            case "home": ContentFrame.Navigate(typeof(Pages.HomePage)); break;
            case "settings": ContentFrame.Navigate(typeof(Pages.SettingsPage)); break;
        }
    }
}
