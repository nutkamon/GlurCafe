using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Analytics;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    // Summary
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
    public int TotalServices { get; set; }
    public int TotalPortfolios { get; set; }
    public int TotalPricingPlans { get; set; }
    public int TotalQuotations { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalAdminUsers { get; set; }

    // Chart data
    public List<ChartDataPoint> MessagesByDay { get; set; } = new();
    public List<ChartDataPoint> MessagesByServiceType { get; set; } = new();
    public List<ChartDataPoint> QuotationsByStatus { get; set; } = new();
    public List<ChartDataPoint> RevenueByMonth { get; set; } = new();

    // Recent
    public List<ContactMessage> RecentMessages { get; set; } = new();
    public List<Quotation> RecentQuotations { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalMessages  = await _db.ContactMessages.CountAsync();
        UnreadMessages = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        TotalServices  = await _db.Services.CountAsync(s => s.IsActive);
        TotalPortfolios = await _db.Portfolios.CountAsync(p => p.IsActive);
        TotalPricingPlans = await _db.PricingPlans.CountAsync(p => p.IsActive);
        TotalQuotations = await _db.Quotations.CountAsync();
        TotalRevenue    = (decimal)(await _db.Quotations.Where(q => q.PaymentStatus == "paid").SumAsync(q => (double)q.PaidAmount));
        TotalAdminUsers = await _db.AdminUsers.CountAsync(u => u.IsActive);

        // Messages per day (last 30 days)
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);
        var messages = await _db.ContactMessages.Where(m => m.CreatedAt >= thirtyDaysAgo).ToListAsync();
        MessagesByDay = Enumerable.Range(0, 30)
            .Select(i => DateTime.Now.Date.AddDays(-29 + i))
            .Select(date => new ChartDataPoint
            {
                Label = date.ToString("dd/MM"),
                Value = messages.Count(m => m.CreatedAt.Date == date)
            }).ToList();

        // Messages by service type
        MessagesByServiceType = (await _db.ContactMessages.ToListAsync())
            .Where(m => !string.IsNullOrWhiteSpace(m.ServiceType))
            .GroupBy(m => m.ServiceType!)
            .Select(g => new ChartDataPoint { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value).Take(8).ToList();

        // Quotations by status
        QuotationsByStatus = (await _db.Quotations.ToListAsync())
            .GroupBy(q => q.Status)
            .Select(g => new ChartDataPoint { Label = GetStatusLabel(g.Key), Value = g.Count() })
            .ToList();

        // Revenue by month (last 6 months)
        var sixMonthsAgo = DateTime.Now.AddMonths(-6);
        var quotations = await _db.Quotations
            .Where(q => q.PaymentStatus == "paid" && q.PaidAt >= sixMonthsAgo).ToListAsync();
        RevenueByMonth = Enumerable.Range(0, 6)
            .Select(i => DateTime.Now.AddMonths(-5 + i))
            .Select(date => new ChartDataPoint
            {
                Label = date.ToString("MMM yyyy"),
                Value = (int)quotations
                    .Where(q => q.PaidAt.HasValue && q.PaidAt.Value.Year == date.Year && q.PaidAt.Value.Month == date.Month)
                    .Sum(q => q.PaidAmount)
            }).ToList();

        RecentMessages   = await _db.ContactMessages.OrderByDescending(m => m.CreatedAt).Take(5).ToListAsync();
        RecentQuotations = await _db.Quotations.OrderByDescending(q => q.CreatedAt).Take(5).ToListAsync();

        ViewData["UnreadCount"] = UnreadMessages;
    }

    private static string GetStatusLabel(string status) => status switch
    {
        "draft" => "ร่าง", "sent" => "ส่งแล้ว", "accepted" => "ตอบรับ",
        "rejected" => "ปฏิเสธ", "paid" => "ชำระแล้ว",
        "completed" => "เสร็จสิ้น", "cancelled" => "ยกเลิก", _ => status
    };

    public class ChartDataPoint
    {
        public string Label { get; set; } = "";
        public int Value { get; set; }
    }
}
