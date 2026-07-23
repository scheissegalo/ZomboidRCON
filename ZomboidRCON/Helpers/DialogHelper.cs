using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace ZomboidRCON.Helpers;

public static class DialogHelper
{
    public static async Task ShowMessage(Window? parent, string message, string title = "ZomboidRCON")
    {
        if (parent == null || !parent.IsLoaded) return;
        var dialog = new Window
        {
            Title = title,
            Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap, MaxWidth = 400 },
                    new Button
                    {
                        Content = "OK",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        MinWidth = 80
                    }
                }
            },
            Width = 450,
            SizeToContent = Avalonia.Controls.SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false
        };

        var okButton = (Button)((StackPanel)dialog.Content).Children[1];
        okButton.Click += (_, _) => dialog.Close();

        await dialog.ShowDialog(parent);
    }
}
