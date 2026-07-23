using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.ComponentModel;
using Downloader;
using ZomboidRCON.UpdateSystem;
using ZomboidRCON.UpdateSystem.GithubApi;

namespace ZomboidRCON.Views;

public partial class UpdateWindow : Window
{
    private Updator updator;
    private Release release;
    private Asset? asset;

    public UpdateWindow(Updator updator, Release release)
    {
        InitializeComponent();
        this.updator = updator;
        this.release = release;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        asset = updator.GetDownloadableAsset(release);
        if (string.IsNullOrWhiteSpace(release.body))
            DetailsTxt.Text = "No details provided for the version";
        else
            DetailsTxt.Text = release.body;
        SizeLbl.Text = BytesToMb(asset?.size ?? 0) + " mb";
        VersionLbl.Text = release.tag_name;
    }

    private int BytesToMb(long bytes)
    {
        return (int)bytes / 1024 / 1024;
    }

    private async void OnDownloadClick(object? sender, RoutedEventArgs e)
    {
        DownloadBtn.IsEnabled = false;
        try
        {
            await updator.Update(release, true,
                downloadStarted,
                chunkDownloaded,
                downloadProgress,
                downloadCompleted);
        }
        catch (Exception ex)
        {
            DetailsTxt.IsVisible = true;
            DownloadBar.IsVisible = false;
            UpdateLabel.Text = "Update failed: " + ex.Message;
            DownloadBtn.IsEnabled = true;
        }
    }

    private void downloadCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DownloadBar.Value = 100;
            UpdateLabel.Text = "Installing...";
        });
    }

    private void downloadProgress(object? sender, DownloadProgressChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DownloadBar.Value = (int)e.ProgressPercentage;
            UpdateLabel.Text = "Downloading update " + (int)e.ProgressPercentage + "% (" + BytesToMb(e.ReceivedBytesSize) + "/" + BytesToMb(e.TotalBytesToReceive) + ")...";
        });
    }

    private void chunkDownloaded(object? sender, DownloadProgressChangedEventArgs e) { }

    private void downloadStarted(object? sender, DownloadStartedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            DetailsTxt.IsVisible = false;
            DownloadBar.IsVisible = true;
            UpdateLabel.Text = "Downloading...";
            DownloadBar.Value = 0;
        });
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (updator.IsBusy) e.Cancel = true;
        base.OnClosing(e);
    }
}
