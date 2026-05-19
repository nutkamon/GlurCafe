using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Quotations;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public Quotation Quotation { get; set; } = new();

    [BindProperty]
    public List<QuotationItemInput> Items { get; set; } = [];

    public class QuotationItemInput
    {
        public string Description { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public string Unit { get; set; } = "ชิ้น";
        public decimal UnitPrice { get; set; }
    }

    public async Task OnGetAsync()
    {
        Quotation.QuotationNumber = await GenerateQuotationNumberAsync();
        Quotation.Status = "draft";
        Quotation.PaymentStatus = "unpaid";
        Quotation.CreatedAt = DateTime.Now;
    }

    private async Task<string> GenerateQuotationNumberAsync()
    {
        string prefix = $"QT-{DateTime.Now:yyyyMMdd}-";
        int count = await _db.Quotations.CountAsync(q => q.QuotationNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Remove navigation-property validation noise
        ModelState.Remove("Quotation.Items");

        if (!ModelState.IsValid)
            return Page();

        if (!Items.Any(i => !string.IsNullOrWhiteSpace(i.Description)))
        {
            ModelState.AddModelError("", "กรุณาเพิ่มรายการอย่างน้อย 1 รายการ");
            return Page();
        }

        Quotation.CreatedAt = DateTime.Now;
        Quotation.UpdatedAt = DateTime.Now;

        var validItems = Items.Where(i => !string.IsNullOrWhiteSpace(i.Description)).ToList();
        Quotation.Items = validItems.Select(i => new QuotationItem
        {
            Description = i.Description,
            Quantity = i.Quantity,
            Unit = i.Unit,
            UnitPrice = i.UnitPrice,
            Amount = i.Quantity * i.UnitPrice
        }).ToList();

        Quotation.TotalAmount = Quotation.Items.Sum(i => i.Amount);
        Quotation.NetAmount = Quotation.TotalAmount * (1 - (Quotation.DiscountPercent / 100m));

        _db.Quotations.Add(Quotation);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"สร้างใบเสนอราคา {Quotation.QuotationNumber} เรียบร้อย";
        return RedirectToPage("Index");
    }
}
