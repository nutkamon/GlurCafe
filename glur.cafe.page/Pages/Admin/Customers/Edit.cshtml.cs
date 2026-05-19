using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Customers;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public Customer Input { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["ActiveMenu"] = "customers";
        var c = await _db.Customers.FindAsync(id);
        if (c is null) return NotFound();
        Input = c;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        ModelState.Remove("Input.Interactions");
        ModelState.Remove("Input.Quotations");
        ModelState.Remove("Input.ContactMessages");

        if (!ModelState.IsValid)
            return Page();

        var existing = await _db.Customers.FindAsync(id);
        if (existing is null) return NotFound();

        existing.FullName = Input.FullName;
        existing.Phone = Input.Phone;
        existing.Email = Input.Email;
        existing.Company = Input.Company;
        existing.Address = Input.Address;
        existing.CustomerType = Input.CustomerType;
        existing.Status = Input.Status;
        existing.Source = Input.Source;
        existing.Tags = Input.Tags;
        existing.Notes = Input.Notes;
        existing.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึกข้อมูลลูกค้าเรียบร้อย";
        return RedirectToPage("Detail", new { id });
    }
}
