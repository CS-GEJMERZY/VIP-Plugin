namespace Core.Models;

public class PlayerTestVipData
{
    public TestVipData? ActiveTestVip { get; set; } = null;
    public DateTime? LastEndTime { get; set; } = null;
    public int UsedCount { get; set; } = 0; // TO:DO load it
}
