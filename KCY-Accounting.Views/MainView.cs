using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using Avalonia.Threading;
using KCY_Accounting.Core;
using KCY_Accounting.Interfaces;

namespace KCY_Accounting.Views;

public class MainView : UserControl, IView
{
    public string Title => "KCY-Accounting";
    public WindowIcon Icon => new("resources/pictures/icon.ico");
    public event EventHandler<ViewType>? NavigationRequested;

    private readonly CancellationTokenSource _disposalTokenSource = new();
    private readonly object _disposeLock = new();
    private volatile bool _isDisposed;
    
    private Button? _orderButton;
    private Button? _customerButton;
    private Border? _orderButtonBorder;
    private Border? _customerButtonBorder;
    private Grid? _mainGrid;
    private TextBlock? _titleText;
    private TextBlock? _subtitleText;

    private static readonly LinearGradientBrush ButtonGradientBrush = new()
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        [
            new GradientStop(Color.FromRgb(45, 45, 55), 0),
            new GradientStop(Color.FromRgb(35, 35, 45), 1)
        ]
    };

    private static readonly LinearGradientBrush ButtonHoverBrush = new()
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        [
            new GradientStop(Color.FromRgb(55, 55, 70), 0),
            new GradientStop(Color.FromRgb(45, 45, 60), 1)
        ]
    };

    public void Init()
    {
        if (_isDisposed) return;
        
        _mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Transitions =
            [
                new DoubleTransition
                {
                    Property = OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(500),
                    Easing = new CubicEaseInOut()
                }
            ]
        };

        // Header Section
        var headerPanel = new StackPanel
        {
            Margin = new Thickness(0, 50, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0
        };

        _titleText = new TextBlock
        {
            Text = "KCY-Accounting",
            FontSize = 42,
            FontWeight = FontWeight.Light,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };

        _subtitleText = new TextBlock
        {
            Text = "Wählen Sie eine Ansicht",
            FontSize = 18,
            FontWeight = FontWeight.Light,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        headerPanel.Children.Add(_titleText);
        headerPanel.Children.Add(_subtitleText);
        Grid.SetRow(headerPanel, 0);
        _mainGrid.Children.Add(headerPanel);

        // Button Container
        var buttonContainer = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            ColumnDefinitions = new ColumnDefinitions("Auto,50,Auto"),
            Opacity = 0
        };

        // Order Button
        _orderButtonBorder = CreateButtonBorder();
        _orderButton = CreateNavigationButton(
            "Auftragsansicht",
            "Verwalten Sie Ihre Aufträge",
            "\uE8CC", // Order icon
            ViewType.Order
        );
        _orderButtonBorder.Child = _orderButton;
        Grid.SetColumn(_orderButtonBorder, 0);

        // Customer Button
        _customerButtonBorder = CreateButtonBorder();
        _customerButton = CreateNavigationButton(
            "Kundenansicht",
            "Verwalten Sie Ihre Kunden",
            "\uE77B", // People icon
            ViewType.Customer
        );
        _customerButtonBorder.Child = _customerButton;
        Grid.SetColumn(_customerButtonBorder, 2);

        buttonContainer.Children.Add(_orderButtonBorder);
        buttonContainer.Children.Add(_customerButtonBorder);
        
        Grid.SetRow(buttonContainer, 1);
        _mainGrid.Children.Add(buttonContainer);
        
        Content = _mainGrid;

        // Start entrance animations
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_isDisposed) return;
            
            await Task.Delay(100);
            AnimateElementIn(headerPanel, 0);
            AnimateElementIn(buttonContainer, 200);
        });
    }

    private Border CreateButtonBorder()
    {
        return new Border
        {
            Background = ButtonGradientBrush,
            BorderBrush = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(10),
            BoxShadow = BoxShadows.Parse("0 8 20 0 #1A000000"),
            Transitions =
            [
                new BoxShadowsTransition
                {
                    Property = Border.BoxShadowProperty,
                    Duration = TimeSpan.FromMilliseconds(200)
                },

                new BrushTransition
                {
                    Property = Border.BackgroundProperty,
                    Duration = TimeSpan.FromMilliseconds(200)
                },

                new TransformOperationsTransition
                {
                    Property = RenderTransformProperty,
                    Duration = TimeSpan.FromMilliseconds(200),
                    Easing = new CubicEaseOut()
                }
            ],
            RenderTransform = TransformOperations.Parse("scale(1)")
        };
    }

    private Button CreateNavigationButton(string title, string subtitle, string icon, ViewType viewType)
    {
        var button = new Button
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(30, 25),
            Cursor = new Cursor(StandardCursorType.Hand),
            Tag = viewType,
            Transitions = new Transitions
            {
                new DoubleTransition
                {
                    Property = OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(150)
                }
            }
        };

        var contentGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            Width = 200
        };

        var iconText = new TextBlock
        {
            Text = icon,
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 48,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 15),
            Transitions =
            [
                new TransformOperationsTransition
                {
                    Property = RenderTransformProperty,
                    Duration = TimeSpan.FromMilliseconds(300),
                    Easing = new ElasticEaseOut()
                }
            ],
            RenderTransform = TransformOperations.Parse("scale(1)")
        };

        var textPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var subtitleText = new TextBlock
        {
            Text = subtitle,
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        textPanel.Children.Add(titleText);
        textPanel.Children.Add(subtitleText);

        Grid.SetRow(iconText, 0);
        Grid.SetRow(textPanel, 1);
        
        contentGrid.Children.Add(iconText);
        contentGrid.Children.Add(textPanel);

        button.Content = contentGrid;
        
        // Wire up events
        button.Click += OnButtonClicked;
        
        button.PointerEntered += (_, _) =>
        {
            if (_isDisposed) return;
            if (button.Parent is Border border)
            {
                border.Background = ButtonHoverBrush;
                border.BoxShadow = BoxShadows.Parse("0 12 30 0 #2A000000");
                border.RenderTransform = TransformOperations.Parse("scale(1.05) translateY(-5px)");
            }
            iconText.RenderTransform = TransformOperations.Parse("scale(1.2) rotate(5deg)");
        };

        button.PointerExited += (_, _) =>
        {
            if (_isDisposed) return;
            if (button.Parent is Border border)
            {
                border.Background = ButtonGradientBrush;
                border.BoxShadow = BoxShadows.Parse("0 8 20 0 #1A000000");
                border.RenderTransform = TransformOperations.Parse("scale(1)");
            }
            iconText.RenderTransform = TransformOperations.Parse("scale(1)");
        };

        button.PointerPressed += (_, _) =>
        {
            if (_isDisposed) return;
            if (button.Parent is Border border)
            {
                border.RenderTransform = TransformOperations.Parse("scale(0.95)");
            }
        };

        button.PointerReleased += (_, _) =>
        {
            if (_isDisposed) return;
            if (button.Parent is Border border)
            {
                border.RenderTransform = TransformOperations.Parse("scale(1.05) translateY(-5px)");
            }
        };

        return button;
    }

    private async void AnimateElementIn(Control element, int delay)
    {
        try
        {
            if (_isDisposed) return;
        
            try
            {
                await Task.Delay(delay, _disposalTokenSource.Token);
                if (_isDisposed) return;
            
                // Add transitions if not already present
                if (element.Transitions == null)
                {
                    element.Transitions = new Transitions();
                }
                
                // Ensure we have the necessary transitions
                var hasOpacityTransition = false;
                var hasTransformTransition = false;
                
                foreach (var transition in element.Transitions)
                {
                    if (transition is DoubleTransition dt && dt.Property == OpacityProperty)
                        hasOpacityTransition = true;
                    if (transition is TransformOperationsTransition tt && tt.Property == RenderTransformProperty)
                        hasTransformTransition = true;
                }
                
                if (!hasOpacityTransition)
                {
                    element.Transitions.Add(new DoubleTransition
                    {
                        Property = OpacityProperty,
                        Duration = TimeSpan.FromMilliseconds(800),
                        Easing = new CubicEaseOut()
                    });
                }
                
                if (!hasTransformTransition)
                {
                    element.Transitions.Add(new TransformOperationsTransition
                    {
                        Property = RenderTransformProperty,
                        Duration = TimeSpan.FromMilliseconds(800),
                        Easing = new CubicEaseOut()
                    });
                }
                
                // Set initial values
                element.Opacity = 0;
                element.RenderTransform = TransformOperations.Parse("translateY(30px)");
                
                // Force layout update
                element.InvalidateVisual();
                
                // Small delay to ensure initial values are applied
                await Task.Delay(50, _disposalTokenSource.Token);
                
                // Animate to final values
                element.Opacity = 1;
                element.RenderTransform = TransformOperations.Parse("translateY(0px)");
            }
            catch (OperationCanceledException)
            {
                // Expected when disposing
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
        }
    }

    private async void OnButtonClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (_isDisposed || sender is not Button button || button.Tag is not ViewType viewType) 
                return;

            // Animate out
            try
            {
                _mainGrid!.Opacity = 0;
                await Task.Delay(300, _disposalTokenSource.Token);
            
                if (!_isDisposed)
                {
                    NavigationRequested?.Invoke(this, viewType);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when disposing
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
        }
    }

    public void Dispose()
    {
        lock (_disposeLock)
        {
            if (_isDisposed) return;
            _isDisposed = true;
        }

        _disposalTokenSource.Cancel();
        _disposalTokenSource.Dispose();

        if (_orderButton != null)
        {
            _orderButton.Click -= OnButtonClicked;
            _orderButton.PointerEntered -= null;
            _orderButton.PointerExited -= null;
            _orderButton.PointerPressed -= null;
            _orderButton.PointerReleased -= null;
        }

        if (_customerButton != null)
        {
            _customerButton.Click -= OnButtonClicked;
            _customerButton.PointerEntered -= null;
            _customerButton.PointerExited -= null;
            _customerButton.PointerPressed -= null;
            _customerButton.PointerReleased -= null;
        }

        _orderButton = null;
        _customerButton = null;
        _orderButtonBorder = null;
        _customerButtonBorder = null;
        _titleText = null;
        _subtitleText = null;

        if (_mainGrid != null)
        {
            _mainGrid.Children.Clear();
            _mainGrid = null;
        }

        (Content as Grid)?.Children.Clear();
        Content = null;
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        Logger.Log("MainView disposed.");
    }
}