using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class SetPasswordWindow : Window
{
    private readonly Server server;
    private readonly Player player;

    public SetPasswordWindow(Player player, Server server)
    {
        this.player = player;
        this.server = server;
        InitializeComponent();
        PlayerNameText.Text = $"Player: {player.Name}";
    }

    private async void OnSetPasswordClick(object? sender, RoutedEventArgs e)
    {
        string password = PasswordBox.Text ?? "";
        if (string.IsNullOrWhiteSpace(password))
        {
            await DialogHelper.ShowMessage(this, "Password cannot be empty.");
            return;
        }

        bool success = await server.SetPlayerPassword(player, password);
        if (success) Close();
    }
}
