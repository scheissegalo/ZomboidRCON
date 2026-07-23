using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class VehicleSpawnWindow : Window
{
    private List<Vehicle> vehicles;
    private Player player;
    private Server server;

    public VehicleSpawnWindow(Player player, Server server)
    {
        InitializeComponent();
        this.player = player;
        this.server = server;
        Title = "Spawn a vehicle for '" + player.Name + "'";
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        vehicles = server.Vehicles;
        foreach (var v in vehicles)
        {
            VehiclesCombo.Items.Add(v.Name);
        }
    }

    private void OnVehiclesComboChanged(object? sender, SelectionChangedEventArgs e)
    {
        int i = VehiclesCombo.SelectedIndex;
        VariantCombo.Items.Clear();
        VariantCombo.IsEnabled = false;
        if (i >= 0 && i < vehicles.Count && vehicles[i].Variants != null)
        {
            foreach (var v in vehicles[i].Variants!)
            {
                VariantCombo.Items.Add(v.Title);
            }
            if (VariantCombo.Items.Count > 0)
            {
                VariantCombo.SelectedIndex = 0;
                VariantCombo.IsEnabled = true;
            }
        }
    }

    private void OnVariantComboChanged(object? sender, SelectionChangedEventArgs e)
    {
        int i = VehiclesCombo.SelectedIndex;
        int j = VariantCombo.SelectedIndex;
        if (i >= 0 && j >= 0 && i < vehicles.Count && vehicles[i].Variants != null && j < vehicles[i].Variants!.Length)
        {
            var variant = vehicles[i].Variants![j];
            AppLog.Log("VehicleSpawn", $"Selected variant: VariantID={variant.VariantID}, isStock={variant.isStock}");
            if (variant.isStock)
            {
                try
                {
                    var path = $"avares://ZomboidRCON/Assets/Vehicles/{variant.VariantID}.png";
                    AppLog.Log("VehicleSpawn", $"Loading image from: {path}");
                    VehicleImage.Source = new Bitmap(path);
                    AppLog.Log("VehicleSpawn", $"Image loaded successfully, Source={VehicleImage.Source != null}");
                }
                catch (Exception ex)
                {
                    AppLog.Log("VehicleSpawn", $"Image load FAILED: {ex}");
                    VehicleImage.Source = null;
                }
            }
            SpawnBtn.IsEnabled = true;
        }
        else
        {
            SpawnBtn.IsEnabled = false;
            VehicleImage.Source = null;
        }
    }

    private async void OnSpawnClick(object? sender, RoutedEventArgs e)
    {
        SpawnBtn.IsEnabled = false;
        int i = VehiclesCombo.SelectedIndex;
        int j = VariantCombo.SelectedIndex;
        if (i >= 0 && j >= 0 && vehicles[i].Variants != null && j < vehicles[i].Variants!.Length)
        {
            VehiclesCombo.IsEnabled = false;
            VariantCombo.IsEnabled = false;
            bool rt = await server.SpawnVehicleForPlayer(player, vehicles[i].Variants![j]);
            if (rt) Close();
            VehiclesCombo.IsEnabled = true;
            VariantCombo.IsEnabled = true;
        }
        SpawnBtn.IsEnabled = true;
    }
}
