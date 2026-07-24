using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;

namespace ZomboidRCON.Services;

public static class ItemPresetStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static List<ItemPreset> _builtInPresets = [];
    private static ItemPresetUserData _userData = new();
    private static List<ItemPreset> _mergedPresets = [];
    private static bool _loaded;

    public static IReadOnlyList<ItemPreset> All
    {
        get
        {
            if (!_loaded)
                Reload();
            return _mergedPresets;
        }
    }

    public static ItemPreset? GetBuiltIn(string id) =>
        _builtInPresets.FirstOrDefault(p => p.Id == id);

    public static string GetItemDisplayName(string itemId)
    {
        var item = ItemCatalog.All.FirstOrDefault(i => i.Id == itemId);
        return item?.Name ?? itemId;
    }

    public static string FormatEntry(ItemPresetEntry entry)
    {
        var name = GetItemDisplayName(entry.Id);
        return entry.Count > 1 ? $"{name} x{entry.Count}" : name;
    }

    public static void Reload()
    {
        _loaded = true;
        LoadBuiltInPresets();
        LoadUserData();
        MergePresets();
    }

    public static void SaveOverride(ItemPreset preset)
    {
        preset.IsBuiltIn = GetBuiltIn(preset.Id) != null;
        _userData.Overrides.RemoveAll(p => p.Id == preset.Id);
        _userData.Overrides.Add(preset);
        _userData.HiddenBuiltInIds.Remove(preset.Id);
        PersistUserData();
        MergePresets();
    }

    public static void SaveCustom(ItemPreset preset)
    {
        preset.IsBuiltIn = false;
        preset.Group = ItemPresetGroup.Custom;
        if (string.IsNullOrWhiteSpace(preset.Id))
            preset.Id = $"custom.{Guid.NewGuid():N}";

        _userData.Custom.RemoveAll(p => p.Id == preset.Id);
        _userData.Custom.Add(preset);
        PersistUserData();
        MergePresets();
    }

    public static void DeletePreset(ItemPreset preset)
    {
        if (preset.IsBuiltIn || GetBuiltIn(preset.Id) != null)
        {
            _userData.Overrides.RemoveAll(p => p.Id == preset.Id);
            if (!_userData.HiddenBuiltInIds.Contains(preset.Id))
                _userData.HiddenBuiltInIds.Add(preset.Id);
        }
        else
        {
            _userData.Custom.RemoveAll(p => p.Id == preset.Id);
        }

        PersistUserData();
        MergePresets();
    }

    public static void ResetBuiltIn(string id)
    {
        _userData.Overrides.RemoveAll(p => p.Id == id);
        _userData.HiddenBuiltInIds.Remove(id);
        PersistUserData();
        MergePresets();
    }

    public static bool IsOverridden(string id) =>
        _userData.Overrides.Any(p => p.Id == id);

    public static bool IsHidden(string id) =>
        _userData.HiddenBuiltInIds.Contains(id);

    private static void LoadBuiltInPresets()
    {
        _builtInPresets = [];
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ZomboidRCON.Resources.default_item_presets.json");
            if (stream == null)
            {
                AppLog.Log("ItemPresetStore", "Embedded default presets not found");
                return;
            }

            using var document = JsonDocument.Parse(stream);
            if (!document.RootElement.TryGetProperty("presets", out var presetsElement))
                return;

            foreach (var element in presetsElement.EnumerateArray())
                _builtInPresets.Add(ParsePreset(element, isBuiltIn: true));

            AppLog.Log("ItemPresetStore", $"Loaded {_builtInPresets.Count} built-in presets");
        }
        catch (Exception ex)
        {
            AppLog.Log("ItemPresetStore", $"Failed to load built-in presets: {ex.Message}");
        }
    }

    private static void LoadUserData()
    {
        _userData = new ItemPresetUserData();
        var path = GetUserPresetsPath();
        if (!File.Exists(path))
            return;

        try
        {
            var json = File.ReadAllText(path);
            _userData = JsonSerializer.Deserialize<ItemPresetUserData>(json, JsonOptions) ?? new ItemPresetUserData();
        }
        catch (Exception ex)
        {
            AppLog.Log("ItemPresetStore", $"Failed to load user presets: {ex.Message}");
            _userData = new ItemPresetUserData();
        }
    }

    private static void PersistUserData()
    {
        try
        {
            var path = GetUserPresetsPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(_userData, JsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            AppLog.Log("ItemPresetStore", $"Failed to save user presets: {ex.Message}");
        }
    }

    private static void MergePresets()
    {
        var merged = new Dictionary<string, ItemPreset>();

        foreach (var preset in _builtInPresets)
        {
            if (_userData.HiddenBuiltInIds.Contains(preset.Id))
                continue;
            merged[preset.Id] = preset.Clone();
        }

        foreach (var preset in _userData.Overrides)
            merged[preset.Id] = preset.Clone();

        foreach (var preset in _userData.Custom)
            merged[preset.Id] = preset.Clone();

        _mergedPresets = merged.Values
            .OrderBy(p => p.Group)
            .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static ItemPreset ParsePreset(JsonElement element, bool isBuiltIn)
    {
        var groupText = element.GetProperty("group").GetString() ?? "Custom";
        Enum.TryParse<ItemPresetGroup>(groupText, out var group);

        var preset = new ItemPreset
        {
            Id = element.GetProperty("id").GetString() ?? "",
            Name = element.GetProperty("name").GetString() ?? "",
            Group = group,
            IsBuiltIn = isBuiltIn
        };

        if (element.TryGetProperty("items", out var itemsElement))
        {
            foreach (var itemElement in itemsElement.EnumerateArray())
            {
                preset.Items.Add(new ItemPresetEntry
                {
                    Id = itemElement.GetProperty("id").GetString() ?? "",
                    Count = itemElement.TryGetProperty("count", out var countElement)
                        ? countElement.GetInt32()
                        : 1
                });
            }
        }

        return preset;
    }

    private static string GetUserPresetsPath()
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

        return Path.Combine(dir, "item_presets.json");
    }
}
