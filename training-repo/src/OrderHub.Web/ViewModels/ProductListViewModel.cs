namespace OrderHub.Web.ViewModels;

public class ProductListViewModel
{
    /// <summary>
    /// all | active | inactive
    /// </summary>
    public string Status { get; set; } = "all";

    public IReadOnlyList<ProductRowViewModel> Products { get; set; } = Array.Empty<ProductRowViewModel>();
}

public class ProductRowViewModel
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
}
