using Microsoft.UI.Xaml;

namespace Webo.App;

public partial class App : Application
{
    public static MainWindow? Shell { get; private set; }

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            // A crash must never silently eat user data: log it and let the user keep working if possible.
            AppLog.Write("UnhandledException: " + e.Exception);
            e.Handled = true;
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Shell = new MainWindow();
        Shell.Activate();
    }
}
