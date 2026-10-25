using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Webo.App;

public static class Dialogs
{
    private static FlowDirection Dir => Loc.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    public static async Task InfoAsync(XamlRoot root, string title, string message)
    {
        var d = new ContentDialog
        {
            XamlRoot = root,
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            CloseButtonText = Loc.T("common.ok"),
            FlowDirection = Dir,
        };
        await d.ShowAsync();
    }

    public static async Task<bool> ConfirmAsync(XamlRoot root, string title, string message)
    {
        var d = new ContentDialog
        {
            XamlRoot = root,
            Title = title,
            Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap },
            PrimaryButtonText = Loc.T("common.yes"),
            CloseButtonText = Loc.T("common.no"),
            DefaultButton = ContentDialogButton.Close,
            FlowDirection = Dir,
        };
        return await d.ShowAsync() == ContentDialogResult.Primary;
    }

    public static nint WindowHandle => WinRT.Interop.WindowNative.GetWindowHandle(App.Shell!);
}
