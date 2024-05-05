namespace Core.Models;

public class PlayerDatabaseData
{
    public int Id { get; set; }
    public List<PlayerServiceData> Services { get; set; } = [];
    public HashSet<string> AllFlags { get; set; } = [];
}
