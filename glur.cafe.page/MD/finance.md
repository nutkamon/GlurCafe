# Finance Module — Template & Knowledge Base

> Template extracted from **glur.cafe.page** (ASP.NET Core 9 Razor Pages + EF Core).  
> Use this document to recreate an equivalent Finance module in any future project.

---

## Overview

The Finance module provides:
- **Dashboard** — Summary cards (Income, COGS, Gross Profit, OpEx, Net Profit) + P&L statement + expense-breakdown pie chart + 6-month trend chart
- **Sale Entry** — Create receipts (`SaleOrder`) with line items, auto-generated order numbers, product price lookup
- **Print Receipt** — Standalone print-friendly receipt page (no shared layout)
- **Expense Entry** — Inline quick-add form on the dashboard
- **Filtering** — Filter by year/month and by type (income / expense)
- **Product Cost Calculator (CostCalc)** — CRUD for products-for-sale with ingredient blending, loss rate, processing fee, and auto-calculated cost/profit per kg

---

## Folder Structure

```
Pages/
├── Finance/
│   ├── Index.cshtml          # Dashboard: summary + charts + transaction lists
│   ├── Index.cshtml.cs
│   ├── Sale.cshtml           # Sale order entry form
│   ├── Sale.cshtml.cs
│   ├── SalePrint.cshtml      # Print receipt (Layout = null)
│   └── SalePrint.cshtml.cs
└── Admin/
    └── CostCalc/
        ├── Index.cshtml      # List all products + delete [Authorize]
        ├── Index.cshtml.cs
        ├── Create.cshtml     # New product cost formula [Authorize]
        ├── Create.cshtml.cs
        ├── Edit.cshtml       # Edit existing product formula [Authorize]
        ├── Edit.cshtml.cs
        ├── Print.cshtml      # Print cost sheet (Layout = null) [Authorize]
        └── Print.cshtml.cs
Pages/Shared/
└── _FinanceLayout.cshtml     # Dedicated layout for Finance pages
Models/
├── Transaction.cs            # Expense record
├── SaleOrder.cs              # Sale receipt header
├── SaleOrderItem.cs          # Sale receipt line item
├── ProductCost.cs            # Product formula header
├── ProductCostItem.cs        # Ingredient line in a formula
└── BeanType.cs               # Ingredient master (lookup table)
```

---

## Data Models

### `Transaction` (Expense record)

```csharp
public class Transaction
{
    public int Id { get; set; }

    [Required, StringLength(10)]
    public string Type { get; set; } = "income"; // always "expense" for cost entries

    [Required, StringLength(50)]
    public string Category { get; set; } = "";   // e.g. "ค่าวัตถุดิบ", "ค่าเช่า"

    [Required, StringLength(200)]
    public string Description { get; set; } = "";

    [Required, Column(TypeName = "decimal(12,2)")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
```

### `SaleOrder` (Receipt header)

```csharp
public class SaleOrder
{
    public int Id { get; set; }

    [StringLength(30)]
    public string OrderNumber { get; set; } = "";  // e.g. "SO-20260612-001"

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    public decimal ItemsTotal  { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal GrandTotal  { get; set; }       // ItemsTotal + DeliveryFee

    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<SaleOrderItem> Items { get; set; } = new();
}
```

### `SaleOrderItem` (Receipt line)

```csharp
public class SaleOrderItem
{
    public int Id { get; set; }
    public int SaleOrderId { get; set; }

    [StringLength(200)]
    public string Description { get; set; } = "";

    public decimal Quantity  { get; set; } = 1;

    [StringLength(20)]
    public string Unit { get; set; } = "กก.";

    public decimal UnitPrice { get; set; }
    public decimal Amount    { get; set; }   // Quantity × UnitPrice

    public SaleOrder? SaleOrder { get; set; }
}
```

### DbContext registrations required

```csharp
public DbSet<Transaction>    Transactions { get; set; }
public DbSet<SaleOrder>      SaleOrders   { get; set; }
public DbSet<SaleOrderItem>  SaleOrderItems { get; set; }
```

