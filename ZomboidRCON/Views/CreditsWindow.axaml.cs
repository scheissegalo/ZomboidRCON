using Avalonia.Controls;
using Avalonia.Input;
using ZomboidRCON.Helpers;

namespace ZomboidRCON.Views;

public partial class CreditsWindow : Window
{
    public CreditsWindow()
    {
        InitializeComponent();
        VersionLbl.Text = "v" + Constants.Version;
    }

    private void OnLink1Click(object? sender, PointerPressedEventArgs e)
    {
        CrossPlatformHelper.OpenUrl("https://github.com/kwmx/ZomboidRCON");
    }

    private void OnLink2Click(object? sender, PointerPressedEventArgs e)
    {
        CrossPlatformHelper.OpenUrl("https://github.com/neslib/RconSharp");
    }

    private void OnLink3Click(object? sender, PointerPressedEventArgs e)
    {
        CrossPlatformHelper.OpenUrl("https://github.com/mbdavid/LiteDB");
    }
}
