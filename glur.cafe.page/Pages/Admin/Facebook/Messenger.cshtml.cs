using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Facebook;

[Authorize]
public class MessengerModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public MessengerModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public string FacebookPageId    { get; set; } = "";
    [BindProperty] public string FacebookAppId     { get; set; } = "";
    [BindProperty] public string MessengerEnabled  { get; set; } = "false";
    [BindProperty] public string MessengerThemeColor { get; set; } = "#00A59A";
    [BindProperty] public string MessengerGreeting { get; set; } = "";

    private static readonly string[] Keys = ["FacebookPageId", "FacebookAppId", "MessengerEnabled", "MessengerThemeColor", "MessengerGreeting"];

    public async Task OnGetAsync()
    {
        var dict = await _db.SiteSettings
            .Where(s => Keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        FacebookPageId     = dict.GetValueOrDefault("FacebookPageId", "");
        FacebookAppId      = dict.GetValueOrDefault("FacebookAppId", "");
        MessengerEnabled   = dict.GetValueOrDefault("MessengerEnabled", "false");
        MessengerThemeColor = dict.GetValueOrDefault("MessengerThemeColor", "#00A59A");
        MessengerGreeting  = dict.GetValueOrDefault("MessengerGreeting", "สวัสดี! มีอะไรให้ช่วยไหมครับ? ☕");

        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        ViewData["ActiveMenu"]  = "fb-messenger";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var values = new Dictionary<string, string>
        {
            ["FacebookPageId"]     = FacebookPageId?.Trim() ?? "",
            ["FacebookAppId"]      = FacebookAppId?.Trim() ?? "",
            ["MessengerEnabled"]   = MessengerEnabled == "true" ? "true" : "false",
            ["MessengerThemeColor"]= MessengerThemeColor?.Trim() ?? "#00A59A",
            ["MessengerGreeting"]  = MessengerGreeting?.Trim() ?? "",
        };

        foreach (var kv in values)
        {
            var s = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == kv.Key);
            if (s != null) { s.Value = kv.Value; s.UpdatedAt = DateTime.Now; }
        }
        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึก Messenger Chat settings เรียบร้อยแล้ว";
        return RedirectToPage();
    }
}
