using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Menu;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public IndexModel(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public List<MenuFolder> Folders { get; set; } = new();
    public int NextOrder { get; set; }

    public async Task OnGetAsync()
    {
        Folders = await _db.MenuFolders
            .Include(f => f.Items)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
        NextOrder = (await _db.MenuFolders.MaxAsync(f => (int?)f.DisplayOrder) ?? 0) + 1;
    }

    public async Task<IActionResult> OnPostCreateAsync(string Name, string? IconClassName, int DisplayOrder)
    {
        if (string.IsNullOrWhiteSpace(Name)) return RedirectToPage();
        _db.MenuFolders.Add(new MenuFolder
        {
            Name          = Name.Trim(),
            IconClassName = string.IsNullOrWhiteSpace(IconClassName) ? "bi-cup-hot" : IconClassName.Trim(),
            DisplayOrder  = DisplayOrder,
            IsActive      = true,
            CreatedAt     = DateTime.Now
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "เพิ่มหมวดเรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int id, string Name, string? IconClassName, int DisplayOrder)
    {
        var folder = await _db.MenuFolders.FindAsync(id);
        if (folder != null)
        {
            folder.Name          = Name.Trim();
            folder.IconClassName = string.IsNullOrWhiteSpace(IconClassName) ? "bi-cup-hot" : IconClassName.Trim();
            folder.DisplayOrder  = DisplayOrder;
            await _db.SaveChangesAsync();
            TempData["Success"] = "แก้ไขหมวดเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var folder = await _db.MenuFolders.FindAsync(id);
        if (folder != null)
        {
            folder.IsActive = !folder.IsActive;
            await _db.SaveChangesAsync();
            TempData["Success"] = folder.IsActive ? "เปิดใช้งานเรียบร้อยแล้ว" : "ปิดใช้งานเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var folder = await _db.MenuFolders.Include(f => f.Items).FirstOrDefaultAsync(f => f.Id == id);
        if (folder != null)
        {
            // ลบไฟล์ในโฟลเดอร์นี้ทั้งหมด
            var dir = Path.Combine(_env.WebRootPath, "img", "menu", id.ToString());
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);

            _db.MenuFolders.Remove(folder); // cascade ลบ MenuItems
            await _db.SaveChangesAsync();
            TempData["Success"] = "ลบหมวดเรียบร้อยแล้ว";
        }
        return RedirectToPage();
    }
}
