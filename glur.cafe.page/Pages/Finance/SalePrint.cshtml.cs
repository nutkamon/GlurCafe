using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Finance;

public class SalePrintModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public SalePrintModel(ApplicationDbContext db) => _db = db;

    public SaleOrder Order { get; set; } = null!;
    public Dictionary<string, string> Site { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var order = await _db.SaleOrders
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (order == null) return NotFound();

        Order = order;
        Site = await _db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
        return Page();
    }
}
