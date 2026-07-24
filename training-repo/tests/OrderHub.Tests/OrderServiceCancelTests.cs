using OrderHub.Core.Domain;
using OrderHub.Core.Services;

namespace OrderHub.Tests;

public class OrderServiceCancelTests
{
    private static async Task<Order> CreateOrderWithStatusAsync(
        Core.Services.OrderService service,
        Infrastructure.Data.OrderHubDbContext db,
        OrderStatus status)
    {
        var customer = TestSetup.AddCustomer(db);
        var product = TestSetup.AddProduct(db);
        var result = await service.CreateOrderAsync(customer.Id, new[] { new NewOrderLine(product.Id, 1) });
        var order = result.Value!;
        order.Status = status;
        await db.SaveChangesAsync();
        return order;
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Confirmed)]
    public async Task CancelOrder_ActiveOrder_SetsStatusCancelled(OrderStatus initialStatus)
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var order = await CreateOrderWithStatusAsync(service, db, initialStatus);

        var result = await service.CancelOrderAsync(order.Id);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Cancelled, db.Orders.Single(o => o.Id == order.Id).Status);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task CancelOrder_NotCancellableStatus_Fails(OrderStatus initialStatus)
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var order = await CreateOrderWithStatusAsync(service, db, initialStatus);

        var result = await service.CancelOrderAsync(order.Id);

        Assert.False(result.Success);
        Assert.Equal(initialStatus, db.Orders.Single(o => o.Id == order.Id).Status);
    }

    [Fact]
    public async Task CancelOrder_NotFound_Fails()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);

        var result = await service.CancelOrderAsync(12345);

        Assert.False(result.Success);
        Assert.Contains("找不到", result.ErrorMessage);
    }

    /// <summary>
    /// Regression: cancel set Status=Cancelled before stock restore if, so stock never returned.
    /// Create 10 → qty 3 → stock 7; after cancel stock must be 10 again.
    /// </summary>
    [Fact]
    public async Task CancelOrder_Pending_RestoresProductStock()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customer = TestSetup.AddCustomer(db);
        var product = TestSetup.AddProduct(db, stock: 10);

        var created = await service.CreateOrderAsync(customer.Id, new[] { new NewOrderLine(product.Id, 3) });
        Assert.True(created.Success);
        Assert.Equal(7, db.Products.Single(p => p.Id == product.Id).StockQuantity);

        var result = await service.CancelOrderAsync(created.Value!.Id);

        Assert.True(result.Success);
        Assert.Equal(OrderStatus.Cancelled, db.Orders.Single(o => o.Id == created.Value.Id).Status);
        Assert.Equal(10, db.Products.Single(p => p.Id == product.Id).StockQuantity);
    }

    [Fact]
    public async Task CancelOrder_Confirmed_RestoresProductStock()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customer = TestSetup.AddCustomer(db);
        var product = TestSetup.AddProduct(db, stock: 10);

        var created = await service.CreateOrderAsync(customer.Id, new[] { new NewOrderLine(product.Id, 2) });
        Assert.True(created.Success);
        var order = created.Value!;
        order.Status = OrderStatus.Confirmed;
        await db.SaveChangesAsync();
        Assert.Equal(8, db.Products.Single(p => p.Id == product.Id).StockQuantity);

        var result = await service.CancelOrderAsync(order.Id);

        Assert.True(result.Success);
        Assert.Equal(10, db.Products.Single(p => p.Id == product.Id).StockQuantity);
    }

    [Fact]
    public async Task CancelOrder_Shipped_DoesNotChangeStock()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customer = TestSetup.AddCustomer(db);
        var product = TestSetup.AddProduct(db, stock: 10);

        var created = await service.CreateOrderAsync(customer.Id, new[] { new NewOrderLine(product.Id, 2) });
        var order = created.Value!;
        order.Status = OrderStatus.Shipped;
        await db.SaveChangesAsync();
        Assert.Equal(8, db.Products.Single(p => p.Id == product.Id).StockQuantity);

        var result = await service.CancelOrderAsync(order.Id);

        Assert.False(result.Success);
        Assert.Equal(8, db.Products.Single(p => p.Id == product.Id).StockQuantity);
        Assert.Equal(OrderStatus.Shipped, db.Orders.Single(o => o.Id == order.Id).Status);
    }
}
