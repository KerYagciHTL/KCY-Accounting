using KCY_Accounting.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KCY_Accounting.Infrastructure;

/// <summary>
/// EF Core database context. Uses SQLite as backing store.
/// The DB file is created in the user's application data folder.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<TransportOrder> TransportOrders => Set<TransportOrder>();
    public DbSet<OrderDocument> OrderDocuments => Set<OrderDocument>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Customer ----
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CustomerNumber).IsRequired().HasMaxLength(20);
            e.HasIndex(c => c.CustomerNumber).IsUnique();
            e.Property(c => c.CompanyName).IsRequired().HasMaxLength(200);
            e.Property(c => c.Currency).HasMaxLength(10);
        });

        // ---- Carrier ----
        modelBuilder.Entity<Carrier>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.CarrierNumber).IsRequired().HasMaxLength(20);
            e.HasIndex(c => c.CarrierNumber).IsUnique();
            e.Property(c => c.CompanyName).IsRequired().HasMaxLength(200);
        });

        // ---- TransportOrder ----
        modelBuilder.Entity<TransportOrder>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.OrderNumber).IsRequired().HasMaxLength(20);
            e.HasIndex(o => o.OrderNumber).IsUnique();

            // Owned entities: loading and unloading point columns are stored
            // in the same TransportOrders table with a column prefix.
            e.OwnsOne(o => o.LoadingPoint, lp =>
            {
                lp.Property(p => p.CompanyOrPersonName).HasColumnName("Loading_CompanyOrPersonName").HasMaxLength(200);
                lp.Property(p => p.Street).HasColumnName("Loading_Street").HasMaxLength(200);
                lp.Property(p => p.ZipCode).HasColumnName("Loading_ZipCode").HasMaxLength(20);
                lp.Property(p => p.City).HasColumnName("Loading_City").HasMaxLength(100);
                lp.Property(p => p.Country).HasColumnName("Loading_Country").HasMaxLength(100);
                lp.Property(p => p.DateFrom).HasColumnName("Loading_DateFrom");
                lp.Property(p => p.DateTo).HasColumnName("Loading_DateTo");
                lp.Property(p => p.ContactPerson).HasColumnName("Loading_ContactPerson").HasMaxLength(100);
                lp.Property(p => p.Phone).HasColumnName("Loading_Phone").HasMaxLength(50);
                lp.Property(p => p.Reference).HasColumnName("Loading_Reference").HasMaxLength(100);
            });

            e.OwnsOne(o => o.UnloadingPoint, up =>
            {
                up.Property(p => p.CompanyOrPersonName).HasColumnName("Unloading_CompanyOrPersonName").HasMaxLength(200);
                up.Property(p => p.Street).HasColumnName("Unloading_Street").HasMaxLength(200);
                up.Property(p => p.ZipCode).HasColumnName("Unloading_ZipCode").HasMaxLength(20);
                up.Property(p => p.City).HasColumnName("Unloading_City").HasMaxLength(100);
                up.Property(p => p.Country).HasColumnName("Unloading_Country").HasMaxLength(100);
                up.Property(p => p.DateFrom).HasColumnName("Unloading_DateFrom");
                up.Property(p => p.DateTo).HasColumnName("Unloading_DateTo");
                up.Property(p => p.ContactPerson).HasColumnName("Unloading_ContactPerson").HasMaxLength(100);
                up.Property(p => p.Phone).HasColumnName("Unloading_Phone").HasMaxLength(50);
                up.Property(p => p.Reference).HasColumnName("Unloading_Reference").HasMaxLength(100);
            });

            e.HasOne(o => o.Customer)
             .WithMany(c => c.TransportOrders)
             .HasForeignKey(o => o.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(o => o.Carrier)
             .WithMany(c => c.TransportOrders)
             .HasForeignKey(o => o.CarrierId)
             .OnDelete(DeleteBehavior.SetNull);

            e.Property(o => o.SalePrice).HasColumnType("decimal(18,2)");
            e.Property(o => o.PurchasePrice).HasColumnType("decimal(18,2)");
            e.Property(o => o.Currency).HasMaxLength(10);
        });

        // ---- OrderDocument ----
        modelBuilder.Entity<OrderDocument>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.TransportOrder)
             .WithMany(o => o.Documents)
             .HasForeignKey(d => d.TransportOrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Invoice ----
        modelBuilder.Entity<Invoice>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(20);
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
            e.Property(i => i.Amount).HasColumnType("decimal(18,2)");
            e.Property(i => i.VatRate).HasColumnType("decimal(5,2)");
            e.HasOne(i => i.TransportOrder)
             .WithMany(o => o.Invoices)
             .HasForeignKey(i => i.TransportOrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

