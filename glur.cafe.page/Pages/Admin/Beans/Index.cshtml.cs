using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Beans;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<BeanType> Beans { get; set; } = [];

    public async Task OnGetAsync()
    {
        Beans = await _db.BeanTypes.OrderBy(b => b.Name).ToListAsync();
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var bean = await _db.BeanTypes.FindAsync(id);
        if (bean != null)
        {
            _db.BeanTypes.Remove(bean);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ลบข้อมูลสารกาแฟเรียบร้อย";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var bean = await _db.BeanTypes.FindAsync(id);
        if (bean != null)
        {
            bean.IsActive = !bean.IsActive;
            bean.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
