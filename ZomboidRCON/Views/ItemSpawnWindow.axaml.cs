using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Services;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class ItemSpawnWindow : Window
{
    private readonly Player player;
    private readonly Server server;
    private bool _suppressSelectionSync;
    private List<ItemPreset> _presets = [];

    public ItemSpawnWindow(Player player, Server server)
    {
        InitializeComponent();
        this.player = player;
        this.server = server;
        Title = "Give item to '" + player.Name + "'";
        InitializeCategories();
        InitializePresets();
        RefreshItemList();
    }

    private void InitializeCategories()
    {
        CategoryCombo.Items.Add("All");
        foreach (var category in ItemCatalog.Categories)
            CategoryCombo.Items.Add(category.ToString());
        CategoryCombo.SelectedIndex = 0;
    }

    private void InitializePresets()
    {
        RefreshPresetList();
        PresetCombo.SelectedIndex = 0;
    }

    private void RefreshPresetList()
    {
        _presets = ItemPresetStore.All.ToList();
        PresetCombo.Items.Clear();
        PresetCombo.Items.Add("(None)");
        foreach (var preset in _presets)
            PresetCombo.Items.Add(preset.DisplayLabel);
    }

    private ItemPreset? GetSelectedPreset()
    {
        int index = PresetCombo.SelectedIndex;
        if (index <= 0 || index - 1 >= _presets.Count)
            return null;
        return _presets[index - 1];
    }

    private void RefreshPresetPreview()
    {
        var preset = GetSelectedPreset();
        GivePresetBtn.IsEnabled = preset != null;
        PresetPreview.ItemsSource = preset?.Items
            .Select(ItemPresetStore.FormatEntry)
            .ToList() ?? [];
    }

    private void OnPresetChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshPresetPreview();
    }

    private async void OnGivePresetClick(object? sender, RoutedEventArgs e)
    {
        var preset = GetSelectedPreset();
        if (preset == null) return;

        GivePresetBtn.IsEnabled = false;
        try
        {
            await server.GiveItemPresetToPlayer(player, preset);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessage(this, "Error giving preset: " + ex.Message);
        }
        finally
        {
            GivePresetBtn.IsEnabled = preset != null;
        }
    }

    private async void OnManagePresetsClick(object? sender, RoutedEventArgs e)
    {
        var editor = new ItemPresetEditorWindow();
        await editor.ShowDialog(this);
        RefreshPresetList();
        RefreshPresetPreview();
    }

    private ItemType? GetSelectedCategory()
    {
        if (CategoryCombo.SelectedItem is not string selected || selected == "All")
            return null;

        return Enum.TryParse<ItemType>(selected, out var category) ? category : null;
    }

    private void RefreshItemList()
    {
        var query = SearchBox.Text;
        var category = GetSelectedCategory();
        var results = ItemCatalog.Search(query, category).ToList();

        _suppressSelectionSync = true;
        ItemsList.ItemsSource = results;

        if (results.Count > 0)
        {
            ItemsList.SelectedIndex = 0;
            ItemIDTxt.Text = results[0].Id;
        }
        else if (string.IsNullOrWhiteSpace(ItemIDTxt.Text))
        {
            ItemsList.SelectedIndex = -1;
        }

        _suppressSelectionSync = false;
        SpawnBtn.IsEnabled = !string.IsNullOrWhiteSpace(ItemIDTxt.Text);
    }

    private void OnFilterChanged(object? sender, RoutedEventArgs e)
    {
        RefreshItemList();
    }

    private void OnItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionSync)
            return;

        if (ItemsList.SelectedItem is Item item)
            ItemIDTxt.Text = item.Id;
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
        SearchBox.IsEnabled = false;
        CategoryCombo.IsEnabled = false;
        ItemsList.IsEnabled = false;
        PresetCombo.IsEnabled = false;
        GivePresetBtn.IsEnabled = false;

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
        SearchBox.IsEnabled = true;
        CategoryCombo.IsEnabled = true;
        ItemsList.IsEnabled = true;
        PresetCombo.IsEnabled = true;
        GivePresetBtn.IsEnabled = GetSelectedPreset() != null;
    }
}
