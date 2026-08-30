using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace glur.cafe.page.Pages.Admin.Messenger;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApplicationDbContext db, IConfiguration config,
        IHttpClientFactory httpFactory, ILogger<IndexModel> logger)
    {
        _db          = db;
        _config      = config;
        _httpFactory = httpFactory;
        _logger      = logger;
    }

    // ── Page state ──────────────────────────────────────────────────────────
    public List<MessengerConversation> Conversations { get; set; } = new();
    public bool WebhookConfigured => !string.IsNullOrEmpty(_config["FacebookSecrets:PageAccessToken"]);

    // ── Bind properties for income modal ───────────────────────────────────
    [BindProperty] public decimal  TxAmount      { get; set; }
    [BindProperty] public string   TxDescription { get; set; } = "";
    [BindProperty] public string   TxCategory    { get; set; } = "ขายสินค้า";
    [BindProperty] public string?  TxNote        { get; set; }
    [BindProperty] public DateTime TxDate        { get; set; } = DateTime.Today;

    // ── OnGet ───────────────────────────────────────────────────────────────
    public async Task OnGetAsync()
    {
        Conversations = await _db.MessengerConversations
            .OrderByDescending(c => c.LastMessageAt)
            .Take(50)
            .ToListAsync();
    }

    // ── AJAX: return conversations as JSON (for polling) ────────────────────
    public async Task<IActionResult> OnGetConversationsAsync()
    {
        var convs = await _db.MessengerConversations
            .OrderByDescending(c => c.LastMessageAt)
            .Take(50)
            .Select(c => new {
                c.Id, c.PsId, c.DisplayName, c.LastMessageText,
                LastMessageAt = c.LastMessageAt.ToString("yyyy-MM-ddTHH:mm:ss"),
                c.UnreadCount
            })
            .ToListAsync();

        return new JsonResult(convs);
    }

    // ── AJAX: return messages for a conversation ────────────────────────────
    public async Task<IActionResult> OnGetMessagesAsync(string psid)
    {
        if (string.IsNullOrWhiteSpace(psid))
            return new JsonResult(Array.Empty<object>());

        // Mark as read
        var conv = await _db.MessengerConversations.FirstOrDefaultAsync(c => c.PsId == psid);
        if (conv != null && conv.UnreadCount > 0)
        {
            conv.UnreadCount = 0;
            await _db.MessengerMessages
                .Where(m => m.ConversationId == conv.Id && !m.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
            await _db.SaveChangesAsync();
        }

        var msgs = await _db.MessengerMessages
            .Where(m => m.Conversation.PsId == psid)
            .OrderBy(m => m.SentAt)
            .Select(m => new {
                m.Id, m.IsFromPage, m.Text,
                SentAt = m.SentAt.ToString("HH:mm")
            })
            .ToListAsync();

        return new JsonResult(msgs);
    }

    // ── AJAX: send a message via Graph API ──────────────────────────────────
    public async Task<IActionResult> OnPostSendAsync()
    {
        var psid = Request.Form["psid"].ToString();
        var text = Request.Form["text"].ToString();

        if (string.IsNullOrWhiteSpace(psid) || string.IsNullOrWhiteSpace(text))
            return new JsonResult(new { ok = false, error = "invalid input" }) { StatusCode = 400 };

        var pageToken = _config["FacebookSecrets:PageAccessToken"] ?? "";
        if (!string.IsNullOrEmpty(pageToken))
        {
            try
            {
                var client  = _httpFactory.CreateClient("meta");
                var payload = new
                {
                    recipient = new { id = psid },
                    message   = new { text }
                };
                var content = new StringContent(
                    JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                content.Headers.Add("Authorization", $"Bearer {pageToken}");

                var resp = await client.PostAsync(
                    "https://graph.facebook.com/v20.0/me/messages", content);

                if (!resp.IsSuccessStatusCode)
                {
                    var err = await resp.Content.ReadAsStringAsync();
                    _logger.LogWarning("Graph API send failed: {Error}", err);
                    return new JsonResult(new { ok = false, error = "Graph API error" }) { StatusCode = 500 };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Messenger message");
                return new JsonResult(new { ok = false, error = ex.Message }) { StatusCode = 500 };
            }
        }

        // Store outbound message in DB
        var conv = await _db.MessengerConversations.FirstOrDefaultAsync(c => c.PsId == psid);
        if (conv == null)
        {
            conv = new MessengerConversation
            {
                PsId          = psid,
                DisplayName   = "ผู้ใช้ Facebook",
                CreatedAt     = DateTime.Now,
                LastMessageAt = DateTime.Now
            };
            _db.MessengerConversations.Add(conv);
            await _db.SaveChangesAsync();
        }

        _db.MessengerMessages.Add(new MessengerMessage
        {
            ConversationId = conv.Id,
            IsFromPage     = true,
            Text           = text,
            SentAt         = DateTime.Now,
            IsRead         = true
        });
        conv.LastMessageText = text;
        conv.LastMessageAt   = DateTime.Now;
        await _db.SaveChangesAsync();

        return new JsonResult(new { ok = true, sentAt = DateTime.Now.ToString("HH:mm") });
    }

    // ── Save income transaction (from modal) ────────────────────────────────
    public async Task<IActionResult> OnPostSaveTransactionAsync()
    {
        if (TxAmount <= 0 || string.IsNullOrWhiteSpace(TxDescription))
        {
            TempData["Error"] = "กรุณากรอกจำนวนเงินและรายละเอียดให้ครบ";
            return RedirectToPage();
        }

        _db.Transactions.Add(new Transaction
        {
            Type        = "income",
            Category    = TxCategory,
            Description = TxDescription,
            Amount      = TxAmount,
            Date        = TxDate,
            Note        = TxNote,
            CreatedAt   = DateTime.Now
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"บันทึกรายรับ {TxAmount:N0} บาท เรียบร้อยแล้ว";
        return RedirectToPage();
    }
}

