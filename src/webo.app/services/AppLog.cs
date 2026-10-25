namespace Webo.App;

/// <summary>Tiny local log (no network). Never throws.</summary>
public static class AppLog
{
    public static string Dir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Webo", "logs");

    public static void Write(string message)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.AppendAllText(Path.Combine(Dir, "webo.log"), $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never be the reason the app fails.
        }
    }
}
