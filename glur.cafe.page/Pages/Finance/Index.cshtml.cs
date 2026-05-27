using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Finance;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public static readonly HashSet<string> CogsCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "ค่าวัตถุดิบ", "ค่าแรงงาน"
    };

    [BindProperty(SupportsGet = true)] public int Year { get; set; } = DateTime.Today.Year;
    [BindProperty(SupportsGet = true)] public int Month { get; set; } = DateTime.Today.Month;
    [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal TotalOpEx { get; set; }
    public decimal GrossProfit => TotalIncome - TotalCOGS;
    public decimal NetBalance => TotalIncome - TotalExpense;
    public decimal GrossMargin => TotalIncome > 0 ? GrossProfit / TotalIncome * 100 : 0;
    public decimal NetMargin => TotalIncome > 0 ? NetBalance / TotalIncome * 100 : 0;

    public List<(string Category, decimal Amount)> ExpenseByCategory { get; set; } = [];
    public List<MonthSummary> MonthlyTrend { get; set; } = [];
    public record MonthSummary(int Year, int Month, decimal Income, decimal Expense)
    {
        public decimal Profit => Income - Expense;
    }

    public List<SaleOrder> SaleOrders { get; set; } = [];
    public List<Transaction> Expenses { get; set; } = [];

    [BindProperty] public Transaction ExpenseInput { get; set; } = new() { Type = "expense" };

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAddExpenseAsync()
    {
        ExpenseInput.Type = "expense";
        foreach (var key in ModelState.Keys.Where(k => !k.StartsWith("ExpenseInput")).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        ExpenseInput.CreatedAt = DateTime.Now;
        _db.Transactions.Add(ExpenseInput);
        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึกรายจ่ายเรียบร้อยแล้ว";
        return RedirectToPage(new { Year = ExpenseInput.Date.Year, Month = ExpenseInput.Date.Month });
    }

    public async Task<IActionResult> OnPostDeleteExpenseAsync(int id)
    {
        var item = await _db.Transactions.FindAsync(id);
        if (item != null) { _db.Transactions.Remove(item); await _db.SaveChangesAsync(); }
        TempData["Success"] = "ลบรายจ่ายเรียบร้อยแล้ว";
        return RedirectToPage(new { Year, Month, TypeFilter });
    }

    public async Task<IActionResult> OnPostDeleteSaleAsync(int id)
    {
        var sale = await _db.SaleOrders.FindAsync(id);
        if (sale != null) { _db.SaleOrders.Remove(sale); await _db.SaveChangesAsync(); }
        TempData["Success"] = "ลบใบเสร็จเรียบร้อยแล้ว";
        return RedirectToPage(new { Year, Month, TypeFilter });
    }

    private async Task LoadDataAsync()
    {
        var salesAll = await _db.SaleOrders
            .Include(s => s.Items)
            .Where(s => s.Date.Year == Year && s.Date.Month == Month)
            .OrderByDescending(s => s.Date).ThenByDescending(s => s.CreatedAt)
            .ToListAsync();
        TotalIncome = salesAll.Sum(s => s.GrandTotal);

        var expensesAll = await _db.Transactions
            .Where(t => t.Date.Year == Year && t.Date.Month == Month && t.Type == "expense")
            .OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt)
            .ToListAsync();
        TotalExpense = expensesAll.Sum(t => t.Amount);
        TotalCOGS    = expensesAll.Where(t => CogsCategories.Contains(t.Category)).Sum(t => t.Amount);
        TotalOpEx    = TotalExpense - TotalCOGS;

        ExpenseByCategory = expensesAll
            .GroupBy(t => t.Category)
            .Select(g => (g.Key, g.Sum(t => t.Amount)))
            .OrderByDescending(x => x.Item2)
            .ToList();

        SaleOrders = TypeFilter == "expense" ? [] : salesAll;
        Expenses   = TypeFilter == "income"  ? [] : expensesAll;

        // Monthly trend 6 เดือนย้อนหลัง
        var trendStart = new DateTime(Year, Month, 1).AddMonths(-5);
        var trendEnd   = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));

        var trendSales = await _db.SaleOrders
            .Where(s => s.Date >= trendStart && s.Date <= trendEnd)
            .ToListAsync();
        var trendExp = await _db.Transactions
            .Where(t => t.Date >= trendStart && t.Date <= trendEnd && t.Type == "expense")
            .ToListAsync();

        MonthlyTrend = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var d = trendStart.AddMonths(i);
                var inc = trendSales.Where(s => s.Date.Year == d.Year && s.Date.Month == d.Month).Sum(s => s.GrandTotal);
                var exp = trendExp.Where(t => t.Date.Year == d.Year && t.Date.Month == d.Month).Sum(t => t.Amount);
                return new MonthSummary(d.Year, d.Month, inc, exp);
            })
            .ToList();
    }
}
