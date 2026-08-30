using glur.cafe.page.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace glur.cafe.page.Pages.Api;

/// <summary>
/// Server-side proxy for Instagram Graph API.
/// Instagram token stays on the server — never exposed to the browser.
/// GET /api/instagram-feed → JSON array of { mediaUrl, permalink, caption, mediaType }
/// </summary>
public class InstagramFeedModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public InstagramFeedModel(ApplicationDbContext db, IHttpClientFactory http, IConfiguration config)
    {
        _db     = db;
        _http   = http;
        _config = config;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Token from appsettings (never from DB)
        var token = _config["FacebookSecrets:InstagramToken"] ?? "";
        if (string.IsNullOrWhiteSpace(token))
            return new JsonResult(new { error = "Instagram token not configured" }) { StatusCode = 503 };

        var settings = await _db.SiteSettings
            .Where(s => s.Key == "FacebookInstagramUserId" || s.Key == "InstagramFeedCount" || s.Key == "InstagramFeedEnabled")
            .ToDictionaryAsync(s => s.Key, s => s.Value);

        if (settings.GetValueOrDefault("InstagramFeedEnabled", "false") != "true")
            return new JsonResult(new { error = "feed disabled" }) { StatusCode = 404 };

        var userId = settings.GetValueOrDefault("FacebookInstagramUserId", "");
        if (string.IsNullOrWhiteSpace(userId))
            return new JsonResult(new { error = "Instagram user ID not configured" }) { StatusCode = 503 };

        var count = int.TryParse(settings.GetValueOrDefault("InstagramFeedCount", "9"), out var c) ? c : 9;
        count = Math.Clamp(count, 1, 20);

        try
        {
            var client = _http.CreateClient("meta");
            var url = $"https://graph.instagram.com/{Uri.EscapeDataString(userId)}/media" +
                      $"?fields=id,media_type,thumbnail_url,permalink,media_url,caption" +
                      $"&limit={count}&access_token={Uri.EscapeDataString(token)}";

            var resp = await client.GetStringAsync(url);
            var doc  = JsonDocument.Parse(resp);
            var items = doc.RootElement.TryGetProperty("data", out var data)
                ? data.EnumerateArray().Select(el => new
                {
                    mediaUrl  = el.TryGetProperty("media_url", out var mu)      ? mu.GetString()  : el.TryGetProperty("thumbnail_url", out var tu) ? tu.GetString() : null,
                    permalink = el.TryGetProperty("permalink", out var pl)      ? pl.GetString()  : null,
                    caption   = el.TryGetProperty("caption",   out var cap)     ? cap.GetString() : null,
                    mediaType = el.TryGetProperty("media_type", out var mt)     ? mt.GetString()  : null,
                }).ToList()
                : new();

            // Cache 30 minutes — reduces API calls to Instagram
            Response.Headers["Cache-Control"] = "public, max-age=1800";
            return new JsonResult(items);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("190"))
        {
            return new JsonResult(new { error = "token_expired" }) { StatusCode = 401 };
        }
        catch
        {
            return new JsonResult(new { error = "fetch_failed" }) { StatusCode = 502 };
        }
    }
}