---

## Layout — `_FinanceLayout.cshtml`

A minimal standalone layout (no sidebar) with a sticky top bar, Bootstrap 5, Bootstrap Icons, and Chart.js.

**Key CSS classes defined:**
| Class | Purpose |
|---|---|
| `.text-teal` | Brand green `#00a86b` |
| `.btn-teal` | Solid brand-green button |
| `.btn-outline-teal` | Outlined brand-green button |
| `.topbar` | Sticky nav bar |
| `.page-content` | Main content padding wrapper |

**CDN dependencies:**
```html
<!-- Fonts -->
<link href="https://fonts.googleapis.com/css2?family=Noto+Sans+Thai:wght@300;400;500;600;700&display=swap" rel="stylesheet" />
<!-- Bootstrap 5.3 -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
<!-- JS -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
```

Chart.js is loaded only in the pages that need it (`@section Scripts`):
```html
<script src="https://cdn.jsdelivr.net/npm/chart.js@4"></script>
```

---

## Page 1 — `Finance/Index` (Dashboard)

### PageModel: `IndexModel`

**Route parameters (SupportsGet):**
| Property | Type | Default | Purpose |
|---|---|---|---|
| `Year` | `int` | `DateTime.Today.Year` | Filter year |
| `Month` | `int` | `DateTime.Today.Month` | Filter month |
| `TypeFilter` | `string?` | `null` | `"income"` / `"expense"` / `null` (all) |

**Computed properties:**
```csharp
// COGS categories that separate raw costs from operating expenses
public static readonly HashSet<string> CogsCategories = new(StringComparer.OrdinalIgnoreCase)
{
    "ค่าวัตถุดิบ", "ค่าแรงงาน"  // adapt to your domain
};

public decimal TotalIncome  { get; set; }
public decimal TotalExpense { get; set; }
public decimal TotalCOGS    { get; set; }   // subset of Expense
public decimal TotalOpEx    { get; set; }   // TotalExpense - TotalCOGS
public decimal GrossProfit  => TotalIncome - TotalCOGS;
public decimal NetBalance   => TotalIncome - TotalExpense;
public decimal GrossMargin  => TotalIncome > 0 ? GrossProfit / TotalIncome * 100 : 0;
public decimal NetMargin    => TotalIncome > 0 ? NetBalance  / TotalIncome * 100 : 0;
```

**Collections:**
```csharp
public List<(string Category, decimal Amount)> ExpenseByCategory { get; set; } = [];
public List<MonthSummary> MonthlyTrend { get; set; } = [];
public record MonthSummary(int Year, int Month, decimal Income, decimal Expense)
{
    public decimal Profit => Income - Expense;
}
public List<SaleOrder>    SaleOrders  { get; set; } = [];
public List<Transaction>  Expenses    { get; set; } = [];
```

**Handlers:**
| Handler | Method | Action |
|---|---|---|
| `OnGetAsync` | GET | Load all data |
| `OnPostAddExpenseAsync` | POST | Add a new expense |
| `OnPostDeleteExpenseAsync(int id)` | POST | Delete an expense |
| `OnPostDeleteSaleAsync(int id)` | POST | Delete a sale order |

**`LoadDataAsync` logic summary:**
1. Query `SaleOrders` for the selected month → `TotalIncome`
2. Query `Transactions` (type = "expense") for the month → split into COGS vs OpEx
3. Group expenses by `Category` → `ExpenseByCategory`
4. Apply `TypeFilter` to control which list is displayed
5. Fetch 6-month rolling window for `MonthlyTrend` (starting 5 months before selected month)

### View: `Index.cshtml`

