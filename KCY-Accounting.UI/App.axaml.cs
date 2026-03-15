using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.ViewModels;
using KCY_Accounting.Infrastructure;
using KCY_Accounting.Infrastructure.Repositories;
using KCY_Accounting.Infrastructure.Services;
using KCY_Accounting.UI.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace KCY_Accounting.UI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Initialize DB schema on first launch
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DatabaseInitializer.EnsureCreatedAsync(db).GetAwaiter().GetResult();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Resolve MainViewModel within a long-lived scope that lives for the
            // entire application lifetime so all repositories share one DbContext.
            var appScope = Services.CreateScope();
            desktop.MainWindow = new MainWindow
            {
                DataContext = appScope.ServiceProvider.GetRequiredService<MainViewModel>()
            };

            // Dispose the scope when the application exits
            desktop.Exit += (_, _) => appScope.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var dbPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "KCY-Accounting",
            "kcy_accounting.db");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dbPath)!);

        services.AddDbContext<AppDbContext>(
            // Explicit UTF-8 encoding ensures umlauts (ä, ö, ü) and special
            // characters are stored and retrieved correctly from SQLite.
            opt => opt.UseSqlite($"Data Source={dbPath};Cache=Shared;"),
            contextLifetime: ServiceLifetime.Scoped);

        // Repositories as Scoped – share the DbContext within one scope
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICarrierRepository, CarrierRepository>();
        services.AddScoped<ITransportOrderRepository, TransportOrderRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ICarrierOrderRepository, CarrierOrderRepository>();

        // PDF generation – stateless, safe as singleton
        services.AddSingleton<IPdfService, PdfService>();

        // MainViewModel as Scoped so it receives the same repository instances
        services.AddScoped<MainViewModel>();
    }
}
