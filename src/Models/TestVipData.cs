namespace Core.Models;

public class TestVipData
{
    public int Id { get; set; } = -1;
    public int PlayerId { get; set; }
    public TestVipMode Mode { get; set; }
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
    public int? TimeLeft { get; set; }
    public bool Completed { get; set; }
}
