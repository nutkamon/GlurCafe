using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Quotations;

[Authorize]
public class PrintModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public PrintModel(ApplicationDbContext db) => _db = db;

    public Quotation Quotation { get; set; } = null!;
    public Dictionary<string, string> Settings { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var q = await _db.Quotations.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (q is null) return NotFound();
        Quotation = q;

        Settings = await _db.SiteSettings
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        return Page();
    }

    public string S(string key, string fallback = "") =>
        Settings.TryGetValue(key, out var v) ? v : fallback;
}
