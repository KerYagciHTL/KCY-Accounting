using Avalonia.Controls;
using KCY_Accounting.Core;
using KCY_Accounting.Interfaces;

namespace KCY_Accounting.Views;

public class AgbsView : UserControl, IView
{ 
    public event EventHandler<ViewType>? NavigationRequested;
    public string Title => "KCY-Accounting - Allgemeine Geschäftsbedingungen";

    public void Init()
    {
        
    }
}