**Sections:**
1. **Alert** — `TempData["Success"]` dismissible Bootstrap alert
2. **Summary Cards** — 5 cards: Income / COGS / Gross Profit / OpEx / Net Profit
3. **P&L Statement** — Left column card listing each line with color-coded amounts
4. **Expense Pie Chart** — Right column: doughnut chart + legend with progress bars
5. **6-Month Trend Chart** — Grouped bar (income/expense) + line (net profit) via Chart.js
6. **Filter Bar** — Year/Month/Type dropdowns + "บันทึกการขาย" and "เพิ่มรายจ่าย" buttons
7. **Expense Quick-Add Form** — Bootstrap Collapse panel with inline form
8. **Income Table** — Collapsible rows showing SaleOrder details
9. **Expense Table** — Simple table with delete button per row

**Thai month helpers (used in view):**
```csharp
var monthsShort = new[] { "ม.ค.","ก.พ.","มี.ค.","เม.ย.","พ.ค.","มิ.ย.",
                          "ก.ค.","ส.ค.","ก.ย.","ต.ค.","พ.ย.","ธ.ค." };
var months = new[] { "มกราคม","กุมภาพันธ์","มีนาคม","เมษายน","พฤษภาคม","มิถุนายน",
                     "กรกฎาคม","สิงหาคม","กันยายน","ตุลาคม","พฤศจิกายน","ธันวาคม" };
```

**Expense category datalist (adapt to domain):**
```
ค่าวัตถุดิบ, ค่าแรงงาน, ค่าเช่า, ค่าสาธารณูปโภค, ค่าขนส่ง, ค่าการตลาด, ค่าอุปกรณ์, ค่าใช้จ่ายอื่นๆ
```

**Chart.js configuration pattern:**
```javascript
// Trend chart — mixed bar+line
new Chart(ctx, {
    data: {
        labels: /* string[] from server */,
        datasets: [
            { type: 'bar',  label: 'รายรับ',   data: incomeArray,  backgroundColor: 'rgba(34,197,94,0.75)' },
            { type: 'bar',  label: 'รายจ่าย',  data: expenseArray, backgroundColor: 'rgba(239,68,68,0.75)' },
            { type: 'line', label: 'กำไรสุทธิ', data: profitArray,  borderColor: '#00a86b', fill: true }
        ]
    },
    options: {
        interaction: { mode: 'index', intersect: false },
        scales: { y: { beginAtZero: true } }
    }
});
```

---

## Page 2 — `Finance/Sale` (Sale Entry)

### PageModel: `SaleModel`

**Bound properties:**
```csharp
[BindProperty] public SaleOrder       Order { get; set; } = new();
[BindProperty] public List<SaleItemInput> Items { get; set; } = [];

public class SaleItemInput
{
    public string  Description { get; set; } = "";
    public decimal Quantity    { get; set; } = 1;
    public string  Unit        { get; set; } = "กก.";
    public decimal UnitPrice   { get; set; }
}

// Product dropdown data
public List<(int Id, string Name, decimal Price)> ProductOptions { get; set; } = [];
```

**Order number generation:**
```csharp
private async Task<string> GenerateOrderNumberAsync()
{
    string prefix = $"SO-{DateTime.Now:yyyyMMdd}-";
    int count = await _db.SaleOrders.CountAsync(s => s.OrderNumber.StartsWith(prefix));
    return $"{prefix}{(count + 1):D3}";
}
```
> Pattern: `SO-YYYYMMDD-NNN` — change `SO` to match your domain.

**POST handler steps:**
1. Remove non-`Items`/`Order` ModelState keys to avoid false validation errors
2. Filter `Items` where `Description` is not empty
3. Regenerate `OrderNumber` to avoid race conditions
4. Calculate `Amount = Quantity × UnitPrice` per item
5. Set `ItemsTotal`, `GrandTotal`, `CreatedAt`
6. Save and redirect to `Index` with the receipt's year/month

### View: `Sale.cshtml`

**Key UI components:**
- Header card: Date / OrderNumber (readonly) / Note
- Items table: populated entirely by JavaScript; uses `<template>` tag for row cloning
- Totals card: live-updating ItemsTotal + DeliveryFee = GrandTotal
- Product dropdown fills description and price automatically

