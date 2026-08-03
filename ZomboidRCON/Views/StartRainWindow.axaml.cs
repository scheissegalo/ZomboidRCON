using Avalonia.Controls;
using Avalonia.Interactivity;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class StartRainWindow : Window
{
    private readonly Server server;

    public StartRainWindow(Server server)
    {
        this.server = server;
        InitializeComponent();
        UseIntensityCheck.IsCheckedChanged += (_, _) =>
            IntensityInput.IsEnabled = UseIntensityCheck.IsChecked == true;
    }

    private async void OnStartRainClick(object? sender, RoutedEventArgs e)
    {
        int? intensity = UseIntensityCheck.IsChecked == true
            ? (int)(IntensityInput.Value ?? 50)
            : null;
        await server.StartRain(intensity);
        Close();
    }
}
