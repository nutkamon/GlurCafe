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

    // Filter
    [BindProperty(SupportsGet = true)] public int Year { get; set; } = DateTime.Today.Year;
    [BindProperty(SupportsGet = true)] public int Month { get; set; } = DateTime.Today.Month;
    [BindProperty(SupportsGet = true)] public string? TypeFilter { get; set; }

    // Summary
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance => TotalIncome - TotalExpense;

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
        var all = await _db.Transactions
            .Where(t => t.Date.Year == Year && t.Date.Month == Month)
            .OrderByDescending(t => t.Date).ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        TotalIncome = all.Where(t => t.Type == "income").Sum(t => t.Amount);
        TotalExpense = all.Where(t => t.Type == "expense").Sum(t => t.Amount);

        Transactions = string.IsNullOrEmpty(TypeFilter)
            ? all
            : all.Where(t => t.Type == TypeFilter).ToList();
    }
}
