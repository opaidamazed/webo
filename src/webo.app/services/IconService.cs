using Microsoft.UI.Windowing;

namespace Webo.App;

public sealed record IconOption(string Id, string LabelKey);

/// <summary>
/// App icon logic (requirements section 22). Stores the icon ID, never touches the original assets,
/// falls back to the default icon if a file is missing, and only reports what an API call returned.
/// </summary>
public static class IconService
{
    public const string DefaultId = "dark";

    public static readonly IconOption[] Options =
    {
        new("mono", "st.icon.mono"),
        new("dark", "st.icon.dark"),
        new("light", "st.icon.light"),
    };

    public static string IconPath(string id) =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "Brand", "derived", "icons", id, $"webo-{id}.ico");

    public static string PreviewPath(string id) =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "Brand", "derived", "icons", id, "preview-256.png");

    /// <summary>Applies the icon to the window (and the taskbar button if this SDK exposes it). Returns null on success, else an error text.</summary>
    public static string? ApplyToWindow(AppWindow window, string id)
    {
        var path = IconPath(id);
        if (!File.Exists(path))
        {
            AppLog.Write("icon missing: " + path);
            path = IconPath(DefaultId); // decision D8: fall back to the embedded default
            if (!File.Exists(path)) return "icon file missing";
        }

        try
        {
            window.SetIcon(path);
        }
        catch (Exception ex)
        {
            AppLog.Write("SetIcon failed: " + ex);
            return ex.Message;
        }

        // SetTaskbarIcon exists only in newer Windows App SDK builds; call it only if present.
        try
        {
            var m = typeof(AppWindow).GetMethod("SetTaskbarIcon", new[] { typeof(string) });
            m?.Invoke(window, new object[] { path });
        }
        catch (Exception ex)
        {
            AppLog.Write("SetTaskbarIcon failed: " + (ex.InnerException ?? ex).Message);
        }
        return null;
    }
}
