using glur.cafe.page.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Facebook;

[Authorize]
public class FeedModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    public FeedModel(ApplicationDbContext db, IHttpClientFactory http, IConfiguration config)
    {
        _db     = db;
        _http   = http;
        _config = config;
    }

    // Page Feed
    [BindProperty] public string FbPageFeedEnabled   { get; set; } = "false";
    [BindProperty] public string FbPageFeedPostCount { get; set; } = "5";

    // Instagram Feed
    [BindProperty] public string InstagramFeedEnabled    { get; set; } = "false";
    [BindProperty] public string FacebookInstagramUserId { get; set; } = "";
    [BindProperty] public string InstagramFeedCount      { get; set; } = "9";

    // Read-only helpers
    public string FacebookPageId           { get; set; } = "";
    public string FacebookAppId            { get; set; } = "";
    public bool   InstagramTokenConfigured { get; set; }

    private static readonly string[] Keys =
    [
        "FbPageFeedEnabled","FbPageFeedPostCount",
        "InstagramFeedEnabled","FacebookInstagramUserId","InstagramFeedCount",
        "FacebookPageId","FacebookAppId"
    ];

    public async Task OnGetAsync()
    {
        var dict = await _db.SiteSettings
            .Where(s => Keys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        FbPageFeedEnabled        = dict.GetValueOrDefault("FbPageFeedEnabled", "false");
        FbPageFeedPostCount      = dict.GetValueOrDefault("FbPageFeedPostCount", "5");
        InstagramFeedEnabled     = dict.GetValueOrDefault("InstagramFeedEnabled", "false");
        FacebookInstagramUserId  = dict.GetValueOrDefault("FacebookInstagramUserId", "");
        InstagramFeedCount       = dict.GetValueOrDefault("InstagramFeedCount", "9");
        FacebookPageId           = dict.GetValueOrDefault("FacebookPageId", "");
        FacebookAppId            = dict.GetValueOrDefault("FacebookAppId", "");
        InstagramTokenConfigured = !string.IsNullOrWhiteSpace(_config["FacebookSecrets:InstagramToken"]);

        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        ViewData["ActiveMenu"]  = "fb-feed";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var values = new Dictionary<string, string>
        {
            ["FbPageFeedEnabled"]       = FbPageFeedEnabled == "true" ? "true" : "false",
            ["FbPageFeedPostCount"]     = FbPageFeedPostCount?.Trim() ?? "5",
            ["InstagramFeedEnabled"]    = InstagramFeedEnabled == "true" ? "true" : "false",
            ["FacebookInstagramUserId"] = FacebookInstagramUserId?.Trim() ?? "",
            ["InstagramFeedCount"]      = InstagramFeedCount?.Trim() ?? "9",
        };

        foreach (var kv in values)
        {
            var s = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == kv.Key);
            if (s != null) { s.Value = kv.Value; s.UpdatedAt = DateTime.Now; }
        }
        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึก Social Feed settings เรียบร้อยแล้ว";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestIgAsync()
    {
        await OnGetAsync();
        var token = _config["FacebookSecrets:InstagramToken"] ?? "";
        if (string.IsNullOrWhiteSpace(token))
        {
            TempData["Error"] = "❌ ยังไม่ตั้งค่า InstagramToken ใน appsettings.json";
            return RedirectToPage();
        }
        try
        {
            var client = _http.CreateClient("meta");
            var url = $"https://graph.instagram.com/me?access_token={Uri.EscapeDataString(token)}&fields=id,name";
            await client.GetStringAsync(url);
            TempData["Success"] = "✅ Token ใช้งานได้";
        }
        catch
        {
            TempData["Error"] = "❌ Token ไม่ถูกต้องหรือหมดอายุ — กรุณาอัปเดตใน appsettings.json";
        }
        return RedirectToPage();
    }
}
