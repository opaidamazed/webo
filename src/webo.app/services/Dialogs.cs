using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Webo.App;

public static class Dialogs
{
    private static FlowDirection Dir => Loc.IsArabic ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    /// <summary>The app font. Read from the app resources so dialogs and pop-ups use it explicitly.</summary>
    public static FontFamily WeboFont => (FontFamily)Application.Current.Resources["WeboFont"];

    /// <summary>
    /// A ContentDialog is shown in the pop-up layer, outside the window's content, so it does NOT inherit the theme
    /// the user picked in Settings (that is set on the window's root element) nor the app font. Everything a dialog
    /// needs to look like the rest of the app is copied onto it here. Always call this before ShowAsync.
    /// </summary>
    public static void Prepare(ContentDialog dialog, XamlRoot root)
    {
        dialog.XamlRoot = root;
        dialog.FlowDirection = Dir;
        if (root.Content is FrameworkElement content) dialog.RequestedTheme = content.RequestedTheme;
        dialog.FontFamily = WeboFont;
    }

    /// <summary>Sets the app font explicitly on controls and text blocks (a local value always wins over a style).</summary>
    public static void ApplyFont(params DependencyObject[] items)
    {
        var font = WeboFont;
        foreach (var item in items)
        {
            switch (item)
            {
                case Control c: c.FontFamily = font; break;
                case TextBlock t: t.FontFamily = font; break;
            }
        }
    }

    public static async Task InfoAsync(XamlRoot root, string title, string message)
    {
        var text = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
        var d = new ContentDialog
        {
            Title = title,
            Content = text,
            CloseButtonText = Loc.T("common.ok"),
        };
        Prepare(d, root);
        ApplyFont(text);
        await d.ShowAsync();
    }

    public static async Task<bool> ConfirmAsync(XamlRoot root, string title, string message)
    {
        var text = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap };
        var d = new ContentDialog
        {
            Title = title,
            Content = text,
            PrimaryButtonText = Loc.T("common.yes"),
            CloseButtonText = Loc.T("common.no"),
            DefaultButton = ContentDialogButton.Close,
        };
        Prepare(d, root);
        ApplyFont(text);
        return await d.ShowAsync() == ContentDialogResult.Primary;
    }

    /// <summary>Asks for one line of text. Returns null when cancelled.</summary>
    public static async Task<string?> PromptAsync(XamlRoot root, string title, string label, string initial, int maxLength = 80, string? note = null)
    {
        var box = new TextBox { Header = label, Text = initial, MaxLength = maxLength, MinWidth = 380 };
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(box);
        if (!string.IsNullOrEmpty(note))
        {
            var noteText = new TextBlock { Text = note, TextWrapping = TextWrapping.Wrap, Opacity = 0.75 };
            ApplyFont(noteText);
            panel.Children.Add(noteText);
        }

        var d = new ContentDialog
        {
            Title = title,
            Content = panel,
            PrimaryButtonText = Loc.T("common.ok"),
            CloseButtonText = Loc.T("common.cancel"),
            DefaultButton = ContentDialogButton.Primary,
        };
        Prepare(d, root);
        ApplyFont(box);
        d.Opened += (_, _) =>
        {
            box.Focus(FocusState.Programmatic);
            box.SelectAll();
        };
        return await d.ShowAsync() == ContentDialogResult.Primary ? box.Text : null;
    }

    public static nint WindowHandle => WinRT.Interop.WindowNative.GetWindowHandle(App.Shell!);
}
