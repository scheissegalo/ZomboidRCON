namespace ZomboidRCON.Models;

public enum ItemPresetGroup
{
    Profession,
    Survival,
    Custom
}

public class ItemPresetEntry
{
    public string Id { get; set; } = "";
    public int Count { get; set; } = 1;
}

public class ItemPreset
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public ItemPresetGroup Group { get; set; }
    public bool IsBuiltIn { get; set; }
    public List<ItemPresetEntry> Items { get; set; } = [];

    public string DisplayLabel => Group == ItemPresetGroup.Custom
        ? Name
        : $"[{Group}] {Name}";

    public ItemPreset Clone() => new()
    {
        Id = Id,
        Name = Name,
        Group = Group,
        IsBuiltIn = IsBuiltIn,
        Items = Items.Select(i => new ItemPresetEntry { Id = i.Id, Count = i.Count }).ToList()
    };
}

public class ItemPresetUserData
{
    public List<ItemPreset> Overrides { get; set; } = [];
    public List<ItemPreset> Custom { get; set; } = [];
    public List<string> HiddenBuiltInIds { get; set; } = [];
}
