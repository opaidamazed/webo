using Microsoft.UI.Windowing;

namespace Webo.App;

/// <summary>
/// App icon policy (decision D2, revised in v0.3.0):
///  - OUTSIDE the app (exe, window/taskbar, Start menu, desktop, uninstall entry) the icon is always the original DARK one.
///  - INSIDE the app only, the title-bar image follows the effective UI theme: Light theme -> light icon, Dark theme -> dark icon.
///  - The MONO icon is used by the installer only (installer/webo.iss). The app never loads it.
/// There is no user choice and nothing is stored: the icon follows the theme, so there is no setting to get out of sync.
/// </summary>
public static class IconService
{
    /// <summary>The icon used everywhere outside the app, and in-app while the theme is dark.</summary>
    public const string DefaultId = "dark";

    /// <summary>Shown in-app only, while the theme is light.</summary>
    public const string LightId = "light";

    public static string IconPath(string id) =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "Brand", "derived", "icons", id, $"webo-{id}.ico");

    public static string PreviewPath(string id) =>
        Path.Combine(AppContext.BaseDirectory, "Assets", "Brand", "derived", "icons", id, "preview-256.png");

    /// <summary>Which icon the in-app title bar shows for the current effective theme.</summary>
    public static string InAppId(bool darkTheme) => darkTheme ? DefaultId : LightId;

    /// <summary>
    /// Sets the window (and taskbar button, if this SDK exposes it) to the fixed dark icon. This never changes with the
    /// theme, so the taskbar and Alt-Tab always match the Start menu and desktop shortcuts. Returns null on success.
    /// </summary>
    public static string? ApplyToWindow(AppWindow window)
    {
        var path = IconPath(DefaultId);
        if (!File.Exists(path))
        {
            AppLog.Write("icon missing: " + path);
            return "icon file missing";
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
