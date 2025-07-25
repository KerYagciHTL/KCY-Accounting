using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Raylib_cs;
using DotNetEnv;
using static Raylib_cs.Raylib;

namespace LicenseServer;

public static class Program
{
    private static Config _config = new();
    private static readonly List<LogEntry> Logs = [];
    private static readonly ServerStats Stats = new();
    private static TcpListener? _server;
    private static Thread? _serverThread;
    private static readonly Lock LogLock = new();

    // UI Constants
    private const int WINDOW_WIDTH = 1200;
    private const int WINDOW_HEIGHT = 800;
    private const int PANEL_MARGIN = 20;
    private const int LOG_HEIGHT = 300;

    // Colors
    private static readonly Color BACKGROUND = new(25, 25, 35, 255);
    private static readonly Color PANEL = new(35, 35, 50, 255);
    private static readonly Color ACCENT = new(70, 130, 180, 255);
    private static readonly Color SUCCESS = new(72, 187, 120, 255);
    private static readonly Color WARNING = new(245, 158, 11, 255);
    private static readonly Color ERROR = new(239, 68, 68, 255);
    private static readonly Color TEXT = new(245, 245, 245, 255);
    private static readonly Color TEXT_SECONDARY = new(160, 160, 160, 255);

    public static void Main()
    {
        Env.Load(".env");
        InitWindow(WINDOW_WIDTH, WINDOW_HEIGHT, "License Server - Raylib GUI");
        SetTargetFPS(60);

        Stats.StartTime = DateTime.Now;
        AddLog("License Server GUI gestartet", LogType.Info);

        while (!WindowShouldClose())
        {
            Update();
            Draw();
        }

        StopServer();
        CloseWindow();
    }

    private static void LoadConfig()
    {
        try
        {
            if (File.Exists("config.json"))
            {
                var json = File.ReadAllText("config.json");
                _config = JsonSerializer.Deserialize<Config>(json) ?? new Config();
                AddLog("Konfiguration erfolgreich geladen", LogType.Success);
            }
            else
            {
                SaveConfig();
                AddLog("Standard-Konfiguration erstellt", LogType.Info);
            }

            if (_config.IpAddress != "ur ip") return;
            _config.IpAddress = Environment.GetEnvironmentVariable("IP-ADDRESS");
            AddLog("Please change your ip in the same directory in config.json", LogType.Error);
        }
        catch (Exception ex)
        {
            AddLog($"Fehler beim Laden der Konfiguration: {ex.Message}", LogType.Error);
        }
    }

    private static void SaveConfig()
    {
        try
        {
            var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("config.json", json);
        }
        catch (Exception ex)
        {
            AddLog($"Fehler beim Speichern der Konfiguration: {ex.Message}", LogType.Error);
        }
    }

    private static void Update()
    {
        if (IsKeyPressed(KeyboardKey.KEY_SPACE))
        {
            if (Stats.IsRunning)
                StopServer();
            else
                StartServer();
        }

        // Clear logs
        if (IsKeyPressed(KeyboardKey.KEY_C))
        {
            lock (LogLock)
            {
                Logs.Clear();
            }
        }

        // Reload config
        if (IsKeyPressed(KeyboardKey.KEY_R))
        {
            LoadConfig();
        }
    }

    private static void Draw()
    {
        BeginDrawing();
        ClearBackground(BACKGROUND);

        DrawHeader();
        DrawServerStatus();
        DrawStatistics();
        DrawLicenseInfo();
        DrawLogs();
        DrawControls();

        EndDrawing();
    }

    private static void DrawHeader()
    {
        var title = "License Server Management";
        var titleSize = 32;
        var titleWidth = MeasureText(title, titleSize);

        DrawText(title, (WINDOW_WIDTH - titleWidth) / 2, 20, titleSize, ACCENT);
        DrawText($"Version: {_config.Version}", WINDOW_WIDTH - 150, 25, 16, TEXT_SECONDARY);
    }

