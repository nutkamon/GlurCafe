using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.CostCalc;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<ProductCost> Products { get; set; } = [];

    public async Task OnGetAsync()
    {
        Products = await _db.ProductCosts
            .Include(p => p.Items)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var product = await _db.ProductCosts.FindAsync(id);
        if (product != null)
        {
            _db.ProductCosts.Remove(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ลบข้อมูลเรียบร้อย";
        }
        return RedirectToPage();
    }
}
