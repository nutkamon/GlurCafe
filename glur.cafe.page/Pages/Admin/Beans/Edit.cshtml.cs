using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace glur.cafe.page.Pages.Admin.Beans;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public BeanType Bean { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var bean = await _db.BeanTypes.FindAsync(id);
        if (bean == null) return NotFound();
        Bean = bean;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existing = await _db.BeanTypes.FindAsync(Bean.Id);
        if (existing == null) return NotFound();

        existing.Name = Bean.Name;
        existing.PricePerKg = Bean.PricePerKg;
        existing.Notes = Bean.Notes;
        existing.IsActive = Bean.IsActive;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"อัปเดตสาร \"{Bean.Name}\" เรียบร้อย";
        return RedirectToPage("Index");
    }
}
