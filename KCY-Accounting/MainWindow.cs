using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using KCY_Accounting.Core;
using KCY_Accounting.Views;
using KCY_Accounting.Interfaces;
using KCY_Accounting.Logic;

namespace KCY_Accounting;

public class MainWindow : Window
{
    private const int THROTTLE_DELAY_MS = 2000;
    private const string APP_VERSION = "1.0.0";
    
    private readonly ContentControl _mainContent = new();
    private IView? _currentView;
    private bool _isInitialized;

    private static readonly LinearGradientBrush DefaultBackgroundBrush = new()
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        [
            new GradientStop(Color.FromRgb(18, 18, 18), 0),
            new GradientStop(Color.FromRgb(25, 25, 35), 1)
        ]
    };

    public MainWindow()
    {
        InitializeWindow();
        SwitchView(new LoadingView());
        Loaded += OnWindowLoaded;
    }

    private void InitializeWindow()
    {
        Logger.Init();
        Width = 1000;
        Height = 600;
        Background = DefaultBackgroundBrush;
        Content = _mainContent;
    }

    private void SubscribeToCurrentViewEvents()
    {
        if (_currentView == null)
        {
            Logger.Warn("View is null. Cannot subscribe.");
            return;
        }

        _currentView.NavigationRequested += HandleNavigation;
    }

    private void UnsubscribeFromCurrentViewEvents()
    {
        if (_currentView == null)
        {
            if (!_isInitialized)
            {
                Logger.Log("Application is starting");
                return;
            }
            
            Logger.Warn("View is null. Cannot unsubscribe.");
            return;
        }

        _currentView.NavigationRequested -= HandleNavigation;
    }

    private async void HandleNavigation(object? sender, ViewType type)
    {
        try
        {
            var newView = CreateView(type);
            if (newView == null)
            {
                Close();
                return;
            }

            SwitchView(newView);
        }
        catch (Exception ex)
        {
            await HandleInitializationError(ex);
        }
    }

    private static IView? CreateView(ViewType type) => type switch
    {
        ViewType.Order => new OrderView(),
        ViewType.Customer => new CustomerView(),
        ViewType.Main => new MainView(),
        ViewType.Welcome => new WelcomeView(),
        ViewType.Loading => new LoadingView(),
        ViewType.License => new LicenseView(),
        ViewType.ToS => new ToSView(),
        ViewType.None => null,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown view type")
    };

    private void SwitchView(IView newView)
    {
        UnsubscribeFromCurrentViewEvents();

        try
        {
            _currentView?.Dispose();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error disposing old view: {ex.Message}");
        }

        newView.Init();
        _mainContent.Content = newView;
        _currentView = newView;

        SubscribeToCurrentViewEvents();
        UpdateWindowProperties(newView);
    }

    private void UpdateWindowProperties(IView view)
    {
        Title = view.Title;
        Icon = view.Icon;
    }

    private async void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_isInitialized) return;

            _isInitialized = true;

            try
            {
                await InitializeApplicationAsync();
            }
            catch (Exception ex)
            {
                await HandleInitializationError(ex);
            }
        }
        catch (Exception ex)
        {
            await HandleInitializationError(ex);
        }
    }

    private async Task InitializeApplicationAsync()
    {
        await Config.InitializeAsync(APP_VERSION);
        await Task.Delay(THROTTLE_DELAY_MS);

        var initialView = await DetermineInitialView();
        SwitchView(initialView);
    }

    private static async Task<IView> DetermineInitialView()
    {
        var wasLoggedIn = await Config.WasLoggedIn();
        if (!wasLoggedIn) return new LicenseView();

        return Config.ShowedAgbs ? new WelcomeView() : new ToSView();
    }

    private async Task HandleInitializationError(Exception ex)
    {
        Logger.Error($"Fehler beim Laden der Konfiguration: {ex.Message}");
        await MessageBox.ShowError("Initialisierungsfehler", ex.Message);
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        UnsubscribeFromCurrentViewEvents();
        base.OnClosed(e);
    }
}