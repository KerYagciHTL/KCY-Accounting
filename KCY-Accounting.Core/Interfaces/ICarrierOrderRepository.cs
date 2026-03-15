using KCY_Accounting.Core.Models;
namespace KCY_Accounting.Core.Interfaces;
/// <summary>
/// Repository interface for carrier orders (Fraechterauftraege).
/// Follows the same pattern as IInvoiceRepository.
/// </summary>
public interface ICarrierOrderRepository : IRepository<CarrierOrder>
{
    Task<string> GetNextCarrierOrderNumberAsync();
    Task<IEnumerable<CarrierOrder>> GetByCarrierAsync(int carrierId);
    Task<CarrierOrder?> GetWithDetailsAsync(int id);
}
