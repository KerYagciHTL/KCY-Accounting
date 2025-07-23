using System.Collections.Generic;

namespace LicenseServer;

public class LicenseEntry
{
    public string Name { get; set; } = "";
    public string LicenseKey { get; set; } = "";
    public int AllowedUsers { get; set; }
    public int RedeemedUsers { get; set; }
    public List<string> AllowedMacs { get; set; } = new();
}