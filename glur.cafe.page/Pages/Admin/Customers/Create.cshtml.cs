using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace glur.cafe.page.Pages.Admin.Customers;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public Customer Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Input.Interactions");
        ModelState.Remove("Input.Quotations");
        ModelState.Remove("Input.ContactMessages");

        if (!ModelState.IsValid)
            return Page();

        Input.CreatedAt = DateTime.Now;
        Input.UpdatedAt = DateTime.Now;

        _db.Customers.Add(Input);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"เพิ่มลูกค้า {Input.FullName} เรียบร้อย";
        return RedirectToPage("Index");
    }
}
