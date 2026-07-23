using System;
using System.IO;

namespace ZomboidRCON.Helpers;

public static class AppLog
{
    private static readonly object _lock = new();
    private static string _logPath;

    static AppLog()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".ZomboidRCON");
        Directory.CreateDirectory(dir);
        _logPath = Path.Combine(dir, "app.log");
        File.WriteAllText(_logPath, $"=== ZomboidRCON Log Started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n");
    }

    public static void Log(string message)
    {
        string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}\n";
        lock (_lock)
        {
            File.AppendAllText(_logPath, line);
        }
        System.Diagnostics.Debug.WriteLine(message);
    }

    public static void Log(string category, string message)
    {
        Log($"[{category}] {message}");
    }

    public static string LogPath => _logPath;
}
