using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using ZomboidRCON.Wrapper;

namespace ZomboidRCON.Views;

public partial class CommandConsoleWindow : Window
{
    private Server server;

    public CommandConsoleWindow(Server server)
    {
        InitializeComponent();
        this.server = server;
        AddToOutput("System", "Starting command console");
    }

    private void AddToOutput(string title, string content, bool contentNewline = false)
    {
        OutputLog.Text += title + ": " + (contentNewline ? "\n" : "") + content + "\n";
        OutputScroll.ScrollToEnd();
    }

    private void OnCommandTxtChanged(object? sender, TextChangedEventArgs e)
    {
        SendBtn.IsEnabled = !string.IsNullOrWhiteSpace(CommandTxt.Text);
    }

    private void OnCommandTxtKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(CommandTxt.Text))
        {
            OnSendClick(sender!, e);
        }
    }

    private async void OnSendClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CommandTxt.Text)) return;
        SendBtn.IsEnabled = false;
        try
        {
            string res = await server.ExecuteCommand(CommandTxt.Text);
            AddToOutput("Zomboid RCON Server Response", res, true);
            CommandTxt.Text = "";
            CommandTxt.Focus();
        }
        catch (TaskCanceledException ex)
        {
            AddToOutput("System", "Execution error, unable to execute command (" + ex.Message + ")");
        }
        SendBtn.IsEnabled = true;
    }
}
