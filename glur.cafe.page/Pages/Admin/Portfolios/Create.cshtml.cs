using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace glur.cafe.page.Pages.Admin.Portfolios;

[Authorize]
[RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public CreateModel(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [BindProperty] public Portfolio Portfolio { get; set; } = new();

    public async Task OnGetAsync()
    {
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        Portfolio.DisplayOrder = (await _db.Portfolios.MaxAsync(p => (int?)p.DisplayOrder) ?? 0) + 1;
    }

    public async Task<IActionResult> OnPostAsync(IFormFile[]? ImageFiles)
    {
        // Default icon if left empty
        if (string.IsNullOrWhiteSpace(Portfolio.IconClassName))
            Portfolio.IconClassName = "bi-cup-hot";

        if (!ModelState.IsValid)
        {
            ModelState.Remove(nameof(Portfolio) + "." + nameof(Portfolio.IconClassName));
            if (!ModelState.IsValid) return Page();
        }

        try
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "img", "portfolios");
            Directory.CreateDirectory(uploadsDir);

            var files = ImageFiles?.Where(f => f.Length > 0).ToList() ?? new();

            if (files.Count == 0)
            {
                Portfolio.CreatedAt = Portfolio.UpdatedAt = DateTime.Now;
                _db.Portfolios.Add(Portfolio);
            }
            else
            {
                int baseOrder = Portfolio.DisplayOrder;
                for (int i = 0; i < files.Count; i++)
                {
                    var fileName = Guid.NewGuid().ToString("N") + ".jpg";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using var inputStream = files[i].OpenReadStream();
                    using var image = await Image.LoadAsync(inputStream);

                    const int maxSize = 1200;
                    if (image.Width > maxSize || image.Height > maxSize)
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(maxSize, maxSize),
                            Mode = ResizeMode.Max
                        }));

                    await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 85 });

                    _db.Portfolios.Add(new Portfolio
                    {
                        ProjectName   = Portfolio.ProjectName,
                        Category      = Portfolio.Category,
                        ClientName    = Portfolio.ClientName,
                        Description   = Portfolio.Description,
                        IconClassName = Portfolio.IconClassName,
                        GradientClass = Portfolio.GradientClass,
                        DisplayOrder  = baseOrder + i,
                        IsActive      = Portfolio.IsActive,
                        ImagePath     = "/img/portfolios/" + fileName,
                        CreatedAt     = DateTime.Now,
                        UpdatedAt     = DateTime.Now,
                    });
                }
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = files.Count > 1
                ? $"เพิ่มรายการแกลเลอรี่เรียบร้อยแล้ว ({files.Count} รายการ)"
                : "เพิ่มรายการแกลเลอรี่เรียบร้อยแล้ว";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"เกิดข้อผิดพลาด: {ex.Message}");
            await OnGetAsync();
            return Page();
        }
    }
}
