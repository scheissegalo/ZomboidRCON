using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class AddExpWindow : Window
{
    private Server server;
    private Player player;

    public AddExpWindow(Player player, Server server)
    {
        this.player = player;
        this.server = server;
        InitializeComponent();
        PopulatePerkComboBox();
    }

    private void PopulatePerkComboBox()
    {
        foreach (var perk in Enum.GetValues(typeof(PerkName)))
        {
            PerkCombo.Items.Add(perk);
        }
        if (PerkCombo.Items.Count > 0)
            PerkCombo.SelectedIndex = 0;
    }

    private async void OnGiveExpClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            int expAmount = (int)(NumExp.Value ?? 100);
            if (PerkCombo.SelectedItem is PerkName selectedPerk)
            {
                bool success = await server.AddExperienceToPlayer(player, selectedPerk, expAmount);
                if (success) Close();
            }
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessage(this, "Error giving experience: " + ex.Message);
        }
    }
}