**JavaScript pattern for dynamic table rows:**
```javascript
let rowIndex = 0;

function addRow() {
    const clone = document.getElementById('rowTemplate').content.cloneNode(true);
    const tr = clone.querySelector('tr');

    // Bind model names for server-side binding
    tr.querySelector('.desc-input').name  = `Items[${rowIndex}].Description`;
    tr.querySelector('.qty-input').name   = `Items[${rowIndex}].Quantity`;
    tr.querySelector('.unit-input').name  = `Items[${rowIndex}].Unit`;
    tr.querySelector('.price-input').name = `Items[${rowIndex}].UnitPrice`;

    // Auto-fill from product dropdown
    tr.querySelector('.product-select').addEventListener('change', function () {
        const opt = this.options[this.selectedIndex];
        if (opt.value) {
            tr.querySelector('.desc-input').value  = opt.dataset.name;
            tr.querySelector('.price-input').value = parseFloat(opt.dataset.price).toFixed(2);
            calcRowTotal(tr);
        }
    });

    tr.querySelector('.qty-input').addEventListener('input',   () => calcRowTotal(tr));
    tr.querySelector('.price-input').addEventListener('input', () => calcRowTotal(tr));

    document.getElementById('itemsBody').appendChild(clone);
    rowIndex++;
    renumber();
}

function renumber() {
    document.querySelectorAll('#itemsBody .item-row').forEach((tr, i) => {
        tr.querySelector('.row-num').textContent = i + 1;
        tr.querySelector('.desc-input').name  = `Items[${i}].Description`;
        tr.querySelector('.qty-input').name   = `Items[${i}].Quantity`;
        tr.querySelector('.unit-input').name  = `Items[${i}].Unit`;
        tr.querySelector('.price-input').name = `Items[${i}].UnitPrice`;
    });
}
```
> **Important:** `renumber()` must be called after every add/remove to keep sequential indexes for model binding.

---

## Page 3 — `Finance/SalePrint` (Print Receipt)

### PageModel: `SalePrintModel`

```csharp
// Route: /Finance/SalePrint/{id:int}
public SaleOrder Order { get; set; } = null!;
public Dictionary<string, string> Site { get; set; } = new(); // from SiteSettings table

public async Task<IActionResult> OnGetAsync(int id)
{
    var order = await _db.SaleOrders.Include(s => s.Items).FirstOrDefaultAsync(s => s.Id == id);
    if (order == null) return NotFound();
    Order = order;
    Site = await _db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
    return Page();
}
```

> If the new project has no `SiteSettings` table, replace with hardcoded values or `appsettings.json`.

### View: `SalePrint.cshtml`

- `Layout = null` — completely standalone HTML page
- Uses CSS `@media print` to hide toolbar and adjust page margins
- Brand information pulled from `Site` dictionary:
  - `SiteName`, `Phone`, `Email`, `LineId`, `Address`
- Opens in new tab from the Index page (`target="_blank"`)

---

## Adapting to a New Project

### Step-by-step checklist

- [ ] **1. Copy models** — `Transaction`, `SaleOrder`, `SaleOrderItem`
  - Adjust `Column(TypeName)` if using a different DB provider
  - Rename `Unit` default value if items aren't measured in `กก.`

- [ ] **2. Register in DbContext**
  ```csharp
  public DbSet<Transaction>   Transactions  { get; set; }
  public DbSet<SaleOrder>     SaleOrders    { get; set; }
  public DbSet<SaleOrderItem> SaleOrderItems { get; set; }
  ```

- [ ] **3. Add & run migration**
  ```bash
  dotnet ef migrations add AddFinanceTables
  dotnet ef database update
  ```

- [ ] **4. Copy `_FinanceLayout.cshtml`** — Update brand name, logo path, and nav links

- [ ] **5. Copy Finance pages** — Update `namespace` and `using` directives to match new project

