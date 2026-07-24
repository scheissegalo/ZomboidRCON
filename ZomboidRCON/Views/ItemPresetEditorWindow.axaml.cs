using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Services;

namespace ZomboidRCON.Views;

public partial class ItemPresetEditorWindow : Window
{
    private List<ItemPreset> _visiblePresets = [];
    private ItemPreset? _editingPreset;
    private bool _isNewPreset;
    private List<Item> _addSearchResults = [];

    public ItemPresetEditorWindow()
    {
        InitializeComponent();
        InitializeFilters();
        LoadPresetList();
        RefreshAddItemCombo();
    }

    private void InitializeFilters()
    {
        GroupFilterCombo.Items.Add("All");
        GroupFilterCombo.Items.Add("Profession");
        GroupFilterCombo.Items.Add("Survival");
        GroupFilterCombo.Items.Add("Custom");
        GroupFilterCombo.SelectedIndex = 0;

        foreach (ItemPresetGroup group in Enum.GetValues(typeof(ItemPresetGroup)))
            GroupCombo.Items.Add(group.ToString());
    }

    private void LoadPresetList()
    {
        var filter = GroupFilterCombo.SelectedItem as string ?? "All";
        _visiblePresets = ItemPresetStore.All
            .Where(p => filter == "All" || p.Group.ToString() == filter)
            .ToList();

        PresetList.ItemsSource = _visiblePresets.Select(p => p.DisplayLabel).ToList();
        if (_visiblePresets.Count > 0)
            PresetList.SelectedIndex = 0;
        else
            ClearEditor();
    }

    private void OnGroupFilterChanged(object? sender, SelectionChangedEventArgs e)
    {
        LoadPresetList();
    }

    private void OnPresetListChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (PresetList.SelectedIndex < 0 || PresetList.SelectedIndex >= _visiblePresets.Count)
        {
            ClearEditor();
            return;
        }

