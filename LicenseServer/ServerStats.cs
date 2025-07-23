using System;

namespace LicenseServer;

public class ServerStats
{
    public int TotalRequests { get; set; }
    public int ValidRequests { get; set; }
    public int InvalidRequests { get; set; }
    public DateTime StartTime { get; set; }
    public bool IsRunning { get; set; }
}