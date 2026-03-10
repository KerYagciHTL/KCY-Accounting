using System;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using KCY_Accounting.Core.ViewModels;

namespace KCY_Accounting.UI;

public class ViewLocator : IDataTemplate
{
    // Cache the UI assembly so we don't look it up on every Build call
    private static readonly Assembly UiAssembly =
        typeof(ViewLocator).Assembly;

    public Control? Build(object? data)
    {
        if (data is null) return null;

        // Derive the expected View type name from the ViewModel type name.
        // e.g. KCY_Accounting.Core.ViewModels.CustomerListViewModel
        //   -> KCY_Accounting.UI.Views.CustomerListView
        var viewName = data.GetType().FullName!
            .Replace("KCY_Accounting.Core.ViewModels", "KCY_Accounting.UI.Views")
            .Replace("ViewModel", "View");

        // Look up the type directly in the UI assembly (avoids cross-assembly issues)
        var type = UiAssembly.GetType(viewName);

        if (type != null)
            return (Control)Activator.CreateInstance(type)!;

        return new TextBlock
        {
            Text = $"View nicht gefunden: {viewName}",
            Margin = new Avalonia.Thickness(20)
        };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
