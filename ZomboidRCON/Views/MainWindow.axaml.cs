using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RconSharp;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;
using ZomboidRCON.UpdateSystem;
using ZomboidRCON.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZomboidRCON.Views;

public partial class MainWindow : Window
{
    private Server? server;
    private UpdateSystem.Updator? updator;
    private bool isConnected = false;
    private int refreshRate = 30000;

    public MainWindow()
    {
        InitializeComponent();
        RefreshRateCombo.SelectedIndex = 1;
        AppLog.Log("MainWindow", "Constructor - InitializeComponent done");
    }

    public void ResetConnection(RconClient clientConnection, string host, int port, string dbname)
    {
        AppLog.Log("MainWindow", $"ResetConnection called: host={host}, port={port}, dbname={dbname}");
        try
        {
            clientConnection.ConnectionClosed += ClientConnection_ConnectionClosed;
            server = new Server(clientConnection, host, port, dbname);
            server.OnMessage = ShowServerMessage;
            isConnected = true;
            StatusText.Text = $"Connected to {host}:{port}";
            StatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#4EC94E"));
            AppLog.Log("MainWindow", "Starting RefreshLoop...");
            _ = RefreshLoop();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Connection setup failed: {ex.Message}";
            StatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E44"));
            AppLog.Log("MainWindow", $"ResetConnection error: {ex}");
        }
    }

    private async Task ShowServerMessage(string message, string title)
    {
        AppLog.Log("MainWindow", $"Server message: [{title}] {message}");
        await Dispatcher.UIThread.InvokeAsync(async () => await DialogHelper.ShowMessage(this, message, title));
    }

    private void ClientConnection_ConnectionClosed()
    {
        AppLog.Log("MainWindow", "ConnectionClosed event received");
        isConnected = false;
        Dispatcher.UIThread.Post(() =>
        {
            StatusText.Text = "Connection lost!";
            StatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E44"));
        });
        Dispatcher.UIThread.Post(async () => await DialogHelper.ShowMessage(this, "Connection Closed!"));
    }

    private async Task RefreshLoop()
    {
        AppLog.Log("MainWindow", $"RefreshLoop started, refreshRate={refreshRate}ms");
        while (isConnected)
        {
            try
            {
                await RefreshPlayers();
            }
            catch (Exception ex)
            {
                AppLog.Log("MainWindow", $"RefreshLoop error: {ex}");
            }
            await Task.Delay(refreshRate);
        }
        AppLog.Log("MainWindow", "RefreshLoop exited");
    }

    private async Task RefreshPlayers()
    {
        if (server == null)
        {
            AppLog.Log("MainWindow", "RefreshPlayers: server is null, returning");
            return;
        }

        AppLog.Log("MainWindow", $"RefreshPlayers: fetching players from {server.Host}:{server.Port}");

        Dispatcher.UIThread.Post(() =>
        {
            RefreshBtn.IsEnabled = false;
            StatusText.Text = $"Connected to {server.Host}:{server.Port} - refreshing...";
        });

        try
        {
            List<Player> players = await server.GetPlayers();
            AppLog.Log("MainWindow", $"RefreshPlayers: got {players.Count} players from server");
            foreach (var p in players)
            {
                AppLog.Log("MainWindow", $"  Player: Name='{p.Name}', AccessLevel={p.accessLevel}, Online={p.isOnline}");
            }

            var viewModels = players.Select(p => new PlayerViewModel(p)).ToList();
            AppLog.Log("MainWindow", $"RefreshPlayers: created {viewModels.Count} view models");

            Dispatcher.UIThread.Post(() =>
            {
                AppLog.Log("MainWindow", $"RefreshPlayers: setting PlayersGrid.ItemsSource ({viewModels.Count} items)");
                PlayersGrid.ItemsSource = viewModels;
                AppLog.Log("MainWindow", $"RefreshPlayers: ItemsSource set. Grid.Bounds={PlayersGrid.Bounds}, Grid.IsVisible={PlayersGrid.IsVisible}");

                StatusText.Text = $"Connected to {server.Host}:{server.Port} - {players.Count} players";
                StatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#4EC94E"));
                RefreshBtn.IsEnabled = true;
            });
        }
        catch (Exception ex)
        {
            AppLog.Log("MainWindow", $"RefreshPlayers error: {ex}");
            Dispatcher.UIThread.Post(() =>
            {
                StatusText.Text = $"Connected to {server.Host}:{server.Port} - refresh error: {ex.Message}";
                StatusText.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E44"));
                RefreshBtn.IsEnabled = true;
            });
        }
    }

    public async Task RunUpdateCheck()
    {
        try
        {
            if (updator == null)
            {
                updator = new UpdateSystem.Updator(Constants.RepoPath, Constants.AssemblyVersionToSemver, true, "ZomboidRCON.zip", "");
            }
            UpdateResult updateResult = await updator.CheckForUpdate();
            if (updateResult.UpdateStatus == UpdateStatus.UpdateNeeded && updateResult.Release != null)
            {
                var updateWindow = new UpdateWindow(updator, updateResult.Release);
                await updateWindow.ShowDialog(this);
            }
        }
        catch (Exception ex)
        {
            AppLog.Log("MainWindow", $"Update check failed: {ex.Message}");
        }
    }

