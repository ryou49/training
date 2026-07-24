using OrderHub.Core.Domain;

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
}
