using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Users;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public AdminUser AdminUser { get; set; } = new();
    [BindProperty] public string? NewPassword { get; set; }
    [BindProperty] public string? ConfirmPassword { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _db.AdminUsers.FindAsync(id);
        if (user == null) return NotFound();
        AdminUser = user;
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var existing = await _db.AdminUsers.FindAsync(AdminUser.Id);
        if (existing == null) return NotFound();

        if (await _db.AdminUsers.AnyAsync(u => u.Username == AdminUser.Username && u.Id != AdminUser.Id))
        {
            ModelState.AddModelError("AdminUser.Username", "ชื่อผู้ใช้นี้มีอยู่แล้ว");
            return Page();
        }

        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            if (NewPassword.Length < 6)
            {
                ModelState.AddModelError("NewPassword", "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร");
                return Page();
            }
            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "รหัสผ่านไม่ตรงกัน");
                return Page();
            }
            existing.PasswordHash = DbSeeder.HashPassword(NewPassword);
        }

        existing.Username = AdminUser.Username;
        existing.DisplayName = AdminUser.DisplayName;
        existing.IsActive = AdminUser.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = "อัปเดตข้อมูลผู้ดูแลระบบเรียบร้อยแล้ว";
        return RedirectToPage("Index");
    }
}
