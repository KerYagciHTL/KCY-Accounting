using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KCY_Accounting.Infrastructure.Repositories;

public class TransportOrderRepository : ITransportOrderRepository
{
    private readonly AppDbContext _db;

    public TransportOrderRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<TransportOrder>> GetAllAsync() =>
        await _db.TransportOrders
            .Include(o => o.Customer)
            .Include(o => o.Carrier)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

    public async Task<TransportOrder?> GetByIdAsync(int id) =>
        await _db.TransportOrders
            .Include(o => o.Customer)
            .Include(o => o.Carrier)
            .FirstOrDefaultAsync(o => o.Id == id);

    /// <summary>Returns an order with all its related data (documents, invoices).</summary>
    public async Task<TransportOrder?> GetWithDetailsAsync(int id) =>
        await _db.TransportOrders
            .Include(o => o.Customer)
            .Include(o => o.Carrier)
            .Include(o => o.Documents)
            .Include(o => o.Invoices)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task AddAsync(TransportOrder entity)
    {
        _db.TransportOrders.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(TransportOrder entity)
    {
        _db.TransportOrders.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.TransportOrders.FindAsync(id);
        if (entity != null)
        {
            _db.TransportOrders.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> GetNextOrderNumberAsync()
    {
        // Format: TA-YYYYNNNNN – year-prefixed sequential number
        int year = DateTime.Today.Year;
        int count = await _db.TransportOrders.CountAsync(o => o.OrderDate.Year == year);
        return $"TA-{year}{(count + 1):D4}";
    }

    public async Task<IEnumerable<TransportOrder>> SearchAsync(OrderFilter filter)
    {
        IQueryable<TransportOrder> q = _db.TransportOrders
            .Include(o => o.Customer)
            .Include(o => o.Carrier);

        if (filter.CustomerId.HasValue)
            q = q.Where(o => o.CustomerId == filter.CustomerId.Value);

        if (filter.CarrierId.HasValue)
            q = q.Where(o => o.CarrierId == filter.CarrierId.Value);

        if (filter.Status.HasValue)
            q = q.Where(o => o.Status == filter.Status.Value);

        if (filter.DateFrom.HasValue)
            q = q.Where(o => o.OrderDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            q = q.Where(o => o.OrderDate <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var txt = filter.SearchText.ToLower();
            q = q.Where(o => o.OrderNumber.ToLower().Contains(txt)
                           || o.Customer.CompanyName.ToLower().Contains(txt)
                           || (o.Carrier != null && o.Carrier.CompanyName.ToLower().Contains(txt)));
        }

        return await q.OrderByDescending(o => o.OrderDate).ToListAsync();
    }
}

