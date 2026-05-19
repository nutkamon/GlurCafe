using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.CostCalc;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public ProductCost Product { get; set; } = new();

    [BindProperty]
    public List<ItemInput> Items { get; set; } = [];

    public List<BeanType> BeanMaster { get; set; } = [];

    public class ItemInput
    {
        public int? BeanTypeId { get; set; }
        public string BeanName { get; set; } = "";
        public decimal PricePerKg { get; set; }
        public decimal RatioPercent { get; set; }
    }

    public async Task OnGetAsync()
    {
        BeanMaster = await _db.BeanTypes.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        Product.LossRate = 20;
        Product.ProcessingFee = 50;
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        BeanMaster = await _db.BeanTypes.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        ModelState.Remove("Product.Items");

        var validItems = Items.Where(i => !string.IsNullOrWhiteSpace(i.BeanName) && i.RatioPercent > 0).ToList();
        if (!validItems.Any())
        {
            ModelState.AddModelError("", "กรุณาเพิ่มสารกาแฟอย่างน้อย 1 รายการ");
        }

        decimal totalRatio = validItems.Sum(i => i.RatioPercent);
        if (Math.Abs(totalRatio - 100) > 0.01m)
        {
            ModelState.AddModelError("", $"รวมอัตราส่วนต้องเท่ากับ 100% (ปัจจุบัน: {totalRatio:0.##}%)");
        }

        if (!ModelState.IsValid) return Page();

        decimal lossMultiplier = 1m / (1m - Product.LossRate / 100m);
        decimal beanCost = validItems.Sum(i => i.RatioPercent / 100m * lossMultiplier * i.PricePerKg);
        Product.CostPerKg = Math.Round(beanCost + Product.ProcessingFee, 2);
        Product.ProfitPercent = Product.SellingPrice > 0 && Product.CostPerKg > 0
            ? Math.Round((Product.SellingPrice - Product.CostPerKg) / Product.CostPerKg * 100, 2)
            : 0;

        Product.CreatedAt = DateTime.Now;
        Product.UpdatedAt = DateTime.Now;
        Product.Items = validItems.Select(i => new ProductCostItem
        {
            BeanTypeId = i.BeanTypeId == 0 ? null : i.BeanTypeId,
            BeanName = i.BeanName,
            PricePerKg = i.PricePerKg,
            RatioPercent = i.RatioPercent
        }).ToList();

        _db.ProductCosts.Add(Product);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"บันทึกสูตร \"{Product.ProductName}\" เรียบร้อย ต้นทุน {Product.CostPerKg:#,##0.00} ฿/กก.";
        return RedirectToPage("Index");
    }
}
