using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
        ActualThemeChanged += (_, _) => BuildIconCards();
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
        ResetBtn.Content = Loc.T("st.icon.reset");
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

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.2.0";
        AboutText.Text = Loc.F("st.about.body", version);

        _loading = false;
        BuildIconCards();
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

    // ---- App icon ----------------------------------------------------------------------------

    private void BuildIconCards()
    {
        IconCards.Children.Clear();
        // Brushes that live in theme dictionaries cannot be looked up per element from code, so pick by the page's actual theme.
        var dark = ActualTheme == ElementTheme.Dark;
        foreach (var option in IconService.Options)
        {
            var selected = option.Id == Settings.Current.AppIcon;

            var image = new Image
            {
                Width = 72,
                Height = 72,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            try
            {
                var preview = IconService.PreviewPath(option.Id);
                if (File.Exists(preview)) image.Source = new BitmapImage(new Uri(preview));
            }
            catch (Exception ex)
            {
                AppLog.Write("icon preview failed: " + ex.Message);
            }

            var badges = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6, HorizontalAlignment = HorizontalAlignment.Center };
            if (selected)
                badges.Children.Add(new TextBlock { Text = Loc.T("st.icon.current"), Style = (Style)Application.Current.Resources["WeboPillText"] });
            if (option.Id == IconService.DefaultId)
                badges.Children.Add(new TextBlock { Text = (selected ? "· " : "") + Loc.T("st.icon.default"), Style = (Style)Application.Current.Resources["WeboMutedText"], TextWrapping = TextWrapping.NoWrap });

            var choose = new Button
            {
                Content = selected ? "✓ " + Loc.T("st.icon.current") : Loc.T("st.icon.choose"),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsEnabled = !selected,
                Tag = option.Id,
            };
            choose.Click += IconChoose_Click;

            var content = new StackPanel { Spacing = 10, Width = 150 };
            content.Children.Add(image);
            content.Children.Add(new TextBlock
            {
                Text = Loc.T(option.LabelKey),
                Style = (Style)Application.Current.Resources["WeboLabelText"],
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            content.Children.Add(badges);
            content.Children.Add(choose);

            var card = new Border
            {
                Style = (Style)Application.Current.Resources["WeboCard"],
                Padding = new Thickness(16),
                Child = content,
                BorderThickness = new Thickness(selected ? 2 : 1),
                BorderBrush = new SolidColorBrush(selected
                    ? Windows.UI.Color.FromArgb(255, dark ? (byte)0x3A : (byte)0x20, dark ? (byte)0xA0 : (byte)0x90, dark ? (byte)0xFF : (byte)0xF0)
                    : (dark
                        ? Windows.UI.Color.FromArgb(255, 0x23, 0x2C, 0x58)
                        : Windows.UI.Color.FromArgb(255, 0xD6, 0xDE, 0xEE))),
            };
            AutomationProperties_SetName(card, Loc.T(option.LabelKey));
            IconCards.Children.Add(card);
        }
    }

    private static void AutomationProperties_SetName(DependencyObject d, string name) =>
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(d, name);

    private void IconChoose_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string id }) SelectIcon(id);
    }

    private void ResetIcon_Click(object sender, RoutedEventArgs e) => SelectIcon(IconService.DefaultId);

    private void SelectIcon(string id)
    {
        Settings.Current.AppIcon = id; // the ID is stored, never a file path
        Settings.Save();
        var err = App.Shell?.ApplyIcon();
        BuildIconCards();

        if (err == null)
        {
            IconStatus.Severity = InfoBarSeverity.Success;
            IconStatus.Message = Loc.T("st.icon.applied");
        }
        else
        {
            IconStatus.Severity = InfoBarSeverity.Warning;
            IconStatus.Message = Loc.F("st.icon.fail", err);
        }
        IconStatus.IsOpen = true;
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
        var result = ShortcutHelper.Upsert(lnk, exe, IconService.IconPath(Settings.Current.AppIcon), out var error);
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
