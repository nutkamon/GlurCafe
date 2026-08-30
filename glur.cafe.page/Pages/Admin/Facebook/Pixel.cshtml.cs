using glur.cafe.page.Data;
using glur.cafe.page.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Facebook;

[Authorize]
public class PixelModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly FacebookConversionsService _fbCv;
    private readonly IConfiguration _config;
    public PixelModel(ApplicationDbContext db, FacebookConversionsService fbCv, IConfiguration config)
    {
        _db     = db;
        _fbCv   = fbCv;
        _config = config;
    }

    [BindProperty] public string FacebookPixelId { get; set; } = "";
    [BindProperty] public string PixelEnabled    { get; set; } = "false";

    // Read-only: shows whether token is configured in appsettings (not shown in form)
    public bool ConversionsTokenConfigured { get; set; }

    private static readonly string[] Keys = ["FacebookPixelId", "PixelEnabled"];

    public async Task OnGetAsync()
    {
        var dict = await _db.SiteSettings
            .Where(s => Keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        FacebookPixelId = dict.GetValueOrDefault("FacebookPixelId", "");
        PixelEnabled    = dict.GetValueOrDefault("PixelEnabled", "false");
        ConversionsTokenConfigured = !string.IsNullOrWhiteSpace(_config["FacebookSecrets:ConversionsToken"]);

        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        ViewData["ActiveMenu"]  = "fb-pixel";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var values = new Dictionary<string, string>
        {
            ["FacebookPixelId"] = FacebookPixelId?.Trim() ?? "",
            ["PixelEnabled"]    = PixelEnabled == "true" ? "true" : "false",
        };

        foreach (var kv in values)
        {
            var s = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == kv.Key);
            if (s != null) { s.Value = kv.Value; s.UpdatedAt = DateTime.Now; }
        }
        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึก Pixel settings เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestAsync()
    {
        await OnGetAsync();
        var ok = await _fbCv.SendEventAsync("TestEvent", HttpContext, FacebookPixelId);
        TempData[ok ? "Success" : "Error"] = ok
            ? "ส่ง Test Event สำเร็จ ✅ ตรวจสอบที่ Meta Events Manager"
            : "ส่ง Test Event ไม่สำเร็จ — ตรวจสอบ Pixel ID และ ConversionsToken ใน appsettings";
        return RedirectToPage();
    }
}
