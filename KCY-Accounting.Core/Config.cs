namespace KCY_Accounting.Core;

public static class Config
{
    public static string Version { get; private set; } = string.Empty;
    public static string UserName { get; private set; } = string.Empty;

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

    public static async Task<bool> WasLoggedIn()
    {
        if (!File.Exists(Logger.APP_DATA_PATH))
        {
            Logger.Warn("App data file not found, assuming not logged in.");
            return false;
        }

        var lines = await File.ReadAllLinesAsync(Logger.APP_DATA_PATH);
        if (lines.Length == 0)
            return false;

        var licenseKey = lines[0];
        if (string.IsNullOrWhiteSpace(licenseKey))
            return false;

        return await Client.IsValidLicenseAsync(licenseKey);
    }

    private static async Task<bool> ValidVersionAsync()
    {
        var serverVersion = await Client.GetVersion();
        return serverVersion == Version;
    }

    public static void SetUserName(string userName)
    {
        UserName = userName;
    }
}