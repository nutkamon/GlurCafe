using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Portfolios;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;
    public int TotalPages { get; set; }
    public const int PageSize = 20;
    public List<Portfolio> Portfolios { get; set; } = new();

    public async Task OnGetAsync()
    {
        var query = _db.Portfolios.OrderBy(p => p.DisplayOrder);
        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        PageNum = Math.Max(1, Math.Min(PageNum, Math.Max(1, TotalPages)));
        Portfolios = await query.Skip((PageNum - 1) * PageSize).Take(PageSize).ToListAsync();
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await _db.Portfolios.FindAsync(id);
        if (item != null) { _db.Portfolios.Remove(item); await _db.SaveChangesAsync(); TempData["Success"] = "ลบรายการแกลเลอรี่เรียบร้อยแล้ว"; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var item = await _db.Portfolios.FindAsync(id);
        if (item != null) { item.IsActive = !item.IsActive; await _db.SaveChangesAsync(); TempData["Success"] = item.IsActive ? "เปิดใช้งานเรียบร้อยแล้ว" : "ปิดใช้งานเรียบร้อยแล้ว"; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBulkDeleteAsync(int[] ids)
    {
        if (ids.Length > 0)
        {
            var items = await _db.Portfolios.Where(p => ids.Contains(p.Id)).ToListAsync();
            _db.Portfolios.RemoveRange(items);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"ลบ {items.Count} รายการเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }
}
