using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class TeleportToCoordinatesWindow : Window
{
    private Player player;
    private Server server;

    public TeleportToCoordinatesWindow(Player player, Server server)
    {
        InitializeComponent();
        this.player = player;
        this.server = server;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Title = "Teleport '" + player.Name + "' to coordinates";
        CheckInputUpdate();
    }

    private bool IsInt(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && int.TryParse(input, out _);
    }

    private void CheckInputUpdate()
    {
        string x = XTxt.Text ?? "";
        string y = YTxt.Text ?? "";
        string z = ZTxt.Text ?? "";
        if (IsInt(x) && IsInt(y) && IsInt(z))
        {
            TeleportPreviewLabel.Text = "Teleporting to: " + x + " x " + y + " x " + z;
            TeleportBtn.IsEnabled = true;
            PreviewBtn.IsEnabled = true;
        }
        else
        {
            TeleportPreviewLabel.Text = "Please fill all required coordinate fields";
            TeleportBtn.IsEnabled = false;
            PreviewBtn.IsEnabled = false;
        }
    }

    private void OnCoordinateChanged(object? sender, TextChangedEventArgs e)
    {
        CheckInputUpdate();
    }

    private void OnPreviewClick(object? sender, RoutedEventArgs e)
    {
        string x = XTxt.Text ?? "";
        string y = YTxt.Text ?? "";
        if (IsInt(x) && IsInt(y))
        {
            CrossPlatformHelper.OpenUrl("https://map.projectzomboid.com/#" + x + "x" + y);
        }
    }

    private async void OnTeleportClick(object? sender, RoutedEventArgs e)
    {
        TeleportBtn.IsEnabled = false;
        PreviewBtn.IsEnabled = false;
        XTxt.IsEnabled = false;
        YTxt.IsEnabled = false;
        ZTxt.IsEnabled = false;

        if (int.TryParse(XTxt.Text, out int x) && int.TryParse(YTxt.Text, out int y) && int.TryParse(ZTxt.Text, out int z))
        {
            bool rt = await server.TeleportPlayerToCoordinates(player, x, y, z);
            if (rt) Close();
        }
        else
        {
            await DialogHelper.ShowMessage(this, "Coordinates conversion failed");
            XTxt.Text = "";
            YTxt.Text = "";
            ZTxt.Text = "";
        }

        XTxt.IsEnabled = true;
        YTxt.IsEnabled = true;
        ZTxt.IsEnabled = true;
        CheckInputUpdate();
    }
}
