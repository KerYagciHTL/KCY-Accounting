using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<string> GetNextInvoiceNumberAsync();
    Task<IEnumerable<Invoice>> GetByOrderIdAsync(int transportOrderId);

    /// <summary>Returns profit (SalePrice - PurchasePrice) for a given order.</summary>
    Task<decimal> GetProfitAsync(int transportOrderId);
}

