using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KCY_Accounting.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ICarrierOrderRepository.
/// Eagerly loads FreightItems, Carrier and TransportOrder on detail queries.
/// </summary>
public class CarrierOrderRepository : ICarrierOrderRepository
{
    private readonly AppDbContext _db;
    public CarrierOrderRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<CarrierOrder>> GetAllAsync() =>
        await _db.CarrierOrders
            .Include(co => co.Carrier)
            .Include(co => co.TransportOrder)
            .Include(co => co.FreightItems)
            .OrderByDescending(co => co.IssuedAt)
            .ToListAsync();

    public async Task<CarrierOrder?> GetByIdAsync(int id) =>
        await _db.CarrierOrders.FindAsync(id);

    public async Task<CarrierOrder?> GetWithDetailsAsync(int id) =>
        await _db.CarrierOrders
            .Include(co => co.Carrier)
            .Include(co => co.TransportOrder)
            .Include(co => co.FreightItems)
            .FirstOrDefaultAsync(co => co.Id == id);

    public async Task<IEnumerable<CarrierOrder>> GetByCarrierAsync(int carrierId) =>
        await _db.CarrierOrders
            .Where(co => co.CarrierId == carrierId)
            .Include(co => co.FreightItems)
            .OrderByDescending(co => co.IssuedAt)
            .ToListAsync();

    public async Task AddAsync(CarrierOrder entity)
    {
        _db.CarrierOrders.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(CarrierOrder entity)
    {
        // Remove old freight items first, then update the order.
        // This avoids duplicate-key conflicts on the child collection.
        var existingItems = await _db.FreightItems
            .Where(fi => fi.CarrierOrderId == entity.Id)
            .ToListAsync();
        _db.FreightItems.RemoveRange(existingItems);

        _db.CarrierOrders.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.CarrierOrders.FindAsync(id);
        if (entity != null)
        {
            _db.CarrierOrders.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> GetNextCarrierOrderNumberAsync()
    {
        int year  = DateTime.Today.Year;
        int count = await _db.CarrierOrders.CountAsync(co => co.IssuedAt.Year == year);
        return $"FA-{year}{(count + 1):D4}";
    }
}