- [ ] **6. Update `CogsCategories`** in `IndexModel` to match the new domain's cost structure

- [ ] **7. Update expense category datalist** in `Index.cshtml` to match the new domain

- [ ] **8. Update OrderNumber prefix** in `GenerateOrderNumberAsync` (e.g., change `SO-` to `INV-`)

- [ ] **9. Handle `SiteSettings`** in `SalePrintModel` — replace with project-specific config source if needed

- [ ] **10. (Optional) Remove `ProductOptions`** from `SaleModel` if no product catalog exists; remove the dropdown from the row template and use free-text description only

### Namespace template

```csharp
// Change "glur.cafe.page" → "YourProject.Namespace"
namespace YourProject.Namespace.Pages.Finance;
```

---

## Stock Module (Template — not yet in glur.cafe.page)

> This section is a **design template** for integrating stock tracking into the Finance module.  
> Stock is deducted automatically when a `SaleOrder` is saved, and can be adjusted manually.

---

### Concept

```
ProductCost (product catalog)
    └── StockLot (stock entries — each represents an add or deduct movement)
            ▲                ▲
     manual top-up     auto-deduct on SaleOrder save
```

Each row in `StockLot` is an **immutable movement** (+ or −). Current stock = `SUM(Quantity)` per product.  
This gives a full audit trail without ever updating a single "quantity" column.

---

### New Models

#### `StockLot` (movement ledger)

```csharp
public class StockLot
{
    public int Id { get; set; }

    // Link to the product (references ProductCost.Id)
    public int ProductCostId { get; set; }
    public ProductCost? Product { get; set; }

    // Positive = stock in, Negative = stock out
    [Column(TypeName = "decimal(12,3)")]
    public decimal Quantity { get; set; }   // e.g. +10.000 (add) or -0.500 (sold)

    [StringLength(20)]
    public string Unit { get; set; } = "กก.";

    // "sale" | "manual_in" | "manual_out" | "adjustment"
    [StringLength(20)]
    public string MovementType { get; set; } = "manual_in";

    // FK to SaleOrder when MovementType == "sale" (nullable)
    public int? SaleOrderId { get; set; }
    public SaleOrder? SaleOrder { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
```

#### Add to DbContext

```csharp
public DbSet<StockLot> StockLots { get; set; }
```

#### Add navigation to `ProductCost`

```csharp
// Inside ProductCost model
public List<StockLot> StockLots { get; set; } = new();

// Computed (not mapped) — use for display only
[NotMapped]
public decimal CurrentStock => StockLots.Sum(s => s.Quantity);
```

> **Note:** `CurrentStock` is only populated when `StockLots` is `.Include()`-d. For queries, use `SUM` in the DB query instead.

---

### Auto-Deduct on Sale

In `SaleModel.OnPostAsync`, after saving the order, loop through line items and write a negative `StockLot` per product:

```csharp
public async Task<IActionResult> OnPostAsync()
{
    // ... existing validation & save logic ...

    _db.SaleOrders.Add(Order);
    await _db.SaveChangesAsync(); // saves Order and gets its Id

    // Auto-deduct stock for each sold item
    foreach (var item in Order.Items)
    {
        // Match sold item description to a ProductCost by name (or add ProductCostId to SaleOrderItem)
        var product = await _db.ProductCosts
            .FirstOrDefaultAsync(p => p.ProductName == item.Description);

        if (product != null)
        {
            _db.StockLots.Add(new StockLot
            {
                ProductCostId = product.Id,
                Quantity      = -item.Quantity,   // negative = deduct
                Unit          = item.Unit,
                MovementType  = "sale",
                SaleOrderId   = Order.Id,
                Note          = $"ขาย {Order.OrderNumber}",
                Date          = Order.Date,
                CreatedAt     = DateTime.Now
            });
        }
    }
    await _db.SaveChangesAsync();

    TempData["Success"] = $"บันทึกใบเสร็จ {Order.OrderNumber} เรียบร้อย";
    return RedirectToPage("Index", new { Year = Order.Date.Year, Month = Order.Date.Month });
}
```

