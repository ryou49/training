using OrderHub.Core.Domain;

namespace OrderHub.Tests;

public class OrderServiceQueryTests
{
    [Fact]
    public async Task GetOrders_WithStatusFilter_ReturnsOnlyMatchingStatus()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customer = TestSetup.AddCustomer(db);

        db.Orders.AddRange(
            new Order { CustomerId = customer.Id, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow },
            new Order { CustomerId = customer.Id, Status = OrderStatus.Shipped, CreatedAt = DateTime.UtcNow },
            new Order { CustomerId = customer.Id, Status = OrderStatus.Shipped, CreatedAt = DateTime.UtcNow });
        db.SaveChanges();

        var result = await service.GetOrdersAsync(1, 20, OrderStatus.Shipped);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count); // empty Items previously passed Assert.All
        Assert.All(result.Items, o => Assert.Equal(OrderStatus.Shipped, o.Status));
    }

    [Fact]
    public async Task GetOrders_ReportsTotalCountAndTotalPages()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customer = TestSetup.AddCustomer(db);

        for (var i = 0; i < 45; i++)
            db.Orders.Add(new Order { CustomerId = customer.Id, Status = OrderStatus.Confirmed, CreatedAt = DateTime.UtcNow.AddMinutes(-i) });
        db.SaveChanges();

        var result = await service.GetOrdersAsync(1, 20, null);

        Assert.Equal(45, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(20, result.Items.Count);
    }

    /// <summary>
    /// Regression: UI page is 1-based; Skip(page * pageSize) dropped the first page
    /// (newest orders like #201 missing on /Orders page 1).
    /// </summary>
    [Fact]
    public async Task GetOrders_Page1_IncludesNewestOrder()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customer = TestSetup.AddCustomer(db);

        for (var i = 0; i < 25; i++)
        {
            db.Orders.Add(new Order
            {
                CustomerId = customer.Id,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        db.SaveChanges();

        var newest = db.Orders.OrderByDescending(o => o.CreatedAt).First();
        var page1 = await service.GetOrdersAsync(1, 20, null);

        Assert.Equal(25, page1.TotalCount);
        Assert.Equal(20, page1.Items.Count);
        Assert.Contains(page1.Items, o => o.Id == newest.Id);
        Assert.Equal(newest.Id, page1.Items[0].Id);
    }

    /// <summary>
    /// Regression: last page used Skip(page * pageSize) and was often empty.
    /// </summary>
    [Fact]
    public async Task GetOrders_LastPage_IsNotEmpty_WhenOrdersExist()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customer = TestSetup.AddCustomer(db);

        for (var i = 0; i < 45; i++)
        {
            db.Orders.Add(new Order
            {
                CustomerId = customer.Id,
                Status = OrderStatus.Confirmed,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        db.SaveChanges();

        const int pageSize = 20;
        var lastPageNumber = 3; // ceil(45/20) = 3, remainder 5
        var lastPage = await service.GetOrdersAsync(lastPageNumber, pageSize, null);

        Assert.Equal(45, lastPage.TotalCount);
        Assert.Equal(3, lastPage.TotalPages);
        Assert.NotEmpty(lastPage.Items);
        Assert.Equal(5, lastPage.Items.Count);
    }

    /// <summary>
    /// Regression: status filter "已取消" showed a blank list because page 1 skipped pageSize rows.
    /// </summary>
    [Fact]
    public async Task GetOrders_CancelledFilter_Page1_ReturnsCancelledOrders()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customer = TestSetup.AddCustomer(db);

        db.Orders.AddRange(
            new Order { CustomerId = customer.Id, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new Order { CustomerId = customer.Id, Status = OrderStatus.Cancelled, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new Order { CustomerId = customer.Id, Status = OrderStatus.Cancelled, CreatedAt = DateTime.UtcNow.AddMinutes(-3) },
            new Order { CustomerId = customer.Id, Status = OrderStatus.Shipped, CreatedAt = DateTime.UtcNow.AddMinutes(-4) });
        db.SaveChanges();

        var result = await service.GetOrdersAsync(1, 20, OrderStatus.Cancelled);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, o => Assert.Equal(OrderStatus.Cancelled, o.Status));
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetCustomerOrders_ReturnsOnlyThatCustomersOrders()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var customerA = TestSetup.AddCustomer(db, name: "客戶A");
        var customerB = TestSetup.AddCustomer(db, name: "客戶B");

        db.Orders.AddRange(
            new Order { CustomerId = customerA.Id, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow },
            new Order { CustomerId = customerB.Id, Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow },
            new Order { CustomerId = customerA.Id, Status = OrderStatus.Shipped, CreatedAt = DateTime.UtcNow });
        db.SaveChanges();

        var orders = await service.GetCustomerOrdersAsync(customerA.Id);

        Assert.Equal(2, orders.Count);
        Assert.All(orders, o => Assert.Equal(customerA.Id, o.CustomerId));
    }
}
