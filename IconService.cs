using System.Text.Json;
using Microsoft.UI.Windowing;

namespace WeboIconSpike;

public sealed record IconOption(string Id, string Label);

/// <summary>
/// Spike-only icon logic. Stores the icon ID (not a path), never touches the original assets,
/// and only reports what an API call returned. It never claims a visual change happened:
/// that is verified by the human tester per location (tests 9 and 10 in the requirements doc).
/// </summary>
public static class IconService
{
    public const string DefaultId = "dark";

    public static readonly IconOption[] Options =
    {
        new("mono",  "أحادية اللون"),
        new("dark",  "النسخة الداكنة"),
        new("light", "النسخة الفاتحة"),
    };

    private static string SettingsDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WeboIconSpike");

    private static string SettingsFile => Path.Combine(SettingsDir, "settings.json");

    public static string ResultsDir => SettingsDir;

    public static string IconPath(string id) =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "Brand", "derived", "icons", id, $"webo-{id}.ico");

    public static string PreviewPath(string id) =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "Brand", "derived", "icons", id, "preview-256.png");

    public static string LoadSelected()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(SettingsFile));
                if (doc.RootElement.TryGetProperty("appIcon", out var v))
                {
                    var id = v.GetString();
                    if (id != null && Options.Any(o => o.Id == id)) return id;
                }
            }
        }
        catch
        {
            // Corrupt or unreadable settings must never crash the app: fall back to the default.
        }
        return DefaultId;
    }

    public static void SaveSelected(string id)
    {
        Directory.CreateDirectory(SettingsDir);
        File.WriteAllText(SettingsFile, JsonSerializer.Serialize(new { appIcon = id }));
    }

    /// <summary>Test location 1 (and possibly 2): window / title bar icon.</summary>
    public static string ApplyToWindow(AppWindow window, string id)
    {
        var path = IconPath(id);
        if (!File.Exists(path)) return $"FAIL: icon file missing: {path}";
        try
        {
            window.SetIcon(path);
            return $"API OK: AppWindow.SetIcon(\"{path}\") returned without an exception (visual result NOT verified)";
        }
        catch (Exception ex)
        {
            return $"FAIL: AppWindow.SetIcon threw {ex.GetType().Name}: {ex.Message}";
        }
    }

    /// <summary>Test location 2: SetTaskbarIcon may not exist in the Windows App SDK version used, so call it by reflection.</summary>
    public static string TryApplyTaskbar(AppWindow window, string id)
    {
        var method = typeof(AppWindow).GetMethod("SetTaskbarIcon", new[] { typeof(string) });
        if (method == null)
            return "N/A: AppWindow.SetTaskbarIcon(string) does not exist in this Windows App SDK version";
        try
        {
            method.Invoke(window, new object[] { IconPath(id) });
            return "API OK: AppWindow.SetTaskbarIcon invoked without an exception (visual result NOT verified)";
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException ?? ex;
            return $"FAIL: SetTaskbarIcon threw {inner.GetType().Name}: {inner.Message}";
        }
    }
}