> **Better approach:** Add `public int? ProductCostId { get; set; }` to `SaleOrderItem` and populate it from the product dropdown. This avoids fragile name-matching and works even when names change.

#### Improved `SaleOrderItem` (recommended)

```csharp
public class SaleOrderItem
{
    // ... existing fields ...
    public int? ProductCostId { get; set; }   // ADD THIS
    public ProductCost? Product { get; set; } // ADD THIS
}
```

Then in `SaleModel`, the dropdown already passes `ProductCostId` via the hidden input, so the deduct loop becomes:

```csharp
foreach (var item in Order.Items.Where(i => i.ProductCostId.HasValue))
{
    _db.StockLots.Add(new StockLot
    {
        ProductCostId = item.ProductCostId!.Value,
        Quantity      = -item.Quantity,
        Unit          = item.Unit,
        MovementType  = "sale",
        SaleOrderId   = Order.Id,
        Note          = $"ขาย {Order.OrderNumber}",
        Date          = Order.Date
    });
}
```

---

### Stock Management Pages

#### Folder structure

```
Pages/Admin/Stock/
├── Index.cshtml       # Stock levels table + manual adjustment form
└── Index.cshtml.cs
```

#### `StockIndexModel`

```csharp
[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    // Current stock per product (from DB aggregation)
    public List<StockLevel> StockLevels { get; set; } = [];
    public record StockLevel(int ProductId, string ProductName, decimal CurrentQty, string Unit, decimal LowAlert);

    [BindProperty] public StockAdjustInput Adjust { get; set; } = new();

    public class StockAdjustInput
    {
        public int    ProductCostId { get; set; }
        public decimal Quantity     { get; set; }
        public string  Unit         { get; set; } = "กก.";
        public string  MovementType { get; set; } = "manual_in";  // "manual_in" | "manual_out" | "adjustment"
        public string? Note         { get; set; }
        public DateTime Date        { get; set; } = DateTime.Today;
    }

    public async Task OnGetAsync()
    {
        // Aggregate current stock per product
        StockLevels = await _db.ProductCosts
            .Select(p => new StockLevel(
                p.Id,
                p.ProductName,
                _db.StockLots.Where(s => s.ProductCostId == p.Id).Sum(s => (decimal?)s.Quantity) ?? 0,
                "กก.",
                0   // set your low-stock threshold here or add a column to ProductCost
            ))
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAdjustAsync()
    {
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }

        _db.StockLots.Add(new StockLot
        {
            ProductCostId = Adjust.ProductCostId,
            Quantity      = Adjust.MovementType == "manual_out" ? -Math.Abs(Adjust.Quantity) : Math.Abs(Adjust.Quantity),
            Unit          = Adjust.Unit,
            MovementType  = Adjust.MovementType,
            Note          = Adjust.Note,
            Date          = Adjust.Date,
            CreatedAt     = DateTime.Now
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึกการเคลื่อนไหวสต็อกเรียบร้อย";
        return RedirectToPage();
    }
}
```

#### View: `Stock/Index.cshtml` — key sections

```html
<!-- Stock levels table -->
<table class="table align-middle">
    <thead>
        <tr>
            <th>สินค้า</th>
            <th class="text-end">คงเหลือ</th>
            <th>หน่วย</th>
            <th>สถานะ</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var s in Model.StockLevels)
        {
            var low = s.CurrentQty <= s.LowAlert;
            <tr class="@(low ? "table-warning" : "")">
                <td>@s.ProductName</td>
                <td class="text-end fw-bold @(s.CurrentQty < 0 ? "text-danger" : "text-success")">
                    @s.CurrentQty.ToString("0.###")
                </td>
                <td>@s.Unit</td>
                <td>
                    @if (s.CurrentQty < 0)
                    { <span class="badge bg-danger">ติดลบ</span> }
                    else if (low)
                    { <span class="badge bg-warning text-dark">ใกล้หมด</span> }
                    else
                    { <span class="badge bg-success">ปกติ</span> }
                </td>
            </tr>
        }
    </tbody>
</table>

<!-- Manual adjustment collapse form -->
<button class="btn btn-sm btn-outline-secondary" data-bs-toggle="collapse" data-bs-target="#adjustForm">
    <i class="bi bi-sliders me-1"></i>ปรับสต็อก
</button>
<div class="collapse" id="adjustForm">
    <form method="post" asp-page-handler="Adjust">
        @Html.AntiForgeryToken()
        <!-- ProductCostId dropdown, Quantity, MovementType radio/select, Note, Date, Submit -->
    </form>
</div>
```

