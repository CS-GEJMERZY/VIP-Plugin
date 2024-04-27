namespace Core.Models;

public class PlayerServiceData
{
    public int Id { get; set; }
    public ServiceAvailability Availability { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<string> Flags { get; set; } = [];
    public string GroupId { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public static List<string> FlagsToList(string flags)
    {
        return string.IsNullOrEmpty(flags) ? ([]) : new List<string>(flags.Split(','));
    }
}

