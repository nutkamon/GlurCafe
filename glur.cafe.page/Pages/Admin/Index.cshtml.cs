using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public int ServiceCount     { get; set; }
    public int PortfolioCount   { get; set; }
    public int UnreadCount      { get; set; }
    public int TotalMessages    { get; set; }
    public int QuotationCount   { get; set; }
    public int PricingPlanCount { get; set; }
    public List<ContactMessage> RecentMessages { get; set; } = new();

    public async Task OnGetAsync()
    {
        ServiceCount     = await _db.Services.CountAsync();
        PortfolioCount   = await _db.Portfolios.CountAsync();
        UnreadCount      = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        TotalMessages    = await _db.ContactMessages.CountAsync();
        QuotationCount   = await _db.Quotations.CountAsync();
        PricingPlanCount = await _db.PricingPlans.CountAsync();
        RecentMessages   = await _db.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .ToListAsync();
    }
}
