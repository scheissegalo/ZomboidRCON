namespace ZomboidRCON.Models;

public class TeleportLocation
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; } = 0;
}
