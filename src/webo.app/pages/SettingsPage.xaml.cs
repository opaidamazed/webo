using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System.Reflection;

namespace Webo.App.Pages;

public sealed partial class SettingsPage : Page
{
    private bool _loading;
    private readonly string[] _langIds = { "ar", "en" };
    private readonly string[] _themeIds = { "system", "light", "dark" };

    public SettingsPage()
    {
        InitializeComponent();
        ActualThemeChanged += (_, _) => ShowInAppIcon();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        TitleText.Text = Loc.T("st.title");
        AppearanceHeader.Text = Loc.T("st.appearance");
        LangLabel.Text = Loc.T("st.lang");
        LangNote.Text = Loc.T("st.lang.note");
        ThemeLabel.Text = Loc.T("st.theme");
        IconHeader.Text = Loc.T("st.icon");
        IconNote.Text = Loc.T("st.icon.note");
        ScTitle.Text = Loc.T("st.sc.title");
        ScNote.Text = Loc.T("st.sc.note");
        ScDesktopBtn.Content = Loc.T("st.sc.desktop");
        ScStartBtn.Content = Loc.T("st.sc.start");
        AboutHeader.Text = Loc.T("st.about");
        DataText.Text = Loc.T("st.data");
        _loading = true;

        LangChoice.Items.Add(Loc.T("st.lang.ar"));
        LangChoice.Items.Add(Loc.T("st.lang.en"));
        LangChoice.SelectedIndex = Math.Max(0, Array.IndexOf(_langIds, Settings.Current.Language));

        ThemeChoice.Items.Add(Loc.T("st.theme.system"));
        ThemeChoice.Items.Add(Loc.T("st.theme.light"));
        ThemeChoice.Items.Add(Loc.T("st.theme.dark"));
        ThemeChoice.SelectedIndex = Math.Max(0, Array.IndexOf(_themeIds, Settings.Current.Theme));

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.3.0";
        AboutText.Text = Loc.F("st.about.body", version);

        _loading = false;
        ShowInAppIcon();
    }

    private void LangChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || LangChoice.SelectedIndex < 0) return;
        var id = _langIds[LangChoice.SelectedIndex];
        // RadioButtons raises SelectionChanged again when its items are realized (after OnNavigatedTo has finished).
        // Without this check that late event re-created the page, which raised it again, forever (page flicker, scroll reset).
        if (id == Settings.Current.Language) return;
        AppLog.Write("language changed by user: " + Settings.Current.Language + " -> " + id);
        Settings.Current.Language = id;
        Settings.Save();
        App.Shell?.ApplyLanguage(); // re-creates this page in the new language and direction
    }

    private void ThemeChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || ThemeChoice.SelectedIndex < 0) return;
        var id = _themeIds[ThemeChoice.SelectedIndex];
        if (id == Settings.Current.Theme) return; // same late-event guard as the language choice
        Settings.Current.Theme = id;
        Settings.Save();
        App.Shell?.ApplyTheme();
    }

    // ---- App icon (read-only: it follows the theme) --------------------------------------------

    private void ShowInAppIcon()
    {
        var dark = ActualTheme == ElementTheme.Dark;
        var id = IconService.InAppId(dark);
        try
        {
            var preview = IconService.PreviewPath(id);
            if (File.Exists(preview)) InAppIconImage.Source = new BitmapImage(new Uri(preview));
        }
        catch (Exception ex)
        {
            AppLog.Write("icon preview failed: " + ex.Message);
        }
        InAppIconLabel.Text = Loc.F("st.icon.inapp", Loc.T(dark ? "st.icon.dark" : "st.icon.light"));
    }

    // ---- Shortcuts (decision D3: consent first, only our own shortcuts) ----------------------

    private async void DesktopShortcut_Click(object sender, RoutedEventArgs e) =>
        await UpsertShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Webo.lnk"));

    private async void StartShortcut_Click(object sender, RoutedEventArgs e) =>
        await UpsertShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Webo.lnk"));

    private async Task UpsertShortcut(string lnk)
    {
        var ok = await Dialogs.ConfirmAsync(XamlRoot, Loc.T("st.sc.title"), Loc.F("st.sc.confirm", lnk));
        if (!ok) return;

        var exe = Environment.ProcessPath ?? "";
        // Shortcuts always use the original dark icon, the same as the installer's shortcuts.
        var result = ShortcutHelper.Upsert(lnk, exe, IconService.IconPath(IconService.DefaultId), out var error);
        switch (result)
        {
            case ShortcutResult.Ok:
                ShortcutStatus.Severity = InfoBarSeverity.Success;
                ShortcutStatus.Message = Loc.T("st.sc.ok");
                break;
            case ShortcutResult.Refused:
                ShortcutStatus.Severity = InfoBarSeverity.Warning;
                ShortcutStatus.Message = Loc.T("st.sc.refused");
                break;
            default:
                ShortcutStatus.Severity = InfoBarSeverity.Error;
                ShortcutStatus.Message = Loc.F("st.sc.fail", error);
                break;
        }
        ShortcutStatus.IsOpen = true;
    }
}
