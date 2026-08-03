using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class TeleportToCoordinatesWindow : Window
{
    private const string ManualEntryLabel = "Manual entry";

    private readonly Player player;
    private readonly Server server;
    private readonly List<TeleportLocation> savedLocations = [];
    private bool suppressUpdates;

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
        ZTxt.Text = "0";
        LoadSavedLocations();
        CheckInputUpdate();
    }

    private void CommitTextInput()
    {
        TeleportBtn.Focus();
    }

    private void LoadSavedLocations(int? selectId = null)
    {
        suppressUpdates = true;
        savedLocations.Clear();
        savedLocations.AddRange(server.GetTeleportLocations());

        LocationCombo.Items.Clear();
        LocationCombo.Items.Add(ManualEntryLabel);
        foreach (var location in savedLocations)
            LocationCombo.Items.Add(location.Name);

        TeleportLocation? selectedLocation = null;
        if (selectId.HasValue)
        {
            var index = savedLocations.FindIndex(l => l.Id == selectId.Value);
            LocationCombo.SelectedIndex = index >= 0 ? index + 1 : 0;
            if (index >= 0)
                selectedLocation = savedLocations[index];
        }
        else
        {
            LocationCombo.SelectedIndex = 0;
        }

        if (selectedLocation != null)
            ApplyLocationToFields(selectedLocation);

        suppressUpdates = false;
        UpdateDeleteButton();
    }

    private void ApplyLocationToFields(TeleportLocation location)
    {
        suppressUpdates = true;
        NameTxt.Text = location.Name;
        XTxt.Text = location.X.ToString();
        YTxt.Text = location.Y.ToString();
        ZTxt.Text = location.Z.ToString();
        suppressUpdates = false;
    }

    private void ApplyLocationToFields(int x, int y, int z, string name)
    {
        suppressUpdates = true;
        NameTxt.Text = name;
        XTxt.Text = x.ToString();
        YTxt.Text = y.ToString();
        ZTxt.Text = z.ToString();
        suppressUpdates = false;
    }

    private TeleportLocation? GetSelectedLocation()
    {
        int index = LocationCombo.SelectedIndex;
        if (index <= 0)
            return null;
        return savedLocations[index - 1];
    }

    private bool IsInt(string input)
    {
        return !string.IsNullOrWhiteSpace(input) && int.TryParse(input, out _);
    }

    private bool TryParseZ(string input, out int z)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            z = 0;
            return true;
        }

        return int.TryParse(input, out z);
    }

    private bool HasValidCoordinates(out int x, out int y, out int z)
    {
        x = y = z = 0;
        return int.TryParse(XTxt.Text ?? "", out x)
            && int.TryParse(YTxt.Text ?? "", out y)
            && TryParseZ(ZTxt.Text ?? "", out z);
    }

    private bool TryGetTeleportCoordinates(out int x, out int y, out int z)
    {
        CommitTextInput();
        return HasValidCoordinates(out x, out y, out z);
    }

    private void CheckInputUpdate()
    {
        if (HasValidCoordinates(out int x, out int y, out int z))
        {
            TeleportPreviewLabel.Text = "Teleporting to: " + x + " x " + y + " x " + z;
            TeleportBtn.IsEnabled = true;
            PreviewBtn.IsEnabled = true;
        }
        else
        {
            TeleportPreviewLabel.Text = "Please fill X and Y coordinate fields";
            TeleportBtn.IsEnabled = false;
            PreviewBtn.IsEnabled = false;
        }

        SaveBtn.IsEnabled = HasValidCoordinates(out _, out _, out _)
            && !string.IsNullOrWhiteSpace(NameTxt.Text);
        UpdateDeleteButton();
    }

    private void UpdateDeleteButton()
    {
        DeleteBtn.IsEnabled = GetSelectedLocation() != null;
    }

    private void OnLocationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (suppressUpdates)
            return;

        var location = GetSelectedLocation();
        if (location == null)
        {
            CheckInputUpdate();
            return;
        }

        ApplyLocationToFields(location);
        CheckInputUpdate();
    }

    private void OnCoordinateChanged(object? sender, TextChangedEventArgs e)
    {
        if (suppressUpdates)
            return;

        if (GetSelectedLocation() != null)
        {
            suppressUpdates = true;
            LocationCombo.SelectedIndex = 0;
            suppressUpdates = false;
        }

        CheckInputUpdate();
    }

    private void OnNameChanged(object? sender, TextChangedEventArgs e)
    {
        if (suppressUpdates)
            return;

        CheckInputUpdate();
    }

    private void OnPreviewClick(object? sender, RoutedEventArgs e)
    {
        CommitTextInput();
        string x = XTxt.Text ?? "";
        string y = YTxt.Text ?? "";
        if (IsInt(x) && IsInt(y))
            CrossPlatformHelper.OpenUrl("https://map.projectzomboid.com/#" + x + "x" + y);
    }

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        CommitTextInput();
        if (!HasValidCoordinates(out int x, out int y, out int z))
            return;

        string name = (NameTxt.Text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            return;

        var selected = GetSelectedLocation();
        var location = new TeleportLocation
        {
            Id = selected?.Id ?? 0,
            Name = name,
            X = x,
            Y = y,
            Z = z
        };

        AppLog.Log("TeleportToCoordinates", $"Saving location '{name}' at {x},{y},{z}");

        if (!server.SaveTeleportLocation(location))
        {
            await DialogHelper.ShowMessage(this, "A location with that name already exists.");
            return;
        }

        var saved = server.GetTeleportLocations().FirstOrDefault(l => l.Name == name);
        AppLog.Log("TeleportToCoordinates", saved == null
            ? "Save succeeded but location could not be reloaded"
            : $"Saved location reloaded as {saved.X},{saved.Y},{saved.Z} (id={saved.Id})");

        ApplyLocationToFields(x, y, z, name);
        LoadSavedLocations(saved?.Id);
        CheckInputUpdate();
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        var location = GetSelectedLocation();
        if (location == null)
            return;

        server.DeleteTeleportLocation(location.Id);
        suppressUpdates = true;
        NameTxt.Text = "";
        XTxt.Text = "";
        YTxt.Text = "";
        ZTxt.Text = "0";
        suppressUpdates = false;
        LoadSavedLocations();
        CheckInputUpdate();
    }

    private async void OnTeleportClick(object? sender, RoutedEventArgs e)
    {
        TeleportBtn.IsEnabled = false;
        PreviewBtn.IsEnabled = false;
        SaveBtn.IsEnabled = false;
        DeleteBtn.IsEnabled = false;
        LocationCombo.IsEnabled = false;
        NameTxt.IsEnabled = false;
        XTxt.IsEnabled = false;
        YTxt.IsEnabled = false;
        ZTxt.IsEnabled = false;

        if (TryGetTeleportCoordinates(out int x, out int y, out int z))
        {
            AppLog.Log("TeleportToCoordinates", $"Teleporting '{player.Name}' to {x},{y},{z}");
            bool rt = await server.TeleportPlayerToCoordinates(player, x, y, z);
            if (rt) Close();
        }
        else
        {
            await DialogHelper.ShowMessage(this, "Coordinates conversion failed");
            suppressUpdates = true;
            XTxt.Text = "";
            YTxt.Text = "";
            ZTxt.Text = "0";
            suppressUpdates = false;
        }

        LocationCombo.IsEnabled = true;
        NameTxt.IsEnabled = true;
        XTxt.IsEnabled = true;
        YTxt.IsEnabled = true;
        ZTxt.IsEnabled = true;
        CheckInputUpdate();
    }
}