        _isNewPreset = false;
        _editingPreset = _visiblePresets[PresetList.SelectedIndex].Clone();
        PopulateEditor();
    }

    private void PopulateEditor()
    {
        if (_editingPreset == null)
        {
            ClearEditor();
            return;
        }

        NameBox.Text = _editingPreset.Name;
        GroupCombo.SelectedItem = _editingPreset.Group.ToString();
        GroupCombo.IsEnabled = !_editingPreset.IsBuiltIn && ItemPresetStore.GetBuiltIn(_editingPreset.Id) == null;
        RefreshPresetItemsList();
        ResetBtn.IsEnabled = ItemPresetStore.GetBuiltIn(_editingPreset.Id) != null;
        RefreshAddItemCombo();
    }

    private void ClearEditor()
    {
        _editingPreset = null;
        NameBox.Text = "";
        GroupCombo.SelectedIndex = -1;
        PresetItemsList.ItemsSource = null;
        ResetBtn.IsEnabled = false;
    }

    private void RefreshPresetItemsList()
    {
        if (_editingPreset == null)
        {
            PresetItemsList.ItemsSource = null;
            return;
        }

        PresetItemsList.ItemsSource = _editingPreset.Items
            .Select(entry => $"{ItemPresetStore.FormatEntry(entry)} ({entry.Id})")
            .ToList();
    }

    private void RefreshAddItemCombo()
    {
        var query = AddSearchBox.Text;
        _addSearchResults = ItemCatalog.Search(query).Take(50).ToList();
        AddItemCombo.ItemsSource = _addSearchResults.Select(i => i.DisplayText).ToList();
        if (_addSearchResults.Count > 0)
            AddItemCombo.SelectedIndex = 0;
    }

    private void OnAddSearchChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshAddItemCombo();
    }

    private void OnAddItemComboChanged(object? sender, SelectionChangedEventArgs e)
    {
    }

    private void OnPresetItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (_editingPreset == null || PresetItemsList.SelectedIndex < 0)
            return;

        var entry = _editingPreset.Items[PresetItemsList.SelectedIndex];
        EntryCountNumeric.Value = entry.Count;
    }

    private void OnUpdateCountClick(object? sender, RoutedEventArgs e)
    {
        if (_editingPreset == null || PresetItemsList.SelectedIndex < 0)
            return;

        _editingPreset.Items[PresetItemsList.SelectedIndex].Count = (int)(EntryCountNumeric.Value ?? 1);
        RefreshPresetItemsList();
        PresetItemsList.SelectedIndex = Math.Min(PresetItemsList.SelectedIndex, _editingPreset.Items.Count - 1);
    }

    private void OnRemoveItemClick(object? sender, RoutedEventArgs e)
    {
        if (_editingPreset == null || PresetItemsList.SelectedIndex < 0)
            return;

        _editingPreset.Items.RemoveAt(PresetItemsList.SelectedIndex);
        RefreshPresetItemsList();
    }

    private void OnAddItemClick(object? sender, RoutedEventArgs e)
    {
        if (_editingPreset == null)
            return;

        int index = AddItemCombo.SelectedIndex;
        if (index < 0 || index >= _addSearchResults.Count)
            return;

        var item = _addSearchResults[index];
        int count = (int)(AddCountNumeric.Value ?? 1);
        var existing = _editingPreset.Items.FirstOrDefault(i => i.Id == item.Id);
        if (existing != null)
            existing.Count += count;
        else
            _editingPreset.Items.Add(new ItemPresetEntry { Id = item.Id, Count = count });

        RefreshPresetItemsList();
    }

    private void OnNewClick(object? sender, RoutedEventArgs e)
    {
        _isNewPreset = true;
        _editingPreset = new ItemPreset
        {
            Id = "",
            Name = "New Preset",
            Group = ItemPresetGroup.Custom,
            IsBuiltIn = false,
            Items = []
        };
        PopulateEditor();
        NameBox.Focus();
    }

    private void OnDuplicateClick(object? sender, RoutedEventArgs e)
    {
        if (_editingPreset == null && PresetList.SelectedIndex >= 0)
            _editingPreset = _visiblePresets[PresetList.SelectedIndex].Clone();

        if (_editingPreset == null)
            return;

        _isNewPreset = true;
        _editingPreset = _editingPreset.Clone();
        _editingPreset.Id = "";
        _editingPreset.Name = _editingPreset.Name + " (Copy)";
        _editingPreset.IsBuiltIn = false;
        _editingPreset.Group = ItemPresetGroup.Custom;
        PopulateEditor();
    }

    private async void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (PresetList.SelectedIndex < 0 || PresetList.SelectedIndex >= _visiblePresets.Count)
            return;

        var preset = _visiblePresets[PresetList.SelectedIndex];
        ItemPresetStore.DeletePreset(preset);
        LoadPresetList();
    }

    private void OnResetClick(object? sender, RoutedEventArgs e)
    {
        if (_editingPreset == null)
            return;

        var builtIn = ItemPresetStore.GetBuiltIn(_editingPreset.Id);
        if (builtIn == null)
            return;

        ItemPresetStore.ResetBuiltIn(_editingPreset.Id);
        LoadPresetList();

        int index = _visiblePresets.FindIndex(p => p.Id == builtIn.Id);
        if (index >= 0)
            PresetList.SelectedIndex = index;
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        if (_editingPreset == null)
            return;

        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            await DialogHelper.ShowMessage(this, "Preset name is required.");
            return;
        }

        if (_editingPreset.Items.Count == 0)
        {
            await DialogHelper.ShowMessage(this, "Add at least one item to the preset.");
            return;
        }

        _editingPreset.Name = NameBox.Text.Trim();
        if (GroupCombo.SelectedItem is string groupText &&
            Enum.TryParse<ItemPresetGroup>(groupText, out var group))
        {
            _editingPreset.Group = group;
        }

        var unknownItems = _editingPreset.Items
            .Where(i => ItemCatalog.All.All(c => c.Id != i.Id))
            .Select(i => i.Id)
            .ToList();

        if (unknownItems.Count > 0)
        {
            await DialogHelper.ShowMessage(
                this,
                $"Warning: {unknownItems.Count} item ID(s) are not in the bundled catalog (mods/custom items are OK):\n" +
                string.Join(", ", unknownItems.Take(5)) +
                (unknownItems.Count > 5 ? "..." : ""));
        }

        if (_isNewPreset || string.IsNullOrWhiteSpace(_editingPreset.Id))
            ItemPresetStore.SaveCustom(_editingPreset);
        else if (ItemPresetStore.GetBuiltIn(_editingPreset.Id) != null)
            ItemPresetStore.SaveOverride(_editingPreset);
        else
            ItemPresetStore.SaveCustom(_editingPreset);

        _isNewPreset = false;
        LoadPresetList();

        int index = _visiblePresets.FindIndex(p => p.Id == _editingPreset.Id);
        if (index >= 0)
            PresetList.SelectedIndex = index;
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