    private static void DrawServerStatus()
    {
        var panelRect = new Rectangle(PANEL_MARGIN, 70, 350, 120);
        DrawRectangleRounded(panelRect, 0.1f, 8, PANEL);

        var statusColor = Stats.IsRunning ? SUCCESS : ERROR;
        var statusText = Stats.IsRunning ? "LÄUFT" : "GESTOPPT";

        DrawText("Server Status:", (int)panelRect.X + 15, (int)panelRect.Y + 15, 18, TEXT);
        DrawText(statusText, (int)panelRect.X + 15, (int)panelRect.Y + 40, 24, statusColor);

        if (!Stats.IsRunning) return;
        var uptime = DateTime.Now - Stats.StartTime;
        DrawText($"Laufzeit: {uptime:hh\\:mm\\:ss}", (int)panelRect.X + 15, (int)panelRect.Y + 70, 14,
            TEXT_SECONDARY);
        DrawText($"Port: {_config.Port}", (int)panelRect.X + 15, (int)panelRect.Y + 90, 14, TEXT_SECONDARY);
    }

    private static void DrawStatistics()
    {
        var panelRect = new Rectangle(PANEL_MARGIN + 370, 70, 350, 120);
        DrawRectangleRounded(panelRect, 0.1f, 8, PANEL);

        DrawText("Statistiken:", (int)panelRect.X + 15, (int)panelRect.Y + 15, 18, TEXT);
        DrawText($"Gesamt Anfragen: {Stats.TotalRequests}", (int)panelRect.X + 15, (int)panelRect.Y + 40, 14,
            TEXT_SECONDARY);
        DrawText($"Gültige Anfragen: {Stats.ValidRequests}", (int)panelRect.X + 15, (int)panelRect.Y + 60, 14, SUCCESS);
        DrawText($"Ungültige Anfragen: {Stats.InvalidRequests}", (int)panelRect.X + 15, (int)panelRect.Y + 80, 14,
            ERROR);

        if (Stats.TotalRequests > 0)
        {
            var successRate = (Stats.ValidRequests * 100.0) / Stats.TotalRequests;
            DrawText($"Erfolgsrate: {successRate:F1}%", (int)panelRect.X + 15, (int)panelRect.Y + 100, 14,
                TEXT_SECONDARY);
        }
    }

    private static void DrawLicenseInfo()
    {
        var panelRect = new Rectangle(PANEL_MARGIN + 740, 70, 420, 120);
        DrawRectangleRounded(panelRect, 0.1f, 8, PANEL);

        DrawText("Lizenz Übersicht:", (int)panelRect.X + 15, (int)panelRect.Y + 15, 18, TEXT);

        try
        {
            if (File.Exists(_config.LicenseFilePath))
            {
                var json = File.ReadAllText(_config.LicenseFilePath);
                var licenses = JsonSerializer.Deserialize<List<LicenseEntry>>(json) ?? new List<LicenseEntry>();

                var totalLicenses = licenses.Count;
                var totalUsers = licenses.Sum(l => l.AllowedUsers);
                var redeemedUsers = licenses.Sum(l => l.RedeemedUsers);
                var totalMacs = licenses.Sum(l => l.AllowedMacs.Count);

                DrawText($"Lizenzen: {totalLicenses}", (int)panelRect.X + 15, (int)panelRect.Y + 40, 14,
                    TEXT_SECONDARY);
                DrawText($"Erlaubte Nutzer: {totalUsers}", (int)panelRect.X + 15, (int)panelRect.Y + 60, 14,
                    TEXT_SECONDARY);
                DrawText($"Eingelöste Nutzer: {redeemedUsers}", (int)panelRect.X + 220, (int)panelRect.Y + 40, 14,
                    TEXT_SECONDARY);
                DrawText($"Registrierte MACs: {totalMacs}", (int)panelRect.X + 220, (int)panelRect.Y + 60, 14,
                    TEXT_SECONDARY);

                var progress = totalUsers > 0 ? (float)redeemedUsers / totalUsers : 0;
                var progressRect = new Rectangle((int)panelRect.X + 15, (int)panelRect.Y + 85, 390, 20);
                DrawRectangleRounded(progressRect, 0.3f, 4, new Color(50, 50, 70, 255));
                var filledRect = new Rectangle(progressRect.X, progressRect.Y, progressRect.Width * progress,
                    progressRect.Height);
                DrawRectangleRounded(filledRect, 0.3f, 4, ACCENT);

                DrawText($"{progress * 100:F1}% Auslastung", (int)panelRect.X + 170, (int)panelRect.Y + 87, 12, TEXT);
            }
            else
            {
                DrawText("Keine Lizenzdatei gefunden", (int)panelRect.X + 15, (int)panelRect.Y + 40, 14, ERROR);
            }
        }
        catch
        {
            DrawText("Fehler beim Lesen der Lizenzen", (int)panelRect.X + 15, (int)panelRect.Y + 40, 14, ERROR);
        }
    }

