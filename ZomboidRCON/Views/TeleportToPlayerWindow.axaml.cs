using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class TeleportToPlayerWindow : Window
{
    private List<Player> players;
    private Player player;
    private Server server;

    public TeleportToPlayerWindow(Player player, Server server)
    {
        InitializeComponent();
        this.player = player;
        this.server = server;
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Title = "Teleport '" + player.Name + "' to another player";
        InfoLabel.Text = InfoLabel.Text.Replace("%player_name%", player.Name);
        TeleportBtn.IsEnabled = false;
        PlayersCombo.IsEnabled = false;

        players = await server.GetPlayers();
        foreach (Player p in players)
        {
            if (p.Name == player.Name) continue;
            PlayersCombo.Items.Add(p.Name);
        }
        PlayersCombo.IsEnabled = true;
    }

    private void OnPlayersComboChanged(object? sender, SelectionChangedEventArgs e)
    {
        TeleportBtn.IsEnabled = PlayersCombo.SelectedIndex >= 0;
    }

    private async void OnTeleportClick(object? sender, RoutedEventArgs e)
    {
        TeleportBtn.IsEnabled = false;
        PlayersCombo.IsEnabled = false;

        Player? target = null;
        foreach (Player p in players)
        {
            if (p.Name == PlayersCombo.SelectedItem?.ToString()) target = p;
        }

        if (target == null)
        {
            await DialogHelper.ShowMessage(this, "Unable to find target player. Have they gone offline?");
            PlayersCombo.IsEnabled = true;
            return;
        }

        bool rt = await server.TeleportToPlayer(player, target);
        if (rt) Close();
        PlayersCombo.IsEnabled = true;
    }
}
