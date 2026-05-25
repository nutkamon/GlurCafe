using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Finance;

public class SaleModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public SaleModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public SaleOrder Order { get; set; } = new();
    [BindProperty] public List<SaleItemInput> Items { get; set; } = [];

    public List<(int Id, string Name, decimal Price)> ProductOptions { get; set; } = [];

    public class SaleItemInput
    {
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; } = 1;
        public string Unit { get; set; } = "กก.";
        public decimal UnitPrice { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadProductsAsync();
        Order.Date = DateTime.Today;
        Order.OrderNumber = await GenerateOrderNumberAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Order.Items");
        ModelState.Remove("Order.OrderNumber");

        if (!ModelState.IsValid)
        {
            await LoadProductsAsync();
            return Page();
        }

        var validItems = Items.Where(i => !string.IsNullOrWhiteSpace(i.Description)).ToList();
        if (!validItems.Any())
        {
            ModelState.AddModelError("", "กรุณาเพิ่มรายการอย่างน้อย 1 รายการ");
            await LoadProductsAsync();
            return Page();
        }

        Order.OrderNumber = await GenerateOrderNumberAsync();
        Order.Items = validItems.Select(i => new SaleOrderItem
        {
            Description = i.Description,
            Quantity = i.Quantity,
            Unit = i.Unit,
            UnitPrice = i.UnitPrice,
            Amount = i.Quantity * i.UnitPrice
        }).ToList();

        Order.ItemsTotal = Order.Items.Sum(i => i.Amount);
        Order.GrandTotal = Order.ItemsTotal + Order.DeliveryFee;
        Order.CreatedAt = DateTime.Now;

        _db.SaleOrders.Add(Order);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"บันทึกใบเสร็จ {Order.OrderNumber} เรียบร้อย ยอดรวม {Order.GrandTotal:#,##0.00} ฿";
        return RedirectToPage("Index", new { Year = Order.Date.Year, Month = Order.Date.Month });
    }

    private async Task LoadProductsAsync()
    {
        ProductOptions = await _db.ProductCosts
            .OrderBy(p => p.ProductName)
            .Select(p => new { p.Id, p.ProductName, p.SellingPrice })
            .ToListAsync()
            .ContinueWith(t => t.Result.Select(p => (p.Id, p.ProductName, p.SellingPrice)).ToList());
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        string prefix = $"SO-{DateTime.Now:yyyyMMdd}-";
        int count = await _db.SaleOrders.CountAsync(s => s.OrderNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D3}";
    }
}
