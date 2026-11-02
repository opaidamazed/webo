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
    private string? _appliedLang; // language the current page was last built in

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

        Root.ActualThemeChanged += (_, _) => { UpdateCaptionButtons(); UpdateTitleIcon(); };

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

    /// <summary>
    /// The window/taskbar icon is fixed (the original dark one, same as the exe and the shortcuts).
    /// Only the image inside the app's own title bar follows the theme.
    /// </summary>
    public void ApplyIcon()
    {
        var err = IconService.ApplyToWindow(AppWindow);
        if (err != null) AppLog.Write("apply icon: " + err);
        UpdateTitleIcon();
    }

    /// <summary>In-app only: light theme shows the light icon, dark theme shows the dark icon.</summary>
    public string CurrentInAppIconId => IconService.InAppId(Root.ActualTheme == ElementTheme.Dark);

    private void UpdateTitleIcon()
    {
        try
        {
            var preview = IconService.PreviewPath(CurrentInAppIconId);
            if (File.Exists(preview)) TitleIcon.Source = new BitmapImage(new Uri(preview));
        }
        catch (Exception ex)
        {
            AppLog.Write("title icon failed: " + ex.Message);
        }
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
        UpdateTitleIcon();
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

        // Re-create the current page so every string is looked up again, but only when the language really changed.
        // (Second line of defence: a page raising this repeatedly can no longer cause a navigation loop.)
        var langChanged = _appliedLang != Loc.Lang;
        _appliedLang = Loc.Lang;
        if (ContentFrame.Content == null)
            ContentFrame.Navigate(typeof(Pages.HomePage));
        else if (langChanged)
            // No entrance animation: this is a refresh of the same page, not a navigation the user should see.
            ContentFrame.Navigate(ContentFrame.CurrentSourcePageType,
                ContentFrame.Content is Pages.EditorPage ep ? ep.ProjectFile : null,
                new Microsoft.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
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
