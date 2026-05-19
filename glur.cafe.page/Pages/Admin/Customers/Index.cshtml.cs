using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Customers;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;
    public int TotalPages { get; set; }
    public const int PageSize = 20;
    public List<Customer> Customers { get; set; } = [];

    [BindProperty(SupportsGet = true)] public string? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }

    public int CountAll { get; set; }
    public int CountLead { get; set; }
    public int CountProspect { get; set; }
    public int CountActive { get; set; }
    public int CountVip { get; set; }
    public int CountInactive { get; set; }

    public async Task OnGetAsync()
    {
        ViewData["ActiveMenu"] = "customers";
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);

        var all = _db.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(Search))
            all = all.Where(c => c.FullName.Contains(Search) || (c.Phone != null && c.Phone.Contains(Search))
                || (c.Email != null && c.Email.Contains(Search)) || (c.Company != null && c.Company.Contains(Search)));

        CountAll = await all.CountAsync();
        CountLead = await all.CountAsync(c => c.Status == "lead");
        CountProspect = await all.CountAsync(c => c.Status == "prospect");
        CountActive = await all.CountAsync(c => c.Status == "active");
        CountVip = await all.CountAsync(c => c.Status == "vip");
        CountInactive = await all.CountAsync(c => c.Status == "inactive");

        var query = all;
        if (!string.IsNullOrWhiteSpace(StatusFilter))
            query = query.Where(c => c.Status == StatusFilter);
        if (!string.IsNullOrWhiteSpace(TypeFilter))
            query = query.Where(c => c.CustomerType == TypeFilter);

        var ordered = query.OrderByDescending(c => c.CreatedAt);
        var total = await ordered.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        PageNum = Math.Max(1, Math.Min(PageNum, Math.Max(1, TotalPages)));
        Customers = await ordered.Skip((PageNum - 1) * PageSize).Take(PageSize).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c != null)
        {
            _db.Customers.Remove(c);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ลบลูกค้าเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBulkDeleteAsync(int[] ids)
    {
        if (ids.Length > 0)
        {
            var items = await _db.Customers.Where(c => ids.Contains(c.Id)).ToListAsync();
            _db.Customers.RemoveRange(items);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"ลบ {items.Count} รายการเรียบร้อย";
        }
        return RedirectToPage();
    }
}
