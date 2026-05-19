using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin;

[Authorize]
public class MessagesModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public MessagesModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int PageNum { get; set; } = 1;
    public int TotalPages { get; set; }
    public const int PageSize = 20;
    public List<ContactMessage> Messages { get; set; } = new();
    public int UnreadCount { get; set; }

    public async Task OnGetAsync(bool markAllRead = false)
    {
        if (markAllRead)
        {
            await _db.ContactMessages.Where(m => !m.IsRead)
                                     .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
        }

        var query = _db.ContactMessages.OrderByDescending(m => m.CreatedAt);
        var total = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(total / (double)PageSize);
        PageNum = Math.Max(1, Math.Min(PageNum, Math.Max(1, TotalPages)));
        Messages    = await query.Skip((PageNum - 1) * PageSize).Take(PageSize).ToListAsync();
        UnreadCount = await _db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    public async Task<IActionResult> OnPostMarkReadAsync(int id)
    {
        var msg = await _db.ContactMessages.FindAsync(id);
        if (msg != null)
        {
            msg.IsRead = true;
            await _db.SaveChangesAsync();
        }
        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostBulkDeleteAsync(int[] ids)
    {
        if (ids.Length > 0)
        {
            var items = await _db.ContactMessages.Where(m => ids.Contains(m.Id)).ToListAsync();
            _db.ContactMessages.RemoveRange(items);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"ลบ {items.Count} ข้อความเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }
}
