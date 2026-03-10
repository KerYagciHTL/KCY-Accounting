using System.Text;
using Avalonia;
using KCY_Accounting.UI;

namespace KCY_Accounting;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Force UTF-8 globally so umlauts (ä, ö, ü) and special characters
        // are never corrupted when read from or written to SQLite.
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding  = Encoding.UTF8;

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}