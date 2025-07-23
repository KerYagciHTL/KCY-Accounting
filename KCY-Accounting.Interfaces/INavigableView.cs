using KCY_Accounting.Core;

namespace KCY_Accounting.Interfaces;

public interface INavigableView
{
    public event EventHandler<ViewType>? NavigationRequested;
}