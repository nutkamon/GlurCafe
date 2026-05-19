using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Pricing;

[Authorize]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public PricingPlan Plan { get; set; } = new();
    [BindProperty] public string FeaturesText { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _db.PricingPlans.FindAsync(id);
        if (item == null) return NotFound();
        Plan = item;
        // If old JSON array format, convert to HTML list for CKEditor
        var json = item.FeaturesJson ?? "";
        if (json.TrimStart().StartsWith("["))
        {
            try
            {
                var features = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new();
                FeaturesText = "<ul>" + string.Join("", features.Select(f => $"<li>{System.Net.WebUtility.HtmlEncode(f)}</li>")) + "</ul>";
            }
            catch { FeaturesText = json; }
        }
        else
        {
            FeaturesText = json;
        }
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove(nameof(FeaturesText));
        ModelState.Remove(nameof(Plan) + "." + nameof(Plan.PriceUnit));
        ModelState.Remove(nameof(Plan) + "." + nameof(Plan.Description));
        ModelState.Remove(nameof(Plan) + "." + nameof(Plan.FeaturesJson));
        ModelState.Remove(nameof(Plan) + "." + nameof(Plan.CtaText));

        if (!ModelState.IsValid) return Page();

        Plan.PriceUnit ??= "";
        Plan.Description ??= "";
        Plan.CtaText ??= "สั่งซื้อเลย";

        Plan.FeaturesJson = FeaturesText ?? "";
        Plan.UpdatedAt = DateTime.Now;
        _db.PricingPlans.Update(Plan);
        await _db.SaveChangesAsync();
        TempData["Success"] = "อัปเดตราคาสินค้าเรียบร้อยแล้ว";
        return RedirectToPage("Index");
    }
}
