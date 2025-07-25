using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Layout;
using KCY_Accounting.Interfaces;
using Avalonia;
using KCY_Accounting.Core;

namespace KCY_Accounting.Views;

public class LoadingView : UserControl, IView
{
    public string Title => "KCY-Accounting - Lade Konfiguration";
    public WindowIcon Icon => new("resources/pictures/loading-icon.ico");
    public event EventHandler<ViewType>? NavigationRequested;
    public void Init()
    {
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 35
        };

        var logoContainer = CreateLogoContainer();
        mainPanel.Children.Add(logoContainer);

        var titleBlock = new TextBlock
        {
            Text = "KCY-Accounting",
            FontSize = 32,
            FontWeight = FontWeight.Light,
            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        mainPanel.Children.Add(titleBlock);

        var statusText = new TextBlock
        {
            Text = "Lade Konfiguration...",
            FontSize = 16,
            FontWeight = FontWeight.Normal,
            Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 190)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 25)
        };
        mainPanel.Children.Add(statusText);

        var progressBar = new ProgressBar
        {
            Width = 350,
            Height = 8,
            IsIndeterminate = true,
            Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
            Background = new SolidColorBrush(Color.FromRgb(40, 40, 50))
        };
        mainPanel.Children.Add(progressBar);

        var footerText = new TextBlock
        {
            Text = "Einen Moment bitte...",
            FontSize = 12,
            FontWeight = FontWeight.Light,
            Foreground = new SolidColorBrush(Color.FromRgb(120, 120, 130)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 30, 0, 0)
        };
        mainPanel.Children.Add(footerText);

        Content = mainPanel;
    }

    private Panel CreateLogoContainer()
    {
        var container = new Grid
        {
            Width = 80,
            Height = 80,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var outerRing = new Ellipse
        {
            Width = 80,
            Height = 80,
            Stroke = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
            StrokeThickness = 2,
            Opacity = 0.3
        };

        var innerCircle = new Ellipse
        {
            Width = 60,
            Height = 60,
            Fill = new RadialGradientBrush
            {
                GradientStops =
                [
                    new GradientStop(Color.FromRgb(100, 150, 255), 0),
                    new GradientStop(Color.FromRgb(80, 120, 200), 1)
                ]
            }
        };

        var logoText = new TextBlock
        {
            Text = "KCY",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        container.Children.Add(outerRing);
        container.Children.Add(innerCircle);
        container.Children.Add(logoText);

        return container;
    }

    public void Dispose()
    {
        (Content as Panel)?.Children.Clear();
        Content = null;
        
        Logger.Log("LoadingView disposed.");
    }
}