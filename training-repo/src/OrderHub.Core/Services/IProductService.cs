using OrderHub.Core.Common;
using OrderHub.Core.Domain;

namespace OrderHub.Core.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<IReadOnlyList<Product>> GetActiveAsync();
    Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatusFilter filter);

    /// <summary>
    /// Active products below stock threshold, with sales in the last 30 days (excludes cancelled).
    /// </summary>
    Task<IReadOnlyList<LowStockProductInfo>> GetLowStockAsync(int threshold);

    Task<ServiceResult<Product>> CreateAsync(
        string sku,
        string name,
        decimal unitPrice,
        int stockQuantity,
        bool isActive);

    Task<ServiceResult<Product>> UpdateAsync(
        int id,
        string sku,
        string name,
        int stockQuantity,
        bool isActive);
}
