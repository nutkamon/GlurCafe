using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Quotations;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;
    public int TotalPages { get; set; }
    public const int PageSize = 20;
    public List<Quotation> Quotations { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    // Status counts for tabs
    public int CountAll { get; set; }
    public int CountDraft { get; set; }
    public int CountSent { get; set; }
    public int CountAccepted { get; set; }
    public int CountPaid { get; set; }
    public int CountCompleted { get; set; }
    public int CountCancelled { get; set; }

    // Revenue summary
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public decimal PendingPaymentAmount { get; set; }
    public int QuotationsPaidCount { get; set; }
    public int QuotationsAcceptedCount { get; set; }

    public async Task OnGetAsync()
    {
        var query = _db.Quotations.AsQueryable();

        // Count per tab
        var all = _db.Quotations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Search))
            all = all.Where(q => q.QuotationNumber.Contains(Search) || q.CustomerName.Contains(Search));

        CountAll = await all.CountAsync();
        CountDraft = await all.CountAsync(q => q.Status == "draft");
        CountSent = await all.CountAsync(q => q.Status == "sent");
        CountAccepted = await all.CountAsync(q => q.Status == "accepted");
        CountPaid = await all.CountAsync(q => q.Status == "paid");
        CountCompleted = await all.CountAsync(q => q.Status == "completed");
        CountCancelled = await all.CountAsync(q => q.Status == "cancelled");

        // Revenue summary (global, not filtered by search)
        var now = DateTime.Now;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var paid = _db.Quotations.Where(q => q.Status == "paid" || q.Status == "completed");
        TotalRevenue = (decimal)(await paid.SumAsync(q => (double?)q.NetAmount) ?? 0);
        QuotationsPaidCount = await paid.CountAsync();
        RevenueThisMonth = (decimal)(await paid.Where(q => q.CreatedAt >= thisMonthStart).SumAsync(q => (double?)q.NetAmount) ?? 0);
        RevenueLastMonth = (decimal)(await paid.Where(q => q.CreatedAt >= lastMonthStart && q.CreatedAt < thisMonthStart).SumAsync(q => (double?)q.NetAmount) ?? 0);
        var accepted = _db.Quotations.Where(q => q.Status == "accepted");
        PendingPaymentAmount = (decimal)(await accepted.SumAsync(q => (double?)q.NetAmount) ?? 0);
        QuotationsAcceptedCount = await accepted.CountAsync();

        // Filtered list
        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(q => q.QuotationNumber.Contains(Search) || q.CustomerName.Contains(Search));

        if (!string.IsNullOrWhiteSpace(StatusFilter))
            query = query.Where(q => q.Status == StatusFilter);

        var ordered = query.OrderByDescending(q => q.CreatedAt);
        var total = await ordered.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        PageNum = Math.Max(1, Math.Min(PageNum, Math.Max(1, TotalPages)));
        Quotations = await ordered.Skip((PageNum - 1) * PageSize).Take(PageSize).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var q = await _db.Quotations.FindAsync(id);
        if (q != null)
        {
            _db.Quotations.Remove(q);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ลบใบเสนอราคาเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBulkDeleteAsync(int[] ids)
    {
        if (ids.Length > 0)
        {
            var items = await _db.Quotations.Where(q => ids.Contains(q.Id)).ToListAsync();
            _db.Quotations.RemoveRange(items);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"ลบ {items.Count} ใบเสนอราคาเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }
}
