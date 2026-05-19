using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Quotations;

[Authorize]
public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DetailModel(ApplicationDbContext db) => _db = db;

    public Quotation Quotation { get; set; } = null!;

    [BindProperty]
    public decimal PaymentAmount { get; set; }

    [BindProperty]
    public string? PaymentMethod { get; set; }

    [BindProperty]
    public string? PaymentNote { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var q = await _db.Quotations.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (q is null) return NotFound();
        Quotation = q;
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        var q = await _db.Quotations.FindAsync(id);
        if (q is null) return NotFound();

        q.Status = status;
        q.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = "อัปเดตสถานะเรียบร้อย";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRecordPaymentAsync(int id)
    {
        var q = await _db.Quotations.FindAsync(id);
        if (q is null) return NotFound();

        q.PaidAmount = q.PaidAmount + PaymentAmount;
        q.PaymentMethod = PaymentMethod;
        q.PaymentNote = PaymentNote;
        q.PaidAt = DateTime.Now;
        q.UpdatedAt = DateTime.Now;

        q.PaymentStatus = q.PaidAmount >= q.NetAmount ? "paid"
            : q.PaidAmount > 0 ? "partial"
            : "unpaid";

        if (q.PaymentStatus == "paid")
            q.Status = "paid";

        await _db.SaveChangesAsync();

        TempData["Success"] = $"บันทึกการชำระ {PaymentAmount:N2} ฿ เรียบร้อย";
        return RedirectToPage(new { id });
    }
}
