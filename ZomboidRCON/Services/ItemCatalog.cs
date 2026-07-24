using System.Reflection;
using System.Text.Json;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;

namespace ZomboidRCON.Services;

public static class ItemCatalog
{
    private static List<Item> _items = [];
    private static List<ItemType> _categories = [];
    private static bool _loaded;

    public static IReadOnlyList<Item> All
    {
        get
        {
            if (!_loaded)
                Load();
            return _items;
        }
    }

    public static IReadOnlyList<ItemType> Categories
    {
        get
        {
            if (!_loaded)
                Load();
            return _categories;
        }
    }

    public static void Load()
    {
        _items = [];
        _categories = [];
        _loaded = true;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ZomboidRCON.Resources.pz_items.json");
            if (stream == null)
            {
                AppLog.Log("ItemCatalog", "Embedded item list not found");
                return;
            }

            using var document = JsonDocument.Parse(stream);
            if (!document.RootElement.TryGetProperty("items", out var itemsElement))
            {
                AppLog.Log("ItemCatalog", "Item list JSON missing items array");
                return;
            }

            foreach (var element in itemsElement.EnumerateArray())
            {
                var name = element.GetProperty("name").GetString() ?? "";
                var id = element.GetProperty("id").GetString() ?? "";
                var categoryText = element.GetProperty("category").GetString() ?? "Miscellaneous";

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                if (!Enum.TryParse<ItemType>(categoryText, out var category))
                    category = ItemType.Miscellaneous;

                _items.Add(new Item
                {
                    Name = name,
                    Id = id,
                    Type = category,
                    isStock = true
                });
            }

            _items = _items
                .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(i => i.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _categories = _items
                .Select(i => i.Type)
                .Distinct()
                .OrderBy(c => c.ToString())
                .ToList();

            AppLog.Log("ItemCatalog", $"Loaded {_items.Count} items in {_categories.Count} categories");
        }
        catch (Exception ex)
        {
            AppLog.Log("ItemCatalog", $"Failed to load items: {ex.Message}");
        }
    }

    public static IEnumerable<Item> Search(string? query, ItemType? category = null)
    {
        var items = All.AsEnumerable();

        if (category.HasValue)
            items = items.Where(i => i.Type == category.Value);

        if (!string.IsNullOrWhiteSpace(query))
        {
            items = items.Where(i =>
                i.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                i.Id.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        return items;
    }
}
