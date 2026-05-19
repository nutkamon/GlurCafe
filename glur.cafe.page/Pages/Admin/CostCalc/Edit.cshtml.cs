using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.CostCalc;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var product = await _db.ProductCosts.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        Product = product;
        Items = product.Items.Select(i => new ItemInput
        {
            BeanTypeId = i.BeanTypeId,
            BeanName = i.BeanName,
            PricePerKg = i.PricePerKg,
            RatioPercent = i.RatioPercent
        }).ToList();
        BeanMaster = await _db.BeanTypes.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        return Page();
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

        var existing = await _db.ProductCosts.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == Product.Id);
        if (existing == null) return NotFound();

        decimal lossMultiplier = 1m / (1m - Product.LossRate / 100m);
        decimal beanCost = validItems.Sum(i => i.RatioPercent / 100m * lossMultiplier * i.PricePerKg);

        existing.ProductName = Product.ProductName;
        existing.LossRate = Product.LossRate;
        existing.ProcessingFee = Product.ProcessingFee;
        existing.Notes = Product.Notes;
        existing.SellingPrice = Product.SellingPrice;
        existing.CostPerKg = Math.Round(beanCost + Product.ProcessingFee, 2);
        existing.ProfitPercent = Product.SellingPrice > 0 && existing.CostPerKg > 0
            ? Math.Round((Product.SellingPrice - existing.CostPerKg) / existing.CostPerKg * 100, 2)
            : 0;
        existing.UpdatedAt = DateTime.Now;

        // Replace items
        _db.ProductCostItems.RemoveRange(existing.Items);
        existing.Items = validItems.Select(i => new ProductCostItem
        {
            BeanTypeId = i.BeanTypeId == 0 ? null : i.BeanTypeId,
            BeanName = i.BeanName,
            PricePerKg = i.PricePerKg,
            RatioPercent = i.RatioPercent
        }).ToList();

        await _db.SaveChangesAsync();
        TempData["Success"] = $"อัปเดตสูตร \"{existing.ProductName}\" เรียบร้อย ต้นทุน {existing.CostPerKg:#,##0.00} ฿/กก.";
        return RedirectToPage("Index");
    }
}