---

### Undo Sale — Remove Stock Movement

When a `SaleOrder` is deleted, also delete its `StockLot` entries:

```csharp
// In Finance/Index.cshtml.cs — OnPostDeleteSaleAsync
public async Task<IActionResult> OnPostDeleteSaleAsync(int id)
{
    var sale = await _db.SaleOrders.FindAsync(id);
    if (sale != null)
    {
        // Remove associated stock movements
        var stockMoves = await _db.StockLots.Where(s => s.SaleOrderId == id).ToListAsync();
        _db.StockLots.RemoveRange(stockMoves);

        _db.SaleOrders.Remove(sale);
        await _db.SaveChangesAsync();
    }
    TempData["Success"] = "ลบใบเสร็จและคืนสต็อกเรียบร้อยแล้ว";
    return RedirectToPage(new { Year, Month, TypeFilter });
}
```

---

### Migration

```bash
dotnet ef migrations add AddStockLot
dotnet ef database update
```

---

### Step-by-step checklist for Stock

- [ ] Add `StockLot` model and DbContext registration
- [ ] (Recommended) Add `ProductCostId` FK to `SaleOrderItem`
- [ ] Add migration and update DB
- [ ] Edit `SaleModel.OnPostAsync` to write negative `StockLot` entries after saving
- [ ] Edit `IndexModel.OnPostDeleteSaleAsync` to remove `StockLot` rows when a sale is deleted
- [ ] Create `Pages/Admin/Stock/Index` page for viewing levels and manual adjustments
- [ ] Add a nav link to the Stock page in `_FinanceLayout.cshtml` or Admin layout
- [ ] (Optional) Add a `LowAlertQty` column to `ProductCost` for threshold warnings

---

## Key Patterns Summary

| Pattern | Where | Detail |
|---|---|---|
| COGS vs OpEx split | `IndexModel` | `HashSet<string>` of category names drives the split |
| Auto order number | `SaleModel` | Prefix + date + daily counter padded to 3 digits |
| Dynamic table rows | `Sale.cshtml` JS | `<template>` cloning + sequential name re-indexing |
| Mixed chart | `Index.cshtml` JS | Chart.js v4 mixed `bar` + `line` dataset types |
| Standalone print | `SalePrint.cshtml` | `Layout = null` + `@media print` CSS |
| TempData feedback | All POST handlers | `TempData["Success"]` shown as dismissible alert on next GET |
| Month filter state | `Index.cshtml` | Year/Month passed as hidden inputs in delete forms and as query params on redirect |
| Cost formula calc | `CostCalc/Create` | JS live-recalc: loss multiplier `1/(1−loss%)` × blended bean cost + processing fee |
| Edit replaces items | `CostCalc/Edit` | `RemoveRange` old items then re-add — simpler than diffing |
| Stock ledger | `StockLot` | Immutable ± movements; current stock = `SUM(Quantity)` — full audit trail |
| Auto stock deduct | `SaleModel.OnPostAsync` | Write negative `StockLot` per sold item immediately after `SaveChanges` |
| Undo stock deduct | `OnPostDeleteSaleAsync` | `RemoveRange` all `StockLot` where `SaleOrderId == id` before deleting the order |
