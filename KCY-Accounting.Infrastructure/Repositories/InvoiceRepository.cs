using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KCY_Accounting.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _db;

    public InvoiceRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Invoice>> GetAllAsync() =>
        await _db.Invoices.Include(i => i.TransportOrder).OrderByDescending(i => i.IssuedAt).ToListAsync();

    public async Task<Invoice?> GetByIdAsync(int id) =>
        await _db.Invoices.FindAsync(id);

    public async Task<IEnumerable<Invoice>> GetByOrderIdAsync(int transportOrderId) =>
        await _db.Invoices
            .Where(i => i.TransportOrderId == transportOrderId)
            .OrderBy(i => i.Type)
            .ToListAsync();

    public async Task AddAsync(Invoice entity)
    {
        _db.Invoices.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice entity)
    {
        _db.Invoices.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Invoices.FindAsync(id);
        if (entity != null)
        {
            _db.Invoices.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> GetNextInvoiceNumberAsync()
    {
        int year = DateTime.Today.Year;
        int count = await _db.Invoices.CountAsync(i => i.IssuedAt.Year == year);
        return $"RE-{year}{(count + 1):D4}";
    }

    /// <summary>
    /// Profit = SalePrice of the transport order minus its PurchasePrice.
    /// Does not depend on Invoice records – reads directly from TransportOrder.
    /// </summary>
    public async Task<decimal> GetProfitAsync(int transportOrderId)
    {
        var order = await _db.TransportOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == transportOrderId);

        return order == null ? 0m : order.SalePrice - order.PurchasePrice;
    }
}

