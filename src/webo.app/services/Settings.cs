using System.Text.Json;

namespace Webo.App;

/// <summary>Per-user app settings (decision D7): stored in LocalAppData, icon stored as an ID, never a path.</summary>
public sealed class AppSettings
{
    public string Language { get; set; } = "ar";
    public string Theme { get; set; } = "system"; // system | light | dark
    public string AppIcon { get; set; } = IconService.DefaultId;
}

public static class Settings
{
    public static string Dir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Webo");

    private static string FilePath => Path.Combine(Dir, "settings.json");

    public static AppSettings Current { get; private set; } = new();

    public static void Load()
    {
        var s = new AppSettings();
        try
        {
            if (File.Exists(FilePath))
            {
                var loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath));
                if (loaded != null) s = loaded;
            }
        }
        catch (Exception ex)
        {
            // Corrupt settings must never crash the app: fall back to defaults.
            AppLog.Write("settings load failed: " + ex.Message);
        }

        if (s.Language != "ar" && s.Language != "en") s.Language = "ar";
        if (s.Theme != "system" && s.Theme != "light" && s.Theme != "dark") s.Theme = "system";
        if (!IconService.Options.Any(o => o.Id == s.AppIcon)) s.AppIcon = IconService.DefaultId;
        Current = s;
        Loc.Lang = s.Language;
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex)
        {
            AppLog.Write("settings save failed: " + ex.Message);
        }
    }
}
