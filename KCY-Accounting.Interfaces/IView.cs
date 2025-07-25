using Avalonia.Controls;
using KCY_Accounting.Core;

namespace KCY_Accounting.Interfaces;
public interface IView : INavigableView, IDisposable
{
    public string Title { get; }
    public WindowIcon Icon => new("resources/pictures/license.ico");
    void Init();
}