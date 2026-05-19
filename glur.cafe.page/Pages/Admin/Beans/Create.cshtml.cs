using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace glur.cafe.page.Pages.Admin.Beans;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public BeanType Bean { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        Bean.CreatedAt = DateTime.Now;
        Bean.UpdatedAt = DateTime.Now;
        _db.BeanTypes.Add(Bean);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"เพิ่มสาร \"{Bean.Name}\" เรียบร้อย";
        return RedirectToPage("Index");
    }
}
