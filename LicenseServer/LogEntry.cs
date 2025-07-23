using System;

namespace LicenseServer;

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = "";
    public LogType Type { get; set; }
}