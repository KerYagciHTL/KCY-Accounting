using System;

namespace LicenseServer;

public class Config
{
    public string Version { get; set; } = "1.0.0";
    public string LicenseFilePath { get; set; } = "licenses.json";
    public int Port { get; set; } = 4053;
    public string IpAddress { get; set; } = Environment.GetEnvironmentVariable("IP-ADDRESS")!;
}