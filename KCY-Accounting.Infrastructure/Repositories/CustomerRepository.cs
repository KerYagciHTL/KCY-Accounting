using KCY_Accounting.Core.Interfaces;
using KCY_Accounting.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KCY_Accounting.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Customer>> GetAllAsync() =>
        await _db.Customers.OrderBy(c => c.CustomerNumber).ToListAsync();

    public async Task<Customer?> GetByIdAsync(int id) =>
        await _db.Customers.FindAsync(id);

    public async Task AddAsync(Customer entity)
    {
        _db.Customers.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer entity)
    {
        _db.Customers.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Customers.FindAsync(id);
        if (entity != null)
        {
            _db.Customers.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> GetNextCustomerNumberAsync()
    {
        // Determine next sequential number based on highest existing customer number.
        var last = await _db.Customers
            .OrderByDescending(c => c.Id)
            .Select(c => c.CustomerNumber)
            .FirstOrDefaultAsync();

        int next = 1;
        if (last != null && last.StartsWith("KD-") && int.TryParse(last[3..], out int n))
            next = n + 1;

        return $"KD-{next:D5}";
    }

    public async Task<IEnumerable<Customer>> SearchAsync(string searchText)
    {
        var q = searchText.ToLower();
        return await _db.Customers
            .Where(c => c.CompanyName.ToLower().Contains(q)
                     || c.CustomerNumber.ToLower().Contains(q)
                     || c.ContactPerson.ToLower().Contains(q))
            .OrderBy(c => c.CompanyName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> GetActiveAsync() =>
        await _db.Customers.Where(c => c.IsActive).OrderBy(c => c.CompanyName).ToListAsync();
}

