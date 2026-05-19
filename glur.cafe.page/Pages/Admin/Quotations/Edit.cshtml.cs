using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Quotations;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public Quotation Quotation { get; set; } = new();

    [BindProperty]
    public List<QuotationItemInput> Items { get; set; } = [];

    public class QuotationItemInput
    {
        public int Id { get; set; }
        public string Description { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public string Unit { get; set; } = "ชิ้น";
        public decimal UnitPrice { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var q = await _db.Quotations.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (q is null) return NotFound();

        Quotation = q;
        Items = q.Items.Select(i => new QuotationItemInput
        {
            Id = i.Id,
            Description = i.Description,
            Quantity = i.Quantity,
            Unit = i.Unit,
            UnitPrice = i.UnitPrice
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Quotation.Items");

        if (!ModelState.IsValid)
            return Page();

        var existing = await _db.Quotations.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == Quotation.Id);
        if (existing is null) return NotFound();

        existing.CustomerName = Quotation.CustomerName;
        existing.Phone = Quotation.Phone;
        existing.Email = Quotation.Email;
        existing.Address = Quotation.Address;
        existing.Notes = Quotation.Notes;
        existing.Status = Quotation.Status;
        existing.DiscountPercent = Quotation.DiscountPercent;
        existing.UpdatedAt = DateTime.Now;

        // Replace items
        _db.QuotationItems.RemoveRange(existing.Items);
        var validItems = Items.Where(i => !string.IsNullOrWhiteSpace(i.Description)).ToList();
        existing.Items = validItems.Select(i => new QuotationItem
        {
            QuotationId = existing.Id,
            Description = i.Description,
            Quantity = i.Quantity,
            Unit = i.Unit,
            UnitPrice = i.UnitPrice,
            Amount = i.Quantity * i.UnitPrice
        }).ToList();

        existing.TotalAmount = existing.Items.Sum(i => i.Amount);
        existing.NetAmount = existing.TotalAmount * (1 - (existing.DiscountPercent / 100m));

        await _db.SaveChangesAsync();

        TempData["Success"] = "อัปเดตใบเสนอราคาเรียบร้อย";
        return RedirectToPage("Detail", new { id = existing.Id });
    }
}