    private static void DrawLogs()
    {
        var panelRect = new Rectangle(PANEL_MARGIN, 210, WINDOW_WIDTH - 2 * PANEL_MARGIN, LOG_HEIGHT);
        DrawRectangleRounded(panelRect, 0.1f, 8, PANEL);

        DrawText("Server Logs:", (int)panelRect.X + 15, (int)panelRect.Y + 15, 18, TEXT);

        lock (LogLock)
        {
            var visibleLogs = Math.Min(Logs.Count, 15);
            var startIndex = Math.Max(0, Logs.Count - visibleLogs);

            for (int i = 0; i < visibleLogs; i++)
            {
                var log = Logs[startIndex + i];
                var y = (int)panelRect.Y + 45 + (i * 16);

                var color = log.Type switch
                {
                    LogType.Success => SUCCESS,
                    LogType.Warning => WARNING,
                    LogType.Error => ERROR,
                    _ => TEXT_SECONDARY
                };

                var timeStr = log.Timestamp.ToString("HH:mm:ss");
                DrawText($"[{timeStr}] {log.Message}", (int)panelRect.X + 15, y, 12, color);
            }
        }
    }

    private static void DrawControls()
    {
        var y = WINDOW_HEIGHT - 60;
        var buttonWidth = 150;
        var buttonHeight = 35;
        var buttonSpacing = 20;

        // Start/Stop Button
        var startStopRect = new Rectangle(PANEL_MARGIN, y, buttonWidth, buttonHeight);
        var startStopColor = Stats.IsRunning ? ERROR : SUCCESS;
        var startStopText = Stats.IsRunning ? "STOPPEN (SPACE)" : "STARTEN (SPACE)";

        DrawRectangleRounded(startStopRect, 0.2f, 8, startStopColor);
        var textWidth = MeasureText(startStopText, 14);
        DrawText(startStopText, (int)startStopRect.X + (buttonWidth - textWidth) / 2, (int)startStopRect.Y + 10, 14,
            Color.WHITE);

        // Clear Logs Button
        var clearRect = new Rectangle(PANEL_MARGIN + buttonWidth + buttonSpacing, y, buttonWidth, buttonHeight);
        DrawRectangleRounded(clearRect, 0.2f, 8, WARNING);
        var clearText = "LOGS LÖSCHEN (C)";
        var clearTextWidth = MeasureText(clearText, 14);
        DrawText(clearText, (int)clearRect.X + (buttonWidth - clearTextWidth) / 2, (int)clearRect.Y + 10, 14,
            Color.WHITE);

        // Reload Config Button
        var reloadRect = new Rectangle(PANEL_MARGIN + 2 * (buttonWidth + buttonSpacing), y, buttonWidth, buttonHeight);
        DrawRectangleRounded(reloadRect, 0.2f, 8, ACCENT);
        var reloadText = "CONFIG NEU LADEN (R)";
        var reloadTextWidth = MeasureText(reloadText, 14);
        DrawText(reloadText, (int)reloadRect.X + (buttonWidth - reloadTextWidth) / 2, (int)reloadRect.Y + 10, 14,
            Color.WHITE);

        // Instructions
        DrawText("Tastaturkürzel: SPACE=Start/Stop | C=Logs löschen | R=Config neu laden",
            PANEL_MARGIN, WINDOW_HEIGHT - 20, 12, TEXT_SECONDARY);
    }

    private static void StartServer()
    {
        try
        {
            LoadConfig();
            var ip = IPAddress.Parse(_config.IpAddress);
            _server = new TcpListener(ip, _config.Port);
            _server.Start();

            Stats.IsRunning = true;
            Stats.StartTime = DateTime.Now;

            _serverThread = new Thread(ServerLoop) { IsBackground = true };
            _serverThread.Start();

            AddLog($"Server gestartet auf {_config.IpAddress}:{_config.Port}", LogType.Success);
        }
        catch (Exception ex)
        {
            AddLog($"Fehler beim Starten des Servers: {ex.Message}", LogType.Error);
        }
    }

