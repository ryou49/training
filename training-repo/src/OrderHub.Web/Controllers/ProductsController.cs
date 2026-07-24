using Microsoft.AspNetCore.Mvc;
using OrderHub.Core.Services;
using OrderHub.Web.ViewModels;

namespace OrderHub.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();

        var vm = new ProductListViewModel
        {
            Products = products.Select(p => new ProductRowViewModel
            {
                Sku = p.Sku,
                Name = p.Name,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive
            }).ToList()
        };

        return View(vm);
    }

    /// <summary>
    /// GET /Products/LowStock?threshold=10 — low-stock product mode (from Products filter or nav).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LowStock(int? threshold)
    {
        var vm = new LowStockViewModel();

        if (threshold.HasValue)
        {
            vm.Threshold = threshold.Value;
            // Re-validate explicit query values (including <= 0).
            TryValidateModel(vm);
        }
        else
        {
            vm.Threshold = 10;
        }

        if (!ModelState.IsValid)
            return View(vm);

        var rows = await _productService.GetLowStockAsync(vm.Threshold);
        vm.Products = rows.Select(r => new LowStockRowViewModel
        {
            Sku = r.Sku,
            Name = r.Name,
            StockQuantity = r.StockQuantity,
            SoldLast30Days = r.SoldLast30Days
        }).ToList();

        return View(vm);
    }
}

