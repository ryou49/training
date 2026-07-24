using OrderHub.Core.Domain;
using OrderHub.Core.Interfaces;

namespace OrderHub.Core.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync() => _productRepository.GetAllAsync();

    public Task<IReadOnlyList<Product>> GetActiveAsync() => _productRepository.GetActiveAsync();

    public Task<IReadOnlyList<LowStockProductInfo>> GetLowStockAsync(int threshold)
    {
        if (threshold < 1)
            return Task.FromResult<IReadOnlyList<LowStockProductInfo>>(Array.Empty<LowStockProductInfo>());

        var soldSince = DateTime.UtcNow.AddDays(-30);
        return _productRepository.GetLowStockWithSoldQuantityAsync(threshold, soldSince);
    }
}
