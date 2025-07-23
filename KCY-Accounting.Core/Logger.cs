namespace KCY_Accounting.Core;

public static class Logger
{
    public const string LOG_FILE_PATH = "resources/logs/log.txt";
    public const string APP_DATA_PATH = "resources/appdata/key-data.cache";

    public static void Log(string message, LogType type)
    {
        switch (type)
        {
            case LogType.Console:
                Console.WriteLine(message);
                File.AppendAllText(LOG_FILE_PATH, $"{DateTime.Now}: {message}\n");
                break;
            case LogType.LogFile:
                File.AppendAllText(LOG_FILE_PATH, $"{DateTime.Now}: {message}\n");
                break;
            case LogType.AppData:
                File.WriteAllText(APP_DATA_PATH, message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    public static void Warn(string message, LogType type = LogType.Console)
    {
        Log($"[WARN] {message}", type);
    }

    public static void Error(string message, LogType type = LogType.Console)
    {
        Log($"[ERROR] {message}", type);
        throw new Exception($"{message}");
    }
}
