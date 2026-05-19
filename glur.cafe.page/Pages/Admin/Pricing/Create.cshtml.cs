using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Pricing;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public PricingPlan Plan { get; set; } = new();
    [BindProperty] public string FeaturesText { get; set; } = "";

    public async Task OnGetAsync()
    {
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        Plan.DisplayOrder = (await _db.PricingPlans.MaxAsync(p => (int?)p.DisplayOrder) ?? 0) + 1;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Remove implicit-required validation for optional fields
        ModelState.Remove(nameof(FeaturesText));
        ModelState.Remove(nameof(Plan) + "." + nameof(Plan.PriceUnit));
        ModelState.Remove(nameof(Plan) + "." + nameof(Plan.Description));
        ModelState.Remove(nameof(Plan) + "." + nameof(Plan.FeaturesJson));
        ModelState.Remove(nameof(Plan) + "." + nameof(Plan.CtaText));

        if (!ModelState.IsValid) return Page();

        // Ensure non-null defaults for optional fields
        Plan.PriceUnit ??= "";
        Plan.Description ??= "";
        Plan.CtaText ??= "สั่งซื้อเลย";

        // Store CKEditor HTML directly
        Plan.FeaturesJson = FeaturesText ?? "";
        Plan.CreatedAt = Plan.UpdatedAt = DateTime.Now;
        _db.PricingPlans.Add(Plan);
        await _db.SaveChangesAsync();
        TempData["Success"] = "เพิ่มราคาสินค้าเรียบร้อยแล้ว";
        return RedirectToPage("Index");
    }
}
