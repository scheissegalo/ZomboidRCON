using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class StartStormWindow : Window
{
    private readonly Server server;

    public StartStormWindow(Server server)
    {
        this.server = server;
        InitializeComponent();
        UseDurationCheck.IsCheckedChanged += (_, _) =>
            DurationInput.IsEnabled = UseDurationCheck.IsChecked == true;
    }

    private async void OnStartStormClick(object? sender, RoutedEventArgs e)
    {
        int? duration = UseDurationCheck.IsChecked == true
            ? (int)(DurationInput.Value ?? 24)
            : null;
        await server.StartStorm(duration);
        Close();
    }
}
