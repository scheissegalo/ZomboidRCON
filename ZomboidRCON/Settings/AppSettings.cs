using System.Text.Json;

namespace ZomboidRCON.Settings;

public class AppSettings
{
    public string Ip { get; set; } = "";
    public int Port { get; set; }
    public string Password { get; set; } = "";

    private static string GetSettingsPath()
    {
        string dir;
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            string xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
                               ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            dir = Path.Combine(xdgConfig, "ZomboidRCON");
        }
        else
        {
            dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ZomboidRCON");
        }
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "settings.json");
    }

    public void Save()
    {
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetSettingsPath(), json);
    }

    public static AppSettings Load()
    {
        string path = GetSettingsPath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        return new AppSettings();
    }
}
