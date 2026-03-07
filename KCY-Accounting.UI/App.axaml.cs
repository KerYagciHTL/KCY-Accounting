using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KCY_Accounting.Core.ViewModels;
using KCY_Accounting.UI.Views;

namespace KCY_Accounting.UI;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Hier verknüpfen wir die UI sauber mit der Core-Logik
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}
