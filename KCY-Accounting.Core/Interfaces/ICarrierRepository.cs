using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.Interfaces;

public interface ICarrierRepository : IRepository<Carrier>
{
    Task<string> GetNextCarrierNumberAsync();
    Task<IEnumerable<Carrier>> SearchAsync(string searchText);
}

