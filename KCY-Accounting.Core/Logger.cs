namespace KCY_Accounting.Core;

using System.Globalization;

public static class Logger
{
    private const string LOG_FILE_PATH = "resources/logs/log.txt";
    
    // Reuse a single StreamWriter to avoid reopening the file on every log write (I/O + allocation savings)
    private static readonly object _sync = new();
    private static StreamWriter? _writer;
    private static bool _initialized;
    private static Timer? _flushTimer;
    private const int FLUSH_INTERVAL_MS = 5000; // periodic flush safety net
    private static bool _useUtc; // configurable timestamp mode
    private static string _timestampFormat = "yyyy-MM-dd HH:mm:ss.fff"; // mutable format

    public static void UseUtcTimestamps(bool enabled)
    {
        lock (_sync)
        {
            _useUtc = enabled;
        }
    }

    public static void SetTimestampFormat(string format)
    {
        if (string.IsNullOrWhiteSpace(format)) return;
        try
        {
            // Test format to ensure it's valid (will throw if invalid)
            _ = DateTime.Now.ToString(format, CultureInfo.InvariantCulture);
            lock (_sync)
            {
                _timestampFormat = format;
            }
        }
        catch
        {
            // Ignore invalid custom format
        }
    }

    public static void Init()
    {
        lock (_sync)
        {
            if (_initialized) return;
            var directory = Path.GetDirectoryName(LOG_FILE_PATH);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // Open file in append mode so logs persist across sessions (avoid truncation each run)
            var fs = new FileStream(LOG_FILE_PATH, FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
            _writer = new StreamWriter(fs) { AutoFlush = false }; // manual flush for batching
            _flushTimer = new Timer(static _ => SafeFlush(), null, FLUSH_INTERVAL_MS, FLUSH_INTERVAL_MS);
            var env = Environment.GetEnvironmentVariable("LOG_USE_UTC");
            if (!string.IsNullOrWhiteSpace(env) && bool.TryParse(env, out var useUtc))
            {
                _useUtc = useUtc;
            }
            _initialized = true;
            Console.WriteLine($"Logger initialized -> {LOG_FILE_PATH} (UTC={_useUtc})");
        }
    }

    private static void SafeFlush()
    {
        try { Flush(); } catch { /* ignore background flush errors */ }
    }

    private static void WriteLine(string level, string message)
    {
        var now = _useUtc ? DateTime.UtcNow : DateTime.Now;
        string ts;
        lock (_sync)
        {
            ts = now.ToString(_timestampFormat, CultureInfo.InvariantCulture);
        }
        var line = $"{ts} {level} {message}"; // human-friendly format
        lock (_sync)
        {
            if (_writer == null)
            {
                // Fallback init if someone logs before Init() explicitly called
                Init();
            }
            Console.WriteLine(line);
            _writer!.WriteLine(line);
        }
    }

    public static void Flush()
    {
        lock (_sync)
        {
            _writer?.Flush();
        }
    }

    public static void Shutdown()
    {
        lock (_sync)
        {
            try
            {
                _flushTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                _flushTimer?.Dispose();
                _writer?.Flush();
                _writer?.Dispose();
            }
            finally
            {
                _flushTimer = null;
                _writer = null;
                _initialized = false;
            }
        }
    }

    public static void Log(string message) => WriteLine("[INFO]", message);

    public static void Warn(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        WriteLine("[WARN]", message);
        Console.ResetColor();
    }

    public static void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        WriteLine("[ERROR]", message);
        Console.ResetColor();
    }
}
