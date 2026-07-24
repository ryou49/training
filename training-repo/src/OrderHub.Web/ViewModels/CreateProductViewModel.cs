using System.ComponentModel.DataAnnotations;

namespace OrderHub.Web.ViewModels;

public class CreateProductViewModel
{
    [Required(ErrorMessage = "SKU 為必填")]
    [StringLength(20, ErrorMessage = "SKU 長度不可超過 20")]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "名稱為必填")]
    [StringLength(100, ErrorMessage = "名稱長度不可超過 100")]
    [Display(Name = "名稱")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999999, ErrorMessage = "單價必須大於 0")]
    [Display(Name = "單價")]
    public decimal UnitPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "庫存不可為負數")]
    [Display(Name = "初始庫存")]
    public int StockQuantity { get; set; }

    [Display(Name = "販售中")]
    public bool IsActive { get; set; } = true;
}
