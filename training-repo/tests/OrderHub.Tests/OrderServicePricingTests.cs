using OrderHub.Core.Domain;
using OrderHub.Core.Services;

namespace OrderHub.Tests;

public class OrderServicePricingTests
{
    [Theory]
    [InlineData(CustomerTier.Standard, 0)]
    [InlineData(CustomerTier.Silver, 0.05)]
    [InlineData(CustomerTier.Gold, 0.10)]
    public void GetDiscountRate_ReturnsExpectedRate(CustomerTier tier, decimal expected)
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);

        Assert.Equal(expected, service.GetDiscountRate(tier));
    }

    [Fact]
    public void CalculateSubtotal_SumsQuantityTimesSnapshotPrice()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);

        var order = new Order
        {
            Items =
            {
                new OrderItem { Quantity = 2, UnitPriceSnapshot = 150m },
                new OrderItem { Quantity = 3, UnitPriceSnapshot = 40m }
            }
        };

        Assert.Equal(420m, service.CalculateSubtotal(order));
    }

    [Theory]
    [InlineData(CustomerTier.Standard, 1000, 1000)]
    [InlineData(CustomerTier.Silver, 1000, 950)]
    [InlineData(CustomerTier.Gold, 1000, 900)]
    public void CalculateTotal_AppliesTierDiscountOnSubtotal(CustomerTier tier, decimal unitPrice, decimal expectedTotal)
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);

        var order = new Order
        {
            Customer = new Customer { Tier = tier },
            Items = { new OrderItem { Quantity = 1, UnitPriceSnapshot = unitPrice } }
        };

        Assert.Equal(expectedTotal, service.CalculateTotal(order));
    }

    [Fact]
    public void CalculateTotal_WithoutCustomer_UsesStandardRate()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);

        var order = new Order
        {
            Items = { new OrderItem { Quantity = 2, UnitPriceSnapshot = 250m } }
        };

        Assert.Equal(500m, service.CalculateTotal(order));
    }

    /// <summary>
    /// Regression: Gold was discounted on UnitPriceSnapshot at create AND again in CalculateTotal (100 → 81).
    /// Spec: list price snapshot; discount once on order total (Gold 9 折 → 90).
    /// </summary>
    [Fact]
    public async Task CreateOrder_Gold_SnapshotsListPrice_AndTotalAppliesDiscountOnce()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var gold = TestSetup.AddCustomer(db, CustomerTier.Gold);
        var product = TestSetup.AddProduct(db, unitPrice: 100m);

        var created = await service.CreateOrderAsync(gold.Id, new[] { new NewOrderLine(product.Id, 1) });
        Assert.True(created.Success);

        var order = await service.GetOrderAsync(created.Value!.Id);
        Assert.NotNull(order);

        Assert.Equal(100m, order!.Items.Single().UnitPriceSnapshot);
        Assert.Equal(100m, service.CalculateSubtotal(order));
        Assert.Equal(90m, service.CalculateTotal(order)); // not 81
    }

    /// <summary>
    /// Control: Silver stays correct (snapshot list price, total 95 折 once).
    /// </summary>
    [Fact]
    public async Task CreateOrder_Silver_SnapshotsListPrice_AndTotalAppliesDiscountOnce()
    {
        using var db = TestSetup.CreateContext();
        var service = TestSetup.CreateOrderService(db);
        var silver = TestSetup.AddCustomer(db, CustomerTier.Silver);
        var product = TestSetup.AddProduct(db, unitPrice: 100m);

        var created = await service.CreateOrderAsync(silver.Id, new[] { new NewOrderLine(product.Id, 1) });
        Assert.True(created.Success);

        var order = await service.GetOrderAsync(created.Value!.Id);
        Assert.NotNull(order);

        Assert.Equal(100m, order!.Items.Single().UnitPriceSnapshot);
        Assert.Equal(95m, service.CalculateTotal(order));
    }
}
