using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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

        SetControlsEnabled(false);
        try
        {
            var (_, failed, message) = await server.GiveItemPresetToPlayer(player, preset, showMessage: false);
            if (failed == 0)
            {
                await CloseAndShowResult(message);
                return;
            }

            await DialogHelper.ShowMessage(this, message);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessage(this, "Error giving preset: " + ex.Message);
        }
        finally
        {
            if (IsVisible)
                SetControlsEnabled(true);
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
            UpdateItemPreview(results[0].Id);
        }
        else if (string.IsNullOrWhiteSpace(ItemIDTxt.Text))
        {
            ItemsList.SelectedIndex = -1;
            UpdateItemPreview(null);
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
        {
            ItemIDTxt.Text = item.Id;
            UpdateItemPreview(item.Id);
        }
    }

    private void OnItemIDTxtChanged(object? sender, TextChangedEventArgs e)
    {
        SpawnBtn.IsEnabled = !string.IsNullOrWhiteSpace(ItemIDTxt.Text);
        if (!_suppressSelectionSync)
            UpdateItemPreview(ItemIDTxt.Text);
    }

    private void UpdateItemPreview(string? itemId)
    {
        ItemPreviewImage.Source = string.IsNullOrWhiteSpace(itemId)
            ? null
            : TryLoadItemPreview(itemId.Trim());
    }

    private static Bitmap? TryLoadItemPreview(string itemId)
    {
        var uri = new Uri($"avares://ZomboidRCON/Assets/Items/{itemId}.jpg");
        try
        {
            if (!AssetLoader.Exists(uri))
            {
                AppLog.Log("ItemSpawn", $"Preview not found: {uri}");
                return null;
            }

            return new Bitmap(AssetLoader.Open(uri));
        }
        catch (Exception ex)
        {
            AppLog.Log("ItemSpawn", $"Preview load failed for {itemId}: {ex.Message}");
            return null;
        }
    }

    private async void OnSpawnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ItemIDTxt.Text)) return;

        SetControlsEnabled(false);
        try
        {
            int count = (int)(CountNumeric.Value ?? 1);
            var (success, message) = await server.GiveItemToPlayer(player, ItemIDTxt.Text, count, showMessage: false);
            if (success)
            {
                await CloseAndShowResult(message);
                return;
            }

            await DialogHelper.ShowMessage(this, message);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowMessage(this, "Error giving item: " + ex.Message);
        }
        finally
        {
            if (IsVisible)
                SetControlsEnabled(true);
        }
    }

    private void SetControlsEnabled(bool enabled)
    {
        SpawnBtn.IsEnabled = enabled && !string.IsNullOrWhiteSpace(ItemIDTxt.Text);
        ItemIDTxt.IsEnabled = enabled;
        CountNumeric.IsEnabled = enabled;
        SearchBox.IsEnabled = enabled;
        CategoryCombo.IsEnabled = enabled;
        ItemsList.IsEnabled = enabled;
        PresetCombo.IsEnabled = enabled;
        GivePresetBtn.IsEnabled = enabled && GetSelectedPreset() != null;
        ManagePresetsBtn.IsEnabled = enabled;
    }

    private async Task CloseAndShowResult(string message)
    {
        var owner = Owner as Window;
        Close();
        if (owner != null)
            await DialogHelper.ShowMessage(owner, message);
    }
}