    private void OnRefreshClick(object? sender, RoutedEventArgs e)
    {
        AppLog.Log("MainWindow", "Manual refresh clicked");
        _ = RefreshPlayers();
    }

    private void OnRefreshRateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (RefreshRateCombo == null) return;
        switch (RefreshRateCombo.SelectedIndex)
        {
            case 0: refreshRate = 5000; break;
            case 1: refreshRate = 30000; break;
            case 2: refreshRate = 60000; break;
        }
        AppLog.Log("MainWindow", $"Refresh rate changed to {refreshRate}ms");
    }

    private async Task<Player> GetPlayerByName(string name)
    {
        if (server == null) return new Player { Name = name };
        List<Player> players = await server.GetPlayers();
        foreach (Player player in players)
        {
            if (player.Name == name) return player;
        }
        return new Player { Name = name };
    }

    private async Task<Player?> GetSelectedPlayer()
    {
        var vm = PlayersGrid.SelectedItem as PlayerViewModel;
        if (vm == null) return null;
        return await GetPlayerByName(vm.Name);
    }

    // Context Menu Handlers
    private async void OnAddToWhitelist(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.AddPlayerToWhiteList(player);
        await RefreshPlayers();
    }

    private async void OnKickPlayer(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.KickPlayer(player);
        await RefreshPlayers();
    }

    private async void OnSetAccessNone(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.SetAccessLevel(player, AccessLevel.None);
        await RefreshPlayers();
    }

    private async void OnSetAccessOverseer(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.SetAccessLevel(player, AccessLevel.Overseer);
        await RefreshPlayers();
    }

    private async void OnSetAccessGM(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.SetAccessLevel(player, AccessLevel.GM);
        await RefreshPlayers();
    }

    private async void OnSetAccessModerator(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.SetAccessLevel(player, AccessLevel.Moderator);
        await RefreshPlayers();
    }

    private async void OnSetAccessAdmin(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.SetAccessLevel(player, AccessLevel.Admin);
        await RefreshPlayers();
    }

    private async void OnEnableGodmode(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.ChangePlayerGodmodeStatus(player, true);
        await RefreshPlayers();
    }

    private async void OnDisableGodmode(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        server.ChangePlayerGodmodeStatus(player, false);
        await RefreshPlayers();
    }

    private async void OnSpawnVehicle(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        var vsm = new VehicleSpawnWindow(player, server);
        await vsm.ShowDialog(this);
        await RefreshPlayers();
    }

    private async void OnGiveItem(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        var ism = new ItemSpawnWindow(player, server);
        await ism.ShowDialog(this);
        await RefreshPlayers();
    }

    private async void OnTeleportToPlayer(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        var teleport = new TeleportToPlayerWindow(player, server);
        await teleport.ShowDialog(this);
        await RefreshPlayers();
    }

    private async void OnTeleportToCoords(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        var teleport = new TeleportToCoordinatesWindow(player, server);
        await teleport.ShowDialog(this);
        await RefreshPlayers();
    }

    private async void OnAddExp(object? sender, RoutedEventArgs e)
    {
        var player = await GetSelectedPlayer();
        if (player == null || server == null) return;
        var addExp = new AddExpWindow(player, server);
        await addExp.ShowDialog(this);
        await RefreshPlayers();
    }

    private void OnOpenMapWebsite(object? sender, RoutedEventArgs e)
    {
        CrossPlatformHelper.OpenUrl("https://map.projectzomboid.com/#collapseThree");
    }

    // Menu Handlers
    private void OnCommandConsoleClick(object? sender, RoutedEventArgs e)
    {
        if (server == null) return;
        var cc = new CommandConsoleWindow(server);
        cc.Show();
    }

    private void OnMapClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformHelper.OpenUrl("https://map.projectzomboid.com/#collapseThree");
    }

    private void OnAccessLevelsClick(object? sender, RoutedEventArgs e)
    {
        new AccessLevelInfoWindow().Show();
    }

    private void OnReportIssueClick(object? sender, RoutedEventArgs e)
    {
        CrossPlatformHelper.OpenUrl("https://github.com/kwmx/ZomboidRCON/issues/new");
    }

    private void OnCreditsClick(object? sender, RoutedEventArgs e)
    {
        new CreditsWindow().Show();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        AppLog.Log("MainWindow", "OnClosing");
        try
        {
            isConnected = false;
            server?.Disconnect();
        }
        catch (Exception ex)
        {
            AppLog.Log("MainWindow", $"OnClosing error: {ex.Message}");
        }
        base.OnClosing(e);
    }
}

public class PlayerViewModel
{
    public string Name { get; }
    public string AccessLevelText { get; }
    public string GodmodeText { get; }

    public PlayerViewModel(Player player)
    {
        Name = player.Name;
        AccessLevelText = player.accessLevel.ToString();
        GodmodeText = player.GodmodeEnabled.ToString();
    }
}
