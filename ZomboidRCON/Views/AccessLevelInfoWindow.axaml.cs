using Avalonia.Controls;
using Avalonia.Input;
using ZomboidRCON.Helpers;

namespace ZomboidRCON.Views;

public partial class AccessLevelInfoWindow : Window
{
    public AccessLevelInfoWindow()
    {
        InitializeComponent();
    }

    private void OnLinkClick(object? sender, PointerPressedEventArgs e)
    {
        CrossPlatformHelper.OpenUrl("https://pzwiki.net/wiki/Access_level");
    }
}
