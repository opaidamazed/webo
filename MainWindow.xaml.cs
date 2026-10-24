using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;

namespace WeboIconSpike;

public sealed partial class MainWindow : Window
{
    private static readonly string[] Locations =
    {
        "1. نافذة التطبيق وشريط العنوان",
        "2. زر التطبيق في شريط المهام أثناء التشغيل",
        "3. أيقونة التطبيق المثبّت في شريط المهام (ثبّته يدويًا قبل الاختبار)",
        "4. أيقونة الملف WeboIconSpike.exe في مستكشف الملفات (متوقع: ثابتة)",
        "5. اختصار سطح المكتب",
        "6. اختصار قائمة Start",
    };

    private static readonly string[] Outcomes =
    {
        "لم يُختبر",
        "تغيّر فورًا",
        "يحتاج إعادة تشغيل التطبيق",
        "يحتاج إعادة إنشاء الاختصار أو إعادة التثبيت",
        "لم يتغيّر",
        "ظهرت أيقونة خاطئة أو فارغة",
    };

    private readonly List<ComboBox> _combos = new();
    private readonly List<string> _log = new();
    private bool _loading;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Webo Icon Spike";

        OsInfo.Text = $"{RuntimeInformation.OSDescription} | version {Environment.OSVersion.Version} | " +
                      $"exe: {Environment.ProcessPath}";

        foreach (var option in IconService.Options)
            IconChoice.Items.Add(option.Label);

        for (var i = 0; i < Locations.Length; i++)
        {
            var combo = new ComboBox { Width = 320, ItemsSource = Outcomes, SelectedIndex = 0 };
            _combos.Add(combo);
            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            row.Children.Add(new TextBlock { Text = Locations[i], Width = 440, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center });
            row.Children.Add(combo);
            ResultsPanel.Children.Add(row);
        }

        var saved = IconService.LoadSelected();
        _loading = true;
        IconChoice.SelectedIndex = Array.FindIndex(IconService.Options, o => o.Id == saved);
        _loading = false;
        Apply(saved, save: false);
    }

    private string SelectedId =>
        IconChoice.SelectedIndex >= 0 ? IconService.Options[IconChoice.SelectedIndex].Id : IconService.DefaultId;

    private void IconChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || IconChoice.SelectedIndex < 0) return;
        Apply(SelectedId, save: true);
    }

    private void Apply(string id, bool save)
    {
        Log($"--- selected icon id: {id} ---");
        Log(IconService.ApplyToWindow(AppWindow, id));
        Log(IconService.TryApplyTaskbar(AppWindow, id));
        if (save)
        {
            try
            {
                IconService.SaveSelected(id);
                Log("settings saved (icon ID only): " + IconService.ResultsDir);
            }
            catch (Exception ex)
            {
                Log($"FAIL: saving settings threw {ex.GetType().Name}: {ex.Message}");
            }
        }
        try
        {
            PreviewImage.Source = new BitmapImage(new Uri(IconService.PreviewPath(id)));
        }
        catch (Exception ex)
        {
            PreviewImage.Source = null;
            Log($"preview not loaded ({ex.GetType().Name}); the app keeps working");
        }
    }

    private async void DesktopShortcut_Click(object sender, RoutedEventArgs e) =>
        await UpsertShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Webo Spike.lnk"));

    private async void StartShortcut_Click(object sender, RoutedEventArgs e) =>
        await UpsertShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Webo Spike.lnk"));

    private async Task UpsertShortcut(string lnkPath)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "تحديث الاختصار",
            Content = $"سيُنشأ أو يُحدَّث هذا الاختصار:\n{lnkPath}\n\nلن يُعدَّل أي اختصار لم ينشئه هذا النموذج.",
            PrimaryButtonText = "موافق",
            CloseButtonText = "إلغاء",
            FlowDirection = FlowDirection.RightToLeft,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            Log("shortcut update cancelled by the user");
            return;
        }
        Log(ShortcutHelper.Upsert(lnkPath, Environment.ProcessPath ?? "", IconService.IconPath(SelectedId)));
    }

    private void NotifyShell_Click(object sender, RoutedEventArgs e) => Log(ShortcutHelper.NotifyShell());

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _loading = true;
        IconChoice.SelectedIndex = Array.FindIndex(IconService.Options, o => o.Id == IconService.DefaultId);
        _loading = false;
        Apply(IconService.DefaultId, save: true);
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var report = new
            {
                exportedAt = DateTime.Now.ToString("o"),
                os = RuntimeInformation.OSDescription,
                osVersion = Environment.OSVersion.Version.ToString(),
                selectedIcon = SelectedId,
                locations = Locations.Select((name, i) => new { name, outcome = Outcomes[Math.Max(0, _combos[i].SelectedIndex)] }),
                log = _log,
            };
            Directory.CreateDirectory(IconService.ResultsDir);
            var file = Path.Combine(IconService.ResultsDir, $"results-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            File.WriteAllText(file, JsonSerializer.Serialize(report, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }));
            Log("results exported: " + file);
        }
        catch (Exception ex)
        {
            Log($"FAIL: export threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    private void Log(string line)
    {
        _log.Add($"[{DateTime.Now:HH:mm:ss}] {line}");
        LogBox.Text = string.Join(Environment.NewLine, _log);
    }
}
