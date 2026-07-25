using System.Reflection;
using System.Text.Json;
using ZomboidRCON.Helpers;
using ZomboidRCON.Models;

namespace ZomboidRCON.Services;

public static class VehicleCatalog
{
    private static List<Vehicle> _vehicles = [];
    private static bool _loaded;

    public static IReadOnlyList<Vehicle> All
    {
        get
        {
            if (!_loaded)
                Load();
            return _vehicles;
        }
    }

    public static void Load()
    {
        _vehicles = [];
        _loaded = true;

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("ZomboidRCON.Resources.default_vehicles.json");
            if (stream == null)
            {
                AppLog.Log("VehicleCatalog", "Embedded vehicle list not found");
                return;
            }

            using var document = JsonDocument.Parse(stream);
            if (!document.RootElement.TryGetProperty("vehicles", out var vehiclesElement))
            {
                AppLog.Log("VehicleCatalog", "Vehicle list JSON missing vehicles array");
                return;
            }

            foreach (var vehicleElement in vehiclesElement.EnumerateArray())
            {
                var name = vehicleElement.GetProperty("name").GetString() ?? "";
                if (!vehicleElement.TryGetProperty("variants", out var variantsElement))
                    continue;

                var variants = new List<Variant>();
                foreach (var variantElement in variantsElement.EnumerateArray())
                {
                    var title = variantElement.GetProperty("title").GetString() ?? "Normal";
                    var variantId = variantElement.GetProperty("variantId").GetString() ?? "";
                    if (string.IsNullOrWhiteSpace(variantId))
                        continue;

                    variants.Add(new Variant
                    {
                        Title = title,
                        VariantID = variantId,
                        imageID = variantId,
                        isStock = true,
                    });
                }

                if (variants.Count == 0)
                    continue;

                _vehicles.Add(new Vehicle
                {
                    Name = name,
                    Variants = variants.ToArray(),
                    isStock = true,
                });
            }

            _vehicles = _vehicles
                .OrderBy(v => v.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var variantCount = _vehicles.Sum(v => v.Variants?.Length ?? 0);
            AppLog.Log("VehicleCatalog", $"Loaded {variantCount} variants in {_vehicles.Count} groups");
        }
        catch (Exception ex)
        {
            AppLog.Log("VehicleCatalog", $"Failed to load vehicles: {ex.Message}");
        }
    }
}
