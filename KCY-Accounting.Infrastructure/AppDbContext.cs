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
    public DbSet<CarrierOrder> CarrierOrders => Set<CarrierOrder>();
    public DbSet<FreightItem> FreightItems => Set<FreightItem>();

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
            e.Property(o => o.VatRate).HasColumnType("decimal(5,2)");
            e.Property(o => o.Currency).HasMaxLength(10);
            // LTL cargo dimensions in metres – nullable, only populated for Ltl freight type
            e.Property(o => o.LengthM).HasColumnType("decimal(10,3)");
            e.Property(o => o.WidthM).HasColumnType("decimal(10,3)");
            e.Property(o => o.HeightM).HasColumnType("decimal(10,3)");
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

        // ---- CarrierOrder ----
        modelBuilder.Entity<CarrierOrder>(e =>
        {
            e.HasKey(co => co.Id);
            e.Property(co => co.CarrierOrderNumber).IsRequired().HasMaxLength(20);
            e.HasIndex(co => co.CarrierOrderNumber).IsUnique();
            e.Property(co => co.NetAmount).HasColumnType("decimal(18,2)");
            e.Property(co => co.VatRate).HasColumnType("decimal(5,2)");
            e.Property(co => co.Currency).HasMaxLength(10);

            // Embedded loading/unloading stops (same pattern as TransportOrder)
            e.OwnsOne(co => co.LoadingPoint, lp =>
            {
                lp.Property(p => p.CompanyOrPersonName).HasColumnName("CO_Loading_Company").HasMaxLength(200);
                lp.Property(p => p.Street).HasColumnName("CO_Loading_Street").HasMaxLength(200);
                lp.Property(p => p.ZipCode).HasColumnName("CO_Loading_ZipCode").HasMaxLength(20);
                lp.Property(p => p.City).HasColumnName("CO_Loading_City").HasMaxLength(100);
                lp.Property(p => p.Country).HasColumnName("CO_Loading_Country").HasMaxLength(100);
                lp.Property(p => p.DateFrom).HasColumnName("CO_Loading_DateFrom");
                lp.Property(p => p.DateTo).HasColumnName("CO_Loading_DateTo");
                lp.Property(p => p.ContactPerson).HasColumnName("CO_Loading_Contact").HasMaxLength(100);
                lp.Property(p => p.Phone).HasColumnName("CO_Loading_Phone").HasMaxLength(50);
                lp.Property(p => p.Reference).HasColumnName("CO_Loading_Reference").HasMaxLength(100);
            });

            e.OwnsOne(co => co.UnloadingPoint, up =>
            {
                up.Property(p => p.CompanyOrPersonName).HasColumnName("CO_Unloading_Company").HasMaxLength(200);
                up.Property(p => p.Street).HasColumnName("CO_Unloading_Street").HasMaxLength(200);
                up.Property(p => p.ZipCode).HasColumnName("CO_Unloading_ZipCode").HasMaxLength(20);
                up.Property(p => p.City).HasColumnName("CO_Unloading_City").HasMaxLength(100);
                up.Property(p => p.Country).HasColumnName("CO_Unloading_Country").HasMaxLength(100);
                up.Property(p => p.DateFrom).HasColumnName("CO_Unloading_DateFrom");
                up.Property(p => p.DateTo).HasColumnName("CO_Unloading_DateTo");
                up.Property(p => p.ContactPerson).HasColumnName("CO_Unloading_Contact").HasMaxLength(100);
                up.Property(p => p.Phone).HasColumnName("CO_Unloading_Phone").HasMaxLength(50);
                up.Property(p => p.Reference).HasColumnName("CO_Unloading_Reference").HasMaxLength(100);
            });

            e.HasOne(co => co.Carrier)
             .WithMany()
             .HasForeignKey(co => co.CarrierId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(co => co.TransportOrder)
             .WithMany()
             .HasForeignKey(co => co.TransportOrderId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ---- FreightItem ----
        modelBuilder.Entity<FreightItem>(e =>
        {
            e.HasKey(fi => fi.Id);
            e.Property(fi => fi.WeightKgPerUnit).HasColumnType("decimal(18,3)");
            // Dimensions stored in metres (3 decimal places)
            e.Property(fi => fi.LengthM).HasColumnName("LengthM").HasColumnType("decimal(10,3)");
            e.Property(fi => fi.WidthM).HasColumnName("WidthM").HasColumnType("decimal(10,3)");
            e.Property(fi => fi.HeightM).HasColumnName("HeightM").HasColumnType("decimal(10,3)");
            e.Property(fi => fi.Description).HasMaxLength(300);
            e.HasOne(fi => fi.CarrierOrder)
             .WithMany(co => co.FreightItems)
             .HasForeignKey(fi => fi.CarrierOrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

