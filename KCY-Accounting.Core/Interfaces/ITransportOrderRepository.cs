using KCY_Accounting.Core.Models;

namespace KCY_Accounting.Core.Interfaces;

/// <summary>Filter parameters for transport order search.</summary>
public class OrderFilter
{
    public int? CustomerId { get; set; }
    public int? CarrierId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SearchText { get; set; }
}

public interface ITransportOrderRepository : IRepository<TransportOrder>
{
    Task<string> GetNextOrderNumberAsync();
    Task<IEnumerable<TransportOrder>> SearchAsync(OrderFilter filter);
    Task<TransportOrder?> GetWithDetailsAsync(int id);
}