    private static void StopServer()
    {
        try
        {
            Stats.IsRunning = false;
            _server?.Stop();
            _serverThread?.Join(1000);
            AddLog("Server gestoppt", LogType.Warning);
        }
        catch (Exception ex)
        {
            AddLog($"Fehler beim Stoppen des Servers: {ex.Message}", LogType.Error);
        }
    }

    private static void ServerLoop()
    {
        while (Stats.IsRunning && _server != null)
        {
            try
            {
                var client = _server.AcceptTcpClient();
                var requestThread = new Thread(() => HandleClient(client)) { IsBackground = true };
                requestThread.Start();
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (Stats.IsRunning)
                {
                    AddLog($"Server-Fehler: {ex.Message}", LogType.Error);
                }
            }
        }
    }

    private static void HandleClient(TcpClient client)
    {
        var stopwatch = Stopwatch.StartNew();
        Stats.TotalRequests++;

        try
        {
            using var stream = client.GetStream();
            var buffer = new byte[256];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            var received = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

            if (string.IsNullOrEmpty(received))
            {
                AddLog("Leere Nachricht empfangen", LogType.Warning);
                Stats.InvalidRequests++;
                return;
            }

            // Handle version request
            if (received.Equals("getversion", StringComparison.OrdinalIgnoreCase))
            {
                var versionResponse = Encoding.UTF8.GetBytes(_config.Version);
                stream.Write(versionResponse, 0, versionResponse.Length);
                AddLog($"Version abgefragt: {_config.Version}", LogType.Info);
                Stats.ValidRequests++;
                return;
            }

            // Handle username request (format: "getusername-licensekey-macaddress")
            string? licenseKey;
            string? macAddress;
            if (received.StartsWith("getusername-", StringComparison.OrdinalIgnoreCase))
            {
                var parts = received.Split('-', 3);
                if (parts.Length == 3)
                {
                    licenseKey = parts[1];
                    macAddress = parts[2];
                    var userName = GetUserName(licenseKey, macAddress);

                    if (!string.IsNullOrEmpty(userName))
                    {
                        var userNameResponse = Encoding.UTF8.GetBytes(userName);
                        stream.Write(userNameResponse, 0, userNameResponse.Length);
                        AddLog($"Username abgefragt für {licenseKey}: {userName}", LogType.Info);
                        Stats.ValidRequests++;
                    }
                    else
                    {
                        var errorResponse = "USER_NOT_FOUND"u8.ToArray();
                        stream.Write(errorResponse, 0, errorResponse.Length);
                        AddLog($"Username nicht gefunden für {licenseKey}", LogType.Warning);
                        Stats.InvalidRequests++;
                    }
                }
                else
                {
                    AddLog("Ungültiges getusername Format", LogType.Warning);
                    Stats.InvalidRequests++;
                }

                return;
            }
            else if (received.StartsWith("logout-", StringComparison.OrdinalIgnoreCase))
            {
                var parts = received.Split('-', 3);
                if (parts.Length == 3)
                {
                    licenseKey = parts[1];
                    macAddress = parts[2];

                    var licenses = LoadLicensesFromFile();
                    if (licenses == null)
                    {
                        var errorResponse = "ERROR_LOADING_LICENSES"u8.ToArray();
                        stream.Write(errorResponse, 0, errorResponse.Length);
                        AddLog("Fehler beim Laden der Lizenzdatei während Logout", LogType.Error);
                        Stats.InvalidRequests++;
                        return;
                    }

                    var license = licenses.FirstOrDefault(l => l.LicenseKey == licenseKey);
                    if (license == null)
                    {
                        var errorResponse = "LICENSE_NOT_FOUND"u8.ToArray();
                        stream.Write(errorResponse, 0, errorResponse.Length);
                        AddLog($"Lizenz zum Logout nicht gefunden: {licenseKey}", LogType.Warning);
                        Stats.InvalidRequests++;
                        return;
                    }

                    if (license.AllowedMacs.RemoveAll(m =>
                            string.Equals(m, macAddress, StringComparison.OrdinalIgnoreCase)) > 0)
                    {
                        license.RedeemedUsers = Math.Max(0, license.RedeemedUsers - 1);

                        // Save updated licenses
                        var updatedJson = JsonSerializer.Serialize(licenses,
                            new JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(_config.LicenseFilePath, updatedJson);

                        var successResponse = "LOGOUT_SUCCESS"u8.ToArray();
                        stream.Write(successResponse, 0, successResponse.Length);
                        AddLog($"Logout durchgeführt: {macAddress} von Lizenz {licenseKey}", LogType.Info);
                        Stats.ValidRequests++;
                    }
                    else
                    {
                        var errorResponse = "MAC_NOT_FOUND"u8.ToArray();
                        stream.Write(errorResponse, 0, errorResponse.Length);
                        AddLog($"MAC-Adresse nicht gefunden beim Logout: {macAddress}", LogType.Warning);
                        Stats.InvalidRequests++;
                    }
                }
                else
                {
                    AddLog("Ungültiges logout Format", LogType.Warning);
                    Stats.InvalidRequests++;
                }

                return;
            }

            // Handle license validation (format: "licensekey-macaddress")
            var licenseParts = received.Split('-', 2);
            if (licenseParts.Length != 2)
            {
                AddLog("Ungültiges Nachrichtenformat erhalten", LogType.Warning);
                Stats.InvalidRequests++;
                return;
            }

            licenseKey = licenseParts[0];
            macAddress = licenseParts[1];

            var isValid = ValidateLicense(licenseKey, macAddress) != null;
            var response = Encoding.UTF8.GetBytes(isValid.ToString().ToLower());
            stream.Write(response, 0, response.Length);

            if (isValid)
            {
                AddLog($"Lizenz validiert: {licenseKey} für MAC {macAddress}", LogType.Success);
                Stats.ValidRequests++;
            }
            else
            {
                AddLog($"Lizenz ungültig: {licenseKey} für MAC {macAddress}", LogType.Warning);
                Stats.InvalidRequests++;
            }
        }
        catch (Exception ex)
        {
            AddLog($"Client-Handling Fehler: {ex.Message}", LogType.Error);
            Stats.InvalidRequests++;
        }
        finally
        {
            client.Close();
            stopwatch.Stop();
            AddLog($"Request bearbeitet in {stopwatch.ElapsedMilliseconds}ms", LogType.Info);
        }
    }

    private static string? GetUserName(string key, string mac)
    {
        var license = ValidateLicense(key, mac);
        return license?.Name;
    }

    private static List<LicenseEntry>? LoadLicensesFromFile()
    {
        try
        {
            var json = File.ReadAllText(_config.LicenseFilePath);
            return JsonSerializer.Deserialize<List<LicenseEntry>>(json);
        }
        catch (Exception ex)
        {
            AddLog($"Fehler beim Laden der Lizenzdatei: {ex.Message}", LogType.Error);
            return null;
        }
    }

    private static LicenseEntry? ValidateLicense(string key, string mac)
    {
        if (!File.Exists(_config.LicenseFilePath))
        {
            AddLog("Lizenzdatei nicht gefunden", LogType.Error);
            return null;
        }

        try
        {
            var licenses = LoadLicensesFromFile();
            if (licenses == null) return null;

            foreach (var license in licenses)
            {
                if (license.LicenseKey != key) continue;

                var macExists = license.AllowedMacs.Any(m =>
                    string.Equals(m, mac, StringComparison.OrdinalIgnoreCase));

                // If MAC already exists, license is valid
                if (macExists)
                {
                    return license;
                }

                // If under limit, add new MAC
                if (license.RedeemedUsers < license.AllowedUsers)
                {
                    license.AllowedMacs.Add(mac);
                    license.RedeemedUsers++;

                    // Save updated licenses
                    var updatedJson =
                        JsonSerializer.Serialize(licenses, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_config.LicenseFilePath, updatedJson);

                    AddLog($"Neue MAC registriert: {mac} für {license.Name}", LogType.Success);
                    return license;
                }

                AddLog($"Limit erreicht für Lizenz: {key}", LogType.Warning);
                return null;
            }

            AddLog($"Lizenzschlüssel nicht gefunden: {key}", LogType.Warning);
            return null;
        }
        catch (Exception ex)
        {
            AddLog($"Fehler beim Validieren der Lizenz: {ex.Message}", LogType.Error);
            return null;
        }
    }

    private static void AddLog(string message, LogType type)
    {
        lock (LogLock)
        {
            Logs.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message,
                Type = type
            });

            try
            {
                using var writer = new StreamWriter("log.txt", true);
                writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{type}] {message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Schreiben in die Logdatei: {ex.Message}");
            }

            // Keep only last 100 logs
            if (Logs.Count > 100)
            {
                Logs.RemoveAt(0);
            }
        }
    }
}