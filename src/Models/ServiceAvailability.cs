namespace Core.Models;

public enum ServiceAvailability
{
    Disabled = 1 << 0,
    Enabled = 1 << 1,
    Expired = 1 << 2
}
