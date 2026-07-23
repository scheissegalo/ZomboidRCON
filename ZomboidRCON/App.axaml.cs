using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ZomboidRCON.Views;

namespace ZomboidRCON;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var connectionWindow = new ConnectionWindow();
            desktop.MainWindow = connectionWindow;
            connectionWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
