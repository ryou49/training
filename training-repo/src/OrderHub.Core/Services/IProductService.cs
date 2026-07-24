using OrderHub.Core.Domain;

namespace OrderHub.Core.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<IReadOnlyList<Product>> GetActiveAsync();

    /// <summary>
    /// Active products below stock threshold, with sales in the last 30 days (excludes cancelled).
    /// </summary>
    Task<IReadOnlyList<LowStockProductInfo>> GetLowStockAsync(int threshold);
}
