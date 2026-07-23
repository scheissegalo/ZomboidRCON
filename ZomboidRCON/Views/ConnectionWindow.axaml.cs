using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using RconSharp;
using System;
using System.Diagnostics;
using System.Net;
using ZomboidRCON.Helpers;
using ZomboidRCON.Settings;

namespace ZomboidRCON.Views;

public partial class ConnectionWindow : Window
{
    private bool connected = false;

    public ConnectionWindow()
    {
        InitializeComponent();
        LoadSettings();
        AppLog.Log("ConnectionWindow", "Initialized");
    }

    private void LoadSettings()
    {
        var settings = AppSettings.Load();
        AppLog.Log("ConnectionWindow", $"Loaded settings: Ip={settings.Ip}, Port={settings.Port}");
        if (!string.IsNullOrWhiteSpace(settings.Ip))
            IpTxt.Text = settings.Ip;
        if (settings.Port > 1023)
            PortTxt.Text = settings.Port.ToString();
        if (!string.IsNullOrWhiteSpace(settings.Password))
            PasswordTxt.Text = settings.Password;
        SaveBox.IsChecked = !string.IsNullOrWhiteSpace(IpTxt.Text) && !string.IsNullOrWhiteSpace(PortTxt.Text) && !string.IsNullOrWhiteSpace(PasswordTxt.Text);
    }

    private async void OnConnectClick(object? sender, RoutedEventArgs e)
    {
        IPAddress? address = null;

        if (SaveBox.IsChecked != true)
        {
            var settings = AppSettings.Load();
            settings.Ip = "";
            settings.Port = 0;
            settings.Password = "";
            settings.Save();
        }

        if (string.IsNullOrEmpty(PortTxt.Text) || string.IsNullOrEmpty(IpTxt.Text) || string.IsNullOrEmpty(PasswordTxt.Text))
        {
            await DialogHelper.ShowMessage(this, "All fields must be filled");
            return;
        }

        if (!IPAddress.TryParse(IpTxt.Text, out address))
        {
            try
            {
                string url = IpTxt.Text.Replace("http://", "").Replace("https://", "").Replace("/", "");
                if (Uri.CheckHostName(url) == UriHostNameType.Unknown)
                {
                    await DialogHelper.ShowMessage(this, "Couldn't find a valid IP for the provided URL");
                    return;
                }
                Uri uri = new Uri("http://" + url);
                IPHostEntry ihe = Dns.GetHostEntry(uri.Host);
                foreach (IPAddress ipa in ihe.AddressList)
                {
                    if (ipa.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        address = ipa;
                        break;
                    }
                }
                if (address == null)
                {
                    await DialogHelper.ShowMessage(this, "Couldn't find a valid IP for the provided URL");
                    return;
                }
            }
            catch (Exception exz)
            {
                await DialogHelper.ShowMessage(this, "Incorrect IP address: " + exz.Message);
                return;
            }
        }

        if (address == null || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
        {
            await DialogHelper.ShowMessage(this, "Incorrect IP please enter an IPv4 address");
            return;
        }

        if (!int.TryParse(PortTxt.Text, out int port))
        {
            await DialogHelper.ShowMessage(this, "Incorrect port number");
            return;
        }

        SetInputEnabled(false);
        StatusLabel.Text = "Connecting...";
        AppLog.Log("ConnectionWindow", $"Connecting to {address}:{port}...");
        await RconConnectAsync(address, port, PasswordTxt.Text, IpTxt.Text);
    }

    private async Task RconConnectAsync(IPAddress address, int port, string password, string dbname)
    {
        RconClient client = RconClient.Create(address.ToString(), port);
        try
        {
            AppLog.Log("ConnectionWindow", $"RconClient connecting...");
            await client.ConnectAsync();
            AppLog.Log("ConnectionWindow", $"RconClient connected, authenticating...");
            var authenticated = await client.AuthenticateAsync(password);
            if (authenticated)
            {
                AppLog.Log("ConnectionWindow", "Authenticated successfully");
                if (SaveBox.IsChecked == true)
                {
                    var settings = new AppSettings
                    {
                        Ip = address.ToString(),
                        Port = port,
                        Password = password
                    };
                    settings.Save();
                }

                var mainWindow = new MainWindow();
                AppLog.Log("ConnectionWindow", "Created MainWindow, calling ResetConnection...");
                mainWindow.ResetConnection(client, address.ToString(), port, dbname);

                connected = true;

                // Show main window and close this one
                var desktop = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime);
                if (desktop != null)
                {
                    desktop.MainWindow = mainWindow;
                }
                mainWindow.Show();
                AppLog.Log("ConnectionWindow", "MainWindow shown, closing ConnectionWindow");
                Close();

                // Run update check after main window is visible
                await mainWindow.RunUpdateCheck();
                return;
            }
            else
            {
                StatusLabel.Text = "";
                AppLog.Log("ConnectionWindow", "Authentication failed");
                await DialogHelper.ShowMessage(this, "Authentication issue: Please check your password");
            }
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "";
            AppLog.Log("ConnectionWindow", $"Connection error: {ex}");
            await DialogHelper.ShowMessage(this, "Error: " + ex.Message);
        }

        SetInputEnabled(true);
        StatusLabel.Text = "";
    }

    private void SetInputEnabled(bool enabled)
    {
        IpTxt.IsEnabled = enabled;
        PortTxt.IsEnabled = enabled;
        PasswordTxt.IsEnabled = enabled;
        ConnectBtn.IsEnabled = enabled;
        SaveBox.IsEnabled = enabled;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        AppLog.Log("ConnectionWindow", $"OnClosing, connected={connected}");
        if (!connected)
        {
            var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            desktop?.Shutdown();
        }
        base.OnClosing(e);
    }
}
