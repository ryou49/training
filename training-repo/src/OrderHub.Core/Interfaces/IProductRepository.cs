using OrderHub.Core.Domain;
using OrderHub.Core.Services;

namespace OrderHub.Core.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<IReadOnlyList<Product>> GetActiveAsync();
    Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatusFilter filter);
    Task<Product?> GetByIdAsync(int id);

    Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null);
    Task AddAsync(Product product);

    /// <summary>
    /// Active products with StockQuantity &lt; threshold, ordered by stock ascending,
    /// including sold quantity since soldSince (non-cancelled orders only).
    /// </summary>
    Task<IReadOnlyList<LowStockProductInfo>> GetLowStockWithSoldQuantityAsync(
        int threshold,
        DateTime soldSince);

    Task SaveChangesAsync();
}
