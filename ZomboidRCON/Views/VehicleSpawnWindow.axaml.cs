using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class VehicleSpawnWindow : Window
{
    private List<Vehicle> vehicles;
    private Player player;
    private Server server;
    private bool _suppressSelectionSync;

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
        else if (string.IsNullOrWhiteSpace(VehicleIDTxt.Text))
        {
            SpawnBtn.IsEnabled = false;
            VehicleImage.Source = null;
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
            _suppressSelectionSync = true;
            VehicleIDTxt.Text = variant.VariantID;
            _suppressSelectionSync = false;
            UpdateVehiclePreview(variant.VariantID);
            SpawnBtn.IsEnabled = !string.IsNullOrWhiteSpace(VehicleIDTxt.Text);
        }
        else if (string.IsNullOrWhiteSpace(VehicleIDTxt.Text))
        {
            SpawnBtn.IsEnabled = false;
            VehicleImage.Source = null;
        }
    }

    private void OnVehicleIDTxtChanged(object? sender, TextChangedEventArgs e)
    {
        SpawnBtn.IsEnabled = !string.IsNullOrWhiteSpace(VehicleIDTxt.Text);
        if (!_suppressSelectionSync)
            UpdateVehiclePreview(VehicleIDTxt.Text);
    }

    private void UpdateVehiclePreview(string? vehicleId)
    {
        VehicleImage.Source = string.IsNullOrWhiteSpace(vehicleId)
            ? null
            : TryLoadVehiclePreview(vehicleId.Trim());
    }

    private async void OnSpawnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(VehicleIDTxt.Text)) return;

        SetControlsEnabled(false);
        try
        {
            bool rt = await server.SpawnVehicleForPlayer(player, VehicleIDTxt.Text.Trim());
            if (rt) Close();
        }
        finally
        {
            if (IsVisible)
                SetControlsEnabled(true);
        }
    }

    private void SetControlsEnabled(bool enabled)
    {
        SpawnBtn.IsEnabled = enabled && !string.IsNullOrWhiteSpace(VehicleIDTxt.Text);
        VehicleIDTxt.IsEnabled = enabled;
        VehiclesCombo.IsEnabled = enabled;
        VariantCombo.IsEnabled = enabled && VariantCombo.Items.Count > 0;
    }

    private static Bitmap? TryLoadVehiclePreview(string variantId)
    {
        var uri = new Uri($"avares://ZomboidRCON/Assets/Vehicles/{variantId}.png");
        try
        {
            if (!AssetLoader.Exists(uri))
            {
                AppLog.Log("VehicleSpawn", $"Preview not found: {uri}");
                return null;
            }

            AppLog.Log("VehicleSpawn", $"Loading preview from: {uri}");
            return new Bitmap(AssetLoader.Open(uri));
        }
        catch (Exception ex)
        {
            AppLog.Log("VehicleSpawn", $"Preview load failed for {variantId}: {ex.Message}");
            return null;
        }
    }
}
