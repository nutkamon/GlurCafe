using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages;

public class MenuModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public MenuModel(ApplicationDbContext db) => _db = db;

    public List<MenuItem> HeaderItems { get; set; } = new();
    public List<MenuFolder> Folders { get; set; } = new();
    public MenuFolder? SelectedFolder { get; set; }
    public bool IsTv { get; set; }
    public Dictionary<string, string> Site { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? cat)
    {
        IsTv = string.Equals(Request.Query["tv"], "1");

        Site = await _db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);

        HeaderItems = await _db.MenuItems
            .Where(i => i.IsActive && i.FilePath != "" && i.Folder.Name == "Header" && i.Folder.IsActive)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync();

        // โหมด tv = แสดงทุกหมวดในหน้าเดียว (สำหรับจอที่ร้าน)
        if (IsTv)
        {
            Folders = await _db.MenuFolders
                .Include(f => f.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                .Where(f => f.IsActive && f.Name != "Header")
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();
            Folders = Folders.Where(f => f.Items.Count > 0).ToList();
            return Page();
        }

        // มี cat = หน้ารายการของหมวดนั้น
        if (cat.HasValue)
        {
            SelectedFolder = await _db.MenuFolders
                .Include(f => f.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(f => f.Id == cat.Value && f.IsActive && f.Name != "Header");
            if (SelectedFolder == null) return RedirectToPage("/Menu");
            return Page();
        }

        // หน้าแรก = ตารางหมวดหมู่ (นับจำนวน item ที่ active)
        var folders = await _db.MenuFolders
            .Include(f => f.Items.Where(i => i.IsActive).OrderBy(i => i.DisplayOrder))
            .Where(f => f.IsActive && f.Name != "Header")
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();
        Folders = folders.Where(f => f.Items.Count > 0).ToList();

        return Page();
    }
}
