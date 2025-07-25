using System.Globalization;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KCY_Accounting.Core;

public static class Config
{
    private static readonly string CacheFilePath = "resources/appdata/data.cache";
    public static string Version { get; private set; } = string.Empty;
    public static string UserName { get; private set; } = string.Empty;
    public static string LicenseKey { get; private set; } = string.Empty;
    public static string McAddress { get; private set; } = string.Empty;
    public static bool ShowedAgbs { get; private set; }
    public static DateTime DateOfToSChanged { get; private set; } = DateTime.MaxValue;

    private static readonly object Lock = new();
    private static bool _initialized;

    public static async Task InitializeAsync(string version)
    {
        lock (Lock)
        {
            if (_initialized)
                return;

            Version = version;
            _initialized = true;
        }

        await LoadConfigFromFileAsync();

        if (!await ValidVersionAsync())
        {
            // Reset in case of failure
            lock (Lock)
            {
                _initialized = false;
                Version = string.Empty;
            }
            throw new Exception("Invalid version");
        }
    }

    private static async Task LoadConfigFromFileAsync()
    {
        try
        {
            if (!File.Exists(CacheFilePath))
            {
                Logger.Log("Cache file not found, creating default config.");
                await CreateDefaultConfigAsync();
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(CacheFilePath);
            var configData = JsonSerializer.Deserialize<ConfigData>(jsonContent);

            if (configData != null)
            {
                UserName = configData.Username;
                LicenseKey = configData.LicenseKey;
                ShowedAgbs = configData.ShowedAgbs;
                McAddress = configData.McAddress;
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Error loading config from cache file: {ex.Message}");
            await CreateDefaultConfigAsync();
        }
    }

    public static async Task LogoutAsync()
    {
        await Client.ClearMcAddress();
        await CreateDefaultConfigAsync();
        
        UserName = string.Empty;
        LicenseKey = string.Empty;
        ShowedAgbs = false;
        McAddress = string.Empty;
        Version = string.Empty;
    }

    private static async Task CreateDefaultConfigAsync()
    {
        var defaultConfig = new ConfigData
        {
            Username = "",
            LicenseKey = "",
            ShowedAgbs = false,
            McAddress = "",
            AppVersion = Version
        };

        await SaveConfigAsync(defaultConfig);
    }

    private static async Task SaveConfigAsync(ConfigData config)
    {
        try
        {
            var directory = Path.GetDirectoryName(CacheFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var jsonContent = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(CacheFilePath, jsonContent);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Error saving config to cache file: {ex.Message}");
        }
    }

    public static async Task<bool> WasLoggedIn()
    {
        if (!string.IsNullOrWhiteSpace(LicenseKey)) return await Client.IsValidLicenseAsync(LicenseKey, McAddress);
        Logger.Warn("No license key found, assuming not logged in.");
        return false;

    }

    public static async Task UpdateLicenseKeyAsync(string licenseKey)
    {
        Logger.Log("Updating LicenseKey to: " + licenseKey);
        LicenseKey = licenseKey;
        await UpdateConfigFileAsync();
    }

    public static async Task UpdateShowedAgbsAsync(bool showed)
    {
        Logger.Log("Updating ShowedAgbs to: " + showed);
        ShowedAgbs = showed;
        await UpdateConfigFileAsync();
    }

    public static async Task UpdateMcAddressAsync(string mcAddress)
    {
        Logger.Log("Updating MC Address to: " + mcAddress);
        if (McAddress != string.Empty)
        {
            return; // McAddress should not be allowed to change once set
        }
        McAddress = mcAddress;
        await UpdateConfigFileAsync();
    }
    public static async Task UpdateUserNameAsync()
    {
        Logger.Log("Changing Username");
        var userName = await Client.GetUserName();
        UserName = userName;
        await UpdateConfigFileAsync();
    }

    private static async Task UpdateConfigFileAsync()
    {
        var config = new ConfigData
        {
            Username = UserName,
            LicenseKey = LicenseKey,
            ShowedAgbs = ShowedAgbs,
            McAddress = McAddress,
            AppVersion = Version
        };

        await SaveConfigAsync(config);
    }

    private static async Task<bool> ValidVersionAsync()
    {
        var serverVersion = await Client.GetVersion();
        return serverVersion == Version;
    }
    
    private class ConfigData
    {
        [JsonPropertyName("username")]
        public string Username { get; init; } = string.Empty;
        
        [JsonPropertyName("licensekey")]
        public string LicenseKey { get; init; } = string.Empty;
        
        [JsonPropertyName("showedAgbs")]  
        public bool ShowedAgbs { get; init; }
        
        [JsonPropertyName("mc-adress")]
        public string McAddress { get; init; } = string.Empty;
        
        [JsonPropertyName("app-version")]
        public string AppVersion { get; init; } = string.Empty;
    }
}