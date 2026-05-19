using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Users;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DeleteModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public AdminUser AdminUser { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _db.AdminUsers.FindAsync(id);
        if (user == null) return NotFound();
        AdminUser = user;
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var user = await _db.AdminUsers.FindAsync(id);
        if (user == null) return NotFound();

        // ป้องกันลบ admin คนสุดท้าย
        if (await _db.AdminUsers.CountAsync(u => u.IsActive) <= 1 && user.IsActive)
            return RedirectToPage("Index");

        _db.AdminUsers.Remove(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = "ลบผู้ดูแลระบบเรียบร้อยแล้ว";
        return RedirectToPage("Index");
    }
}
