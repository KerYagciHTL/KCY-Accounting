using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using KCY_Accounting.Core;
using KCY_Accounting.Interfaces;

namespace KCY_Accounting.Views;

public class MainView : UserControl, IView
{
    public string Title => "KCY-Accounting";
    public WindowIcon Icon => new("resources/pictures/icon.ico");
    public event EventHandler<ViewType>? NavigationRequested;

    private Button _orderButton, _customerButton;

    public void Init()
    {
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        _orderButton = new Button
        {
            Content = "Wechsle zu Auftragsansicht",
            Tag = 0,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        _customerButton = new Button
        {
            Content = "Wechsle zu Kundenansicht",
            Tag = 1,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        _orderButton.Click += OnButtonClicked;
        _customerButton.Click += OnButtonClicked;

        mainPanel.Children.Add(_orderButton);
        mainPanel.Children.Add(_customerButton);

        Content = mainPanel;
    }

    private void OnButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not int) return;
        switch (button.Tag)
        {
            case 0:
                NavigationRequested?.Invoke(this, ViewType.Order);
                break;
            case 1:
                NavigationRequested?.Invoke(this, ViewType.Customer);
                break; 
        }
    }
    public void Dispose()
    {
        _orderButton.Click -= OnButtonClicked;
        _customerButton.Click -= OnButtonClicked;
        
        _orderButton = null!;
        _customerButton = null!;
        
        (Content as Panel)?.Children.Clear();
        Content = null;
        
        Logger.Log("MainView disposed.");
    }
}