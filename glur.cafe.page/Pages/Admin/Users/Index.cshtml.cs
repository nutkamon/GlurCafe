using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Users;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;
    public int TotalPages { get; set; }
    public const int PageSize = 20;
    public List<AdminUser> Users { get; set; } = new();

    public async Task OnGetAsync()
    {
        var query = _db.AdminUsers.OrderBy(u => u.Username);
        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        PageNum = Math.Max(1, Math.Min(PageNum, Math.Max(1, TotalPages)));
        Users = await query.Skip((PageNum - 1) * PageSize).Take(PageSize).ToListAsync();
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    public async Task<IActionResult> OnPostBulkDeleteAsync(int[] ids)
    {
        if (ids.Length > 0)
        {
            var items = await _db.AdminUsers.Where(u => ids.Contains(u.Id)).ToListAsync();
            _db.AdminUsers.RemoveRange(items);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"ลบ {items.Count} ผู้ดูแลเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }
}
