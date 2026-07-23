using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Reflection;
using System.IO;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class ItemSpawnWindow : Window
{
    private Player player;
    private Server server;
    private List<ItemInfo> items = new();

    private class ItemInfo
    {
        public string Name { get; set; } = "";
        public string ID { get; set; } = "";
        public override string ToString() => $"{Name} ({ID})";
    }

    public ItemSpawnWindow(Player player, Server server)
    {
        InitializeComponent();
        this.player = player;
        this.server = server;
        Title = "Give item to '" + player.Name + "'";
        LoadItems();
    }

    private void LoadItems()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ZomboidRCON.Resources.pz_items.csv");
            if (stream == null)
            {
                AppLog.Log("ItemSpawn", "Embedded item list not found");
                return;
            }
            using var reader = new StreamReader(stream);
            string? header = reader.ReadLine();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
                {
                    items.Add(new ItemInfo { Name = parts[0].Trim(), ID = parts[1].Trim() });
                    ItemCombo.Items.Add(parts[0].Trim());
                }
            }
            AppLog.Log("ItemSpawn", $"Loaded {items.Count} items from embedded resource");
        }
        catch (Exception ex)
        {
            AppLog.Log("ItemSpawn", $"Failed to load items: {ex.Message}");
        }
    }

    private void OnItemComboChanged(object? sender, SelectionChangedEventArgs e)
    {
        int idx = ItemCombo.SelectedIndex;
        if (idx >= 0 && idx < items.Count)
        {
            ItemIDTxt.Text = items[idx].ID;
        }
    }

    private void OnItemIDTxtChanged(object? sender, TextChangedEventArgs e)
    {
        SpawnBtn.IsEnabled = !string.IsNullOrWhiteSpace(ItemIDTxt.Text);
    }

    private async void OnSpawnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ItemIDTxt.Text)) return;
        SpawnBtn.IsEnabled = false;
        ItemIDTxt.IsEnabled = false;
        CountNumeric.IsEnabled = false;

        try
        {
            int count = (int)(CountNumeric.Value ?? 1);
            bool success = await server.GiveItemToPlayer(player, ItemIDTxt.Text, count);
            if (success) Close();
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessage(this, "Error giving item: " + ex.Message);
        }

        SpawnBtn.IsEnabled = true;
        ItemIDTxt.IsEnabled = true;
        CountNumeric.IsEnabled = true;
    }
}
