namespace KCY_Accounting.Core;

public static class Logger
{
    private const string LOG_FILE_PATH = "resources/logs/log.txt";

    public static void Init()
    {
        var directory = Path.GetDirectoryName(LOG_FILE_PATH);
        if (!Directory.Exists(directory))
        {
            if (directory != null) Directory.CreateDirectory(directory);
        }
    
        Console.WriteLine(LOG_FILE_PATH + " created");
        using var fs = File.Create(LOG_FILE_PATH);
        fs.Close();
    }

    public static void Log(string message)
    {
        Console.WriteLine(message);
        File.AppendAllText(LOG_FILE_PATH, $"{DateTime.Now}: {message}\n");
    }

    public static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Log($"[WARN] {message}");
        Console.ResetColor();
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Log($"[ERROR] {message}");
        Console.ResetColor();
    }
}
