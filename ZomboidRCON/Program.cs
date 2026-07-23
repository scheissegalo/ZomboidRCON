using Avalonia;
using ZomboidRCON.Helpers;
using System;

namespace ZomboidRCON;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.WriteLine($"Log file: {AppLog.LogPath}");
        AppLog.Log("Program", "Starting application");
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
