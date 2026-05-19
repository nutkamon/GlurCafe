using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Users;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public AdminUser AdminUser { get; set; } = new();
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";

    public async Task OnGetAsync()
    {
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
        {
            ModelState.AddModelError("Password", "รหัสผ่านต้องมีอย่างน้อย 6 ตัวอักษร");
            return Page();
        }
        if (Password != ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "รหัสผ่านไม่ตรงกัน");
            return Page();
        }
        if (await _db.AdminUsers.AnyAsync(u => u.Username == AdminUser.Username))
        {
            ModelState.AddModelError("AdminUser.Username", "ชื่อผู้ใช้นี้มีอยู่แล้ว");
            return Page();
        }

        AdminUser.PasswordHash = DbSeeder.HashPassword(Password);
        AdminUser.CreatedAt = DateTime.Now;
        _db.AdminUsers.Add(AdminUser);
        await _db.SaveChangesAsync();
        TempData["Success"] = "เพิ่มผู้ดูแลระบบเรียบร้อยแล้ว";
        return RedirectToPage("Index");
    }
}
