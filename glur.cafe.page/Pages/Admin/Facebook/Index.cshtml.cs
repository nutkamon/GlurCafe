using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Facebook;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public Dictionary<string, string> Settings { get; set; } = new();

    public bool MessengerConfigured  => !string.IsNullOrWhiteSpace(Settings.GetValueOrDefault("FacebookPageId"))
                                       && !string.IsNullOrWhiteSpace(Settings.GetValueOrDefault("FacebookAppId"));
    public bool MessengerEnabled     => Settings.GetValueOrDefault("MessengerEnabled") == "true";

    public bool PixelConfigured      => !string.IsNullOrWhiteSpace(Settings.GetValueOrDefault("FacebookPixelId"));
    public bool PixelEnabled         => Settings.GetValueOrDefault("PixelEnabled") == "true";
    public bool ConversionsConfigured => !string.IsNullOrWhiteSpace(Settings.GetValueOrDefault("FacebookConversionsToken"));

    public bool PageFeedConfigured   => !string.IsNullOrWhiteSpace(Settings.GetValueOrDefault("FacebookPageId"));
    public bool PageFeedEnabled      => Settings.GetValueOrDefault("FbPageFeedEnabled") == "true";

    public bool IgConfigured         => !string.IsNullOrWhiteSpace(Settings.GetValueOrDefault("FacebookInstagramUserId"))
                                       && !string.IsNullOrWhiteSpace(Settings.GetValueOrDefault("FacebookInstagramToken"));
    public bool IgEnabled            => Settings.GetValueOrDefault("InstagramFeedEnabled") == "true";

    public bool OgConfigured         => !string.IsNullOrWhiteSpace(Settings.GetValueOrDefault("OgImage"));

    public async Task OnGetAsync()
    {
        Settings = await _db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        ViewData["ActiveMenu"]  = "facebook";
    }
}
