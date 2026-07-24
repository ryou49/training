using Microsoft.EntityFrameworkCore;
using OrderHub.Core.Domain;
using OrderHub.Core.Interfaces;
using OrderHub.Core.Services;
using OrderHub.Infrastructure.Data;

namespace OrderHub.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OrderHubDbContext _db;

    public ProductRepository(OrderHubDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync() =>
        await _db.Products.OrderBy(p => p.Sku).ToListAsync();

    public async Task<IReadOnlyList<Product>> GetActiveAsync() =>
        await _db.Products.Where(p => p.IsActive).OrderBy(p => p.Sku).ToListAsync();

    public async Task<IReadOnlyList<Product>> GetByStatusAsync(ProductStatusFilter filter)
    {
        var query = _db.Products.AsQueryable();

        query = filter switch
        {
            ProductStatusFilter.Active => query.Where(p => p.IsActive),
            ProductStatusFilter.Inactive => query.Where(p => !p.IsActive),
            _ => query
        };

        return await query.OrderBy(p => p.Sku).ToListAsync();
    }

    public Task<Product?> GetByIdAsync(int id) =>
        _db.Products.FirstOrDefaultAsync(p => p.Id == id);

    public Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null)
    {
        var query = _db.Products.Where(p => p.Sku == sku);
        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);
        return query.AnyAsync();
    }

    public async Task AddAsync(Product product) => await _db.Products.AddAsync(product);

    public async Task<IReadOnlyList<LowStockProductInfo>> GetLowStockWithSoldQuantityAsync(
        int threshold,
        DateTime soldSince)
    {
        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.IsActive && p.StockQuantity < threshold)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.Sku)
            .ToListAsync();

        if (products.Count == 0)
            return Array.Empty<LowStockProductInfo>();

        var productIds = products.Select(p => p.Id).ToList();

        var soldByProduct = await (
                from oi in _db.OrderItems.AsNoTracking()
                join o in _db.Orders.AsNoTracking() on oi.OrderId equals o.Id
                where productIds.Contains(oi.ProductId)
                      && o.CreatedAt >= soldSince
                      && o.Status != OrderStatus.Cancelled
                group oi by oi.ProductId into g
                select new { ProductId = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToListAsync();

        var soldMap = soldByProduct.ToDictionary(x => x.ProductId, x => x.Qty);

        return products
            .Select(p => new LowStockProductInfo
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                StockQuantity = p.StockQuantity,
                SoldLast30Days = soldMap.GetValueOrDefault(p.Id)
            })
            .ToList();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
