using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Finance;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    // หมวดหมู่ที่ถือว่าเป็น "ต้นทุนสินค้า" (COGS)
    public static readonly HashSet<string> CogsCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "ค่าวัตถุดิบ", "ค่าแรงงาน"
    };

    // Filter
    [BindProperty(SupportsGet = true)] public int Year { get; set; } = DateTime.Today.Year;
    [BindProperty(SupportsGet = true)] public int Month { get; set; } = DateTime.Today.Month;
    [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }

    // Summary (เดือนที่เลือก)
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal TotalOpEx { get; set; }
    public decimal GrossProfit => TotalIncome - TotalCOGS;
    public decimal NetBalance => TotalIncome - TotalExpense;
    public decimal GrossMargin => TotalIncome > 0 ? GrossProfit / TotalIncome * 100 : 0;
    public decimal NetMargin => TotalIncome > 0 ? NetBalance / TotalIncome * 100 : 0;

    // Expense breakdown by category
    public List<(string Category, decimal Amount)> ExpenseByCategory { get; set; } = [];

    // Monthly trend (6 เดือนย้อนหลัง)
    public List<MonthSummary> MonthlyTrend { get; set; } = [];
    public record MonthSummary(int Year, int Month, decimal Income, decimal Expense)
    {
        public decimal Profit => Income - Expense;
    }

    public List<Transaction> Transactions { get; set; } = [];

    // Form binding
    [BindProperty] public Transaction Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        ViewData["ActiveMenu"] = "finance";
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);

        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!ModelState.IsValid)
        {
            ViewData["ActiveMenu"] = "finance";
            ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
            await LoadDataAsync();
            return Page();
        }

        Input.CreatedAt = DateTime.Now;
        _db.Transactions.Add(Input);
        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึกรายการเรียบร้อยแล้ว";
        return RedirectToPage(new { Year = Input.Date.Year, Month = Input.Date.Month });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await _db.Transactions.FindAsync(id);
        if (item != null)
        {
            _db.Transactions.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ลบรายการเรียบร้อยแล้ว";
        }
        return RedirectToPage(new { Year, Month, TypeFilter });
    }

    private async Task LoadDataAsync()
    {
        // โหลดข้อมูลเดือนปัจจุบัน
        var all = await _db.Transactions
            .Where(t => t.Date.Year == Year && t.Date.Month == Month)
            .OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        TotalIncome  = all.Where(t => t.Type == "income").Sum(t => t.Amount);
        TotalExpense = all.Where(t => t.Type == "expense").Sum(t => t.Amount);
        TotalCOGS    = all.Where(t => t.Type == "expense" && CogsCategories.Contains(t.Category)).Sum(t => t.Amount);
        TotalOpEx    = TotalExpense - TotalCOGS;

        ExpenseByCategory = all
            .Where(t => t.Type == "expense")
            .GroupBy(t => t.Category)
            .Select(g => (g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Item2)
            .ToList();

        Transactions = string.IsNullOrEmpty(TypeFilter)
            ? all
            : all.Where(t => t.Type == TypeFilter).ToList();

        // โหลด trend 6 เดือนย้อนหลัง
        var trendStart = new DateTime(Year, Month, 1).AddMonths(-5);
        var trendEnd   = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
        var trendRaw   = await _db.Transactions
            .Where(t => t.Date >= trendStart && t.Date <= trendEnd)
            .ToListAsync();

        MonthlyTrend = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var d = trendStart.AddMonths(i);
                var items = trendRaw.Where(t => t.Date.Year == d.Year && t.Date.Month == d.Month).ToList();
                return new MonthSummary(
                    d.Year, d.Month,
                    items.Where(t => t.Type == "income").Sum(t => t.Amount),
                    items.Where(t => t.Type == "expense").Sum(t => t.Amount));
            })
            .ToList();
    }
}
