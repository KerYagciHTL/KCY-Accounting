using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using KCY_Accounting.Core.ViewModels;

namespace KCY_Accounting.UI;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null) return null;
        var name = data.GetType().FullName!.Replace("KCY_Accounting.Core.ViewModels", "KCY_Accounting.UI.Views").Replace("ViewModel", "View");
        var type = Type.GetType(name);
        if (type != null) return (Control)Activator.CreateInstance(type)!;
        return new TextBlock { Text = "Not Found: " + name };
    }
    public bool Match(object? data) => data is ViewModelBase;
}
