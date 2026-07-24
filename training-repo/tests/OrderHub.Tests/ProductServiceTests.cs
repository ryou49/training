using OrderHub.Core.Domain;
using OrderHub.Core.Services;

namespace OrderHub.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAll_ReturnsAllProductsIncludingInactive()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-A001");
        TestSetup.AddProduct(db, sku: "SKU-A002", isActive: false);

        var products = await service.GetAllAsync();

        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task GetActive_ExcludesInactiveProducts()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-A001");
        TestSetup.AddProduct(db, sku: "SKU-A002", isActive: false);

        var products = await service.GetActiveAsync();

        Assert.All(products, p => Assert.True(p.IsActive));
        Assert.Single(products);
    }

    [Fact]
    public async Task GetLowStock_FiltersByThreshold_AndSortsByStockAscending()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, stock: 15, sku: "SKU-HI");
        TestSetup.AddProduct(db, stock: 8, sku: "SKU-MID");
        TestSetup.AddProduct(db, stock: 3, sku: "SKU-LO");
        TestSetup.AddProduct(db, stock: 10, sku: "SKU-EQ"); // equal to threshold — excluded (< only)

        var result = await service.GetLowStockAsync(threshold: 10);

        Assert.Equal(2, result.Count);
        Assert.Equal(new[] { "SKU-LO", "SKU-MID" }, result.Select(p => p.Sku).ToArray());
        Assert.Equal(3, result[0].StockQuantity);
        Assert.Equal(8, result[1].StockQuantity);
    }

    [Fact]
    public async Task GetLowStock_ExcludesInactiveProducts()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, stock: 1, sku: "SKU-ACT", isActive: true);
        TestSetup.AddProduct(db, stock: 1, sku: "SKU-OFF", isActive: false);

        var result = await service.GetLowStockAsync(threshold: 10);

        Assert.Single(result);
        Assert.Equal("SKU-ACT", result[0].Sku);
    }

    [Fact]
    public async Task GetLowStock_SoldLast30Days_ExcludesCancelledAndOldOrders()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        var customer = TestSetup.AddCustomer(db);
        var product = TestSetup.AddProduct(db, stock: 2, sku: "SKU-SALE", unitPrice: 50m);

        // Recent non-cancelled: qty 5
        TestSetup.AddOrderWithItem(db, customer.Id, product.Id, quantity: 5,
            status: OrderStatus.Confirmed, createdAt: DateTime.UtcNow.AddDays(-5));
        // Recent cancelled: qty 4 (must not count)
        TestSetup.AddOrderWithItem(db, customer.Id, product.Id, quantity: 4,
            status: OrderStatus.Cancelled, createdAt: DateTime.UtcNow.AddDays(-2));
        // Old non-cancelled: qty 10 (outside 30 days)
        TestSetup.AddOrderWithItem(db, customer.Id, product.Id, quantity: 10,
            status: OrderStatus.Shipped, createdAt: DateTime.UtcNow.AddDays(-40));

        var result = await service.GetLowStockAsync(threshold: 10);

        var row = Assert.Single(result);
        Assert.Equal("SKU-SALE", row.Sku);
        Assert.Equal(5, row.SoldLast30Days);
    }

    [Fact]
    public async Task GetByStatus_All_ReturnsActiveAndInactive()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-ON", isActive: true);
        TestSetup.AddProduct(db, sku: "SKU-OFF", isActive: false);

        var products = await service.GetByStatusAsync(ProductStatusFilter.All);

        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task GetByStatus_Active_OnlyActive()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-ON", isActive: true);
        TestSetup.AddProduct(db, sku: "SKU-OFF", isActive: false);

        var products = await service.GetByStatusAsync(ProductStatusFilter.Active);

        Assert.Single(products);
        Assert.True(products[0].IsActive);
    }

    [Fact]
    public async Task GetByStatus_Inactive_OnlyInactive()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-ON", isActive: true);
        TestSetup.AddProduct(db, sku: "SKU-OFF", isActive: false);

        var products = await service.GetByStatusAsync(ProductStatusFilter.Inactive);

        Assert.Single(products);
        Assert.False(products[0].IsActive);
    }

    [Fact]
    public async Task CreateProduct_HappyPath_Persists()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);

        var result = await service.CreateAsync("SKU-NEW", "新商品", 99.5m, 12, isActive: true);

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal("SKU-NEW", result.Value!.Sku);
        Assert.Equal("新商品", result.Value.Name);
        Assert.Equal(99.5m, result.Value.UnitPrice);
        Assert.Equal(12, result.Value.StockQuantity);
        Assert.True(result.Value.IsActive);
        Assert.Single(db.Products);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_Fails()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        TestSetup.AddProduct(db, sku: "SKU-DUP");

        var result = await service.CreateAsync("SKU-DUP", "另一個", 10m, 1, true);

        Assert.False(result.Success);
        Assert.Contains("SKU", result.ErrorMessage);
        Assert.Single(db.Products);
    }

    [Fact]
    public async Task CreateProduct_InvalidPriceOrStock_Fails()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);

        var badPrice = await service.CreateAsync("SKU-P", "P", 0m, 1, true);
        var badStock = await service.CreateAsync("SKU-S", "S", 10m, -1, true);

        Assert.False(badPrice.Success);
        Assert.False(badStock.Success);
        Assert.Empty(db.Products);
    }

    [Fact]
    public async Task UpdateProduct_ChangesSkuNameStockAndStatus()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        var product = TestSetup.AddProduct(db, sku: "SKU-OLD", stock: 10, isActive: true, unitPrice: 50m);

        var result = await service.UpdateAsync(product.Id, "SKU-NEW", "新名稱", 3, isActive: false);

        Assert.True(result.Success);
        var updated = db.Products.Single(p => p.Id == product.Id);
        Assert.Equal("SKU-NEW", updated.Sku);
        Assert.Equal("新名稱", updated.Name);
        Assert.Equal(3, updated.StockQuantity);
        Assert.False(updated.IsActive);
        Assert.Equal(50m, updated.UnitPrice); // price not changed by update
    }

    [Fact]
    public async Task UpdateProduct_DuplicateSku_Fails()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        var a = TestSetup.AddProduct(db, sku: "SKU-A");
        var b = TestSetup.AddProduct(db, sku: "SKU-B");

        var result = await service.UpdateAsync(b.Id, "SKU-A", "B", 1, true);

        Assert.False(result.Success);
        Assert.Equal("SKU-B", db.Products.Single(p => p.Id == b.Id).Sku);
    }

    [Fact]
    public async Task UpdateProduct_SameSku_Succeeds()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        var product = TestSetup.AddProduct(db, sku: "SKU-SAME", stock: 5);

        var result = await service.UpdateAsync(product.Id, "SKU-SAME", "改名", 8, true);

        Assert.True(result.Success);
        Assert.Equal(8, db.Products.Single().StockQuantity);
        Assert.Equal("改名", db.Products.Single().Name);
    }

    [Fact]
    public async Task UpdateProduct_NegativeStock_Fails()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);
        var product = TestSetup.AddProduct(db, stock: 5);

        var result = await service.UpdateAsync(product.Id, product.Sku, product.Name, -1, true);

        Assert.False(result.Success);
        Assert.Equal(5, db.Products.Single().StockQuantity);
    }

    [Fact]
    public async Task UpdateProduct_NotFound_Fails()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateProductService(db);

        var result = await service.UpdateAsync(999, "SKU-X", "X", 1, true);

        Assert.False(result.Success);
        Assert.Contains("找不到", result.ErrorMessage);
    }
}
