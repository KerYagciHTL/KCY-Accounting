using System;
using Avalonia.Controls;
using KCY_Accounting.Core.ViewModels;

namespace KCY_Accounting.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        // The visual tree is now fully attached – safe to trigger the first
        // async data load so Avalonia's DataGrid receives ItemsSource updates
        // while it is already rendered.
        if (DataContext is MainViewModel vm)
            vm.NavigateToDashboard();
    }
}