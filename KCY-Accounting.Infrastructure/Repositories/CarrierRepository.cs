using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KCY_Accounting.Infrastructure.Repositories;

public class CarrierRepository : ICarrierRepository
{
    private readonly AppDbContext _db;

    public CarrierRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Carrier>> GetAllAsync() =>
        await _db.Carriers.OrderBy(c => c.CarrierNumber).ToListAsync();

    public async Task<Carrier?> GetByIdAsync(int id) =>
        await _db.Carriers.FindAsync(id);

    public async Task AddAsync(Carrier entity)
    {
        _db.Carriers.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Carrier entity)
    {
        _db.Carriers.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Carriers.FindAsync(id);
        if (entity != null)
        {
            _db.Carriers.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> GetNextCarrierNumberAsync()
    {
        var last = await _db.Carriers
            .OrderByDescending(c => c.Id)
            .Select(c => c.CarrierNumber)
            .FirstOrDefaultAsync();

        int next = 1;
        if (last != null && last.StartsWith("FR-") && int.TryParse(last[3..], out int n))
            next = n + 1;

        return $"FR-{next:D5}";
    }

    public async Task<IEnumerable<Carrier>> SearchAsync(string searchText)
    {
        var q = searchText.ToLower();
        return await _db.Carriers
            .Where(c => c.CompanyName.ToLower().Contains(q)
                     || c.CarrierNumber.ToLower().Contains(q)
                     || c.ContactPerson.ToLower().Contains(q))
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }
}

