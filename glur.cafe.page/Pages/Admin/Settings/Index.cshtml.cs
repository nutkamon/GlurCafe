using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Settings;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public List<SiteSetting> Settings { get; set; } = new();

    public async Task OnGetAsync()
    {
        Settings = await _db.SiteSettings.OrderBy(s => s.Group).ThenBy(s => s.Key).ToListAsync();
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        ViewData["ActiveMenu"] = "settings";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        foreach (var setting in Settings)
        {
            var existing = await _db.SiteSettings.FindAsync(setting.Id);
            if (existing != null)
            {
                existing.Value = setting.Value;
                existing.UpdatedAt = DateTime.Now;
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "บันทึกการตั้งค่าเรียบร้อยแล้ว";
        return RedirectToPage();
    }
}
