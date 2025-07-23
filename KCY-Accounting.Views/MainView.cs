using Avalonia.Controls;
using Avalonia.Layout;
using KCY_Accounting.Core;
using KCY_Accounting.Interfaces;

namespace KCY_Accounting.Views;

public class MainView : UserControl, IView
{
    public string Title => "KCY-Accounting";
    public WindowIcon Icon => new("resources/pictures/icon.ico");
    
    public event EventHandler<ViewType>? NavigationRequested;

    public void Init()
    {
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var orderButton = new Button
        {
            Content = "Wechsle zu Auftragsansicht",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };

        var customerButton = new Button
        {
            Content = "Wechsle zu Kundenansicht",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        orderButton.Click += (_, _) => NavigationRequested?.Invoke(this, ViewType.Order);
        customerButton.Click += (_, _) => NavigationRequested?.Invoke(this, ViewType.Customer);

        mainPanel.Children.Add(orderButton);
        mainPanel.Children.Add(customerButton);

        Content = mainPanel;
    }
}