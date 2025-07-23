using Avalonia.Controls;
using Avalonia.Layout;
using KCY_Accounting.Core;
using KCY_Accounting.Interfaces;

namespace KCY_Accounting.Views;

public class OrderView : UserControl, IView
{
    public string Title => "KCY-Accounting - Auftragsansicht";
    public WindowIcon Icon => new("resources/pictures/order-management.ico");
    
    public event EventHandler<ViewType>? NavigationRequested;
    public void Init()
    {
        var button = new Button
        {
            Content = "Zurück zur Hauptansicht",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        button.Click += (_, _) => NavigationRequested?.Invoke(this, ViewType.Main);

        Content = button;
    }
}