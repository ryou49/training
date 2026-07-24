namespace OrderHub.Core.Services;

/// <summary>
/// Read model for low-stock listing (not an EF entity).
/// </summary>
public class LowStockProductInfo
{
    public int Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public int SoldLast30Days { get; init; }
}
