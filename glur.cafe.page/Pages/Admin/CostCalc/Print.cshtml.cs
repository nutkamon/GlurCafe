using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.CostCalc;

[Authorize]
public class PrintModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public PrintModel(ApplicationDbContext db) => _db = db;

    public List<ProductCost> Products { get; set; } = [];
    public Dictionary<string, string> Site { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; } // "all" or "selling" (default: selling only)

    public async Task<IActionResult> OnGetAsync()
    {
        var query = _db.ProductCosts.Include(p => p.Items).AsQueryable();

        if (Filter != "all")
            query = query.Where(p => p.SellingPrice > 0);

        Products = await query.OrderBy(p => p.ProductName).ToListAsync();

        Site = await _db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
        return Page();
    }
}
