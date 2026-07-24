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

    public async Task<IActionResult> Index(string? status)
    {
        var filter = ParseStatusFilter(status);
        var products = await _productService.GetByStatusAsync(filter);

        var vm = new ProductListViewModel
        {
            Status = ToStatusQuery(filter),
            Products = products.Select(p => new ProductRowViewModel
            {
                Id = p.Id,
                Sku = p.Sku,
                Name = p.Name,
                UnitPrice = p.UnitPrice,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive
            }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateProductViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductViewModel vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var result = await _productService.CreateAsync(
            vm.Sku,
            vm.Name,
            vm.UnitPrice,
            vm.StockQuantity,
            vm.IsActive);

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(vm);
        }

        TempData["Success"] = $"商品 {result.Value!.Sku} 建立成功";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateProductViewModel vm)
    {
        var status = string.IsNullOrWhiteSpace(vm.Status) ? "all" : vm.Status;

        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join("；",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrEmpty(m)));
            return RedirectToAction(nameof(Index), new { status });
        }

        var result = await _productService.UpdateAsync(
            vm.Id,
            vm.Sku,
            vm.Name,
            vm.StockQuantity,
            vm.IsActive);

        if (!result.Success)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index), new { status });
        }

        var statusLabel = result.Value!.IsActive ? "販售中" : "已停售";
        TempData["Success"] = $"已更新商品 {result.Value.Sku}：庫存 {result.Value.StockQuantity}，{statusLabel}";
        return RedirectToAction(nameof(Index), new { status });
    }

    /// <summary>
    /// GET /Products/LowStock?threshold=10 — low-stock product mode (from Products filter).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> LowStock(int? threshold)
    {
        var vm = new LowStockViewModel();

        if (threshold.HasValue)
        {
            vm.Threshold = threshold.Value;
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

    private static ProductStatusFilter ParseStatusFilter(string? status) =>
        status?.Trim().ToLowerInvariant() switch
        {
            "active" => ProductStatusFilter.Active,
            "inactive" => ProductStatusFilter.Inactive,
            _ => ProductStatusFilter.All
        };

    private static string ToStatusQuery(ProductStatusFilter filter) =>
        filter switch
        {
            ProductStatusFilter.Active => "active",
            ProductStatusFilter.Inactive => "inactive",
            _ => "all"
        };
}
