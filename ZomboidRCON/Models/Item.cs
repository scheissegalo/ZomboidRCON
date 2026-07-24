namespace ZomboidRCON.Models
{
    public enum ItemType
    {
        Clothing,
        Weapons,
        Food,
        Medical,
        Materials,
        Tools,
        Electronics,
        Literature,
        Camping,
        Trapping,
        Containers,
        Miscellaneous
    }

    public class Item
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public ItemType Type { get; set; }
        public Variant[]? Variants { get; set; }
        public bool isStock { get; set; }
        public Uri? Wiki { get; set; }

        public string DisplayText => $"{Name} ({Id})";

        public override string ToString() => DisplayText;
    }
}
