using OrderHub.Core.Common;
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

    public Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatusFilter filter) =>
        _productRepository.GetByStatusAsync(filter);

    public Task<IReadOnlyList<LowStockProductInfo>> GetLowStockAsync(int threshold)
    {
        if (threshold < 1)
            return Task.FromResult<IReadOnlyList<LowStockProductInfo>>(Array.Empty<LowStockProductInfo>());

        var soldSince = DateTime.UtcNow.AddDays(-30);
        return _productRepository.GetLowStockWithSoldQuantityAsync(threshold, soldSince);
    }

    public async Task<ServiceResult<Product>> CreateAsync(
        string sku,
        string name,
        decimal unitPrice,
        int stockQuantity,
        bool isActive)
    {
        sku = (sku ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();

        var errors = ValidateProductFields(sku, name, unitPrice, stockQuantity, requirePrice: true);
        if (errors.Count > 0)
            return ServiceResult<Product>.Fail(errors);

        if (await _productRepository.SkuExistsAsync(sku))
            return ServiceResult<Product>.Fail("SKU 已存在");

        var product = new Product
        {
            Sku = sku,
            Name = name,
            UnitPrice = unitPrice,
            StockQuantity = stockQuantity,
            IsActive = isActive
        };

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return ServiceResult<Product>.Ok(product);
    }

    public async Task<ServiceResult<Product>> UpdateAsync(
        int id,
        string sku,
        string name,
        int stockQuantity,
        bool isActive)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            return ServiceResult<Product>.Fail("找不到指定的商品");

        sku = (sku ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();

        // Price is not updated on list; pass current price so shared validators skip price checks when requirePrice is false.
        var errors = ValidateProductFields(sku, name, product.UnitPrice, stockQuantity, requirePrice: false);
        if (errors.Count > 0)
            return ServiceResult<Product>.Fail(errors);

        if (await _productRepository.SkuExistsAsync(sku, excludeProductId: id))
            return ServiceResult<Product>.Fail("SKU 已存在");

        product.Sku = sku;
        product.Name = name;
        product.StockQuantity = stockQuantity;
        product.IsActive = isActive;

        await _productRepository.SaveChangesAsync();

        return ServiceResult<Product>.Ok(product);
    }

    private static List<string> ValidateProductFields(
        string sku,
        string name,
        decimal unitPrice,
        int stockQuantity,
        bool requirePrice)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sku))
            errors.Add("SKU 為必填");
        else if (sku.Length > 20)
            errors.Add("SKU 長度不可超過 20");

        if (string.IsNullOrWhiteSpace(name))
            errors.Add("名稱為必填");
        else if (name.Length > 100)
            errors.Add("名稱長度不可超過 100");

        if (requirePrice && unitPrice <= 0)
            errors.Add("單價必須大於 0");

        if (stockQuantity < 0)
            errors.Add("庫存不可為負數");

        return errors;
    }
}
