using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<string> GetNextCustomerNumberAsync();
    Task<IEnumerable<Customer>> SearchAsync(string searchText);
    Task<IEnumerable<Customer>> GetActiveAsync();
}

