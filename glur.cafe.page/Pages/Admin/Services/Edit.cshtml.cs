using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace glur.cafe.page.Pages.Admin.Services;

[Authorize]
[RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public EditModel(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [BindProperty] public Service Service { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await _db.Services.FindAsync(id);
        if (item == null) return NotFound();
        Service = item;
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(IFormFile? ImageFile, bool RemoveImage = false)
    {
        if (string.IsNullOrWhiteSpace(Service.IconClassName))
            Service.IconClassName = "bi-cup-hot";

        if (!ModelState.IsValid)
        {
            ModelState.Remove(nameof(Service) + "." + nameof(Service.IconClassName));
            if (!ModelState.IsValid) return Page();
        }

        if (RemoveImage && !string.IsNullOrEmpty(Service.ImagePath))
        {
            DeleteImageFile(Service.ImagePath);
            Service.ImagePath = null;
        }

        try
        {
            if (ImageFile is { Length: > 0 })
            {
                if (!string.IsNullOrEmpty(Service.ImagePath))
                    DeleteImageFile(Service.ImagePath);

                var uploadsDir = Path.Combine(_env.WebRootPath, "img", "services");
                Directory.CreateDirectory(uploadsDir);
                var fileName = Guid.NewGuid().ToString("N") + ".jpg";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var inputStream = ImageFile.OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);
                const int maxSize = 1200;
                if (image.Width > maxSize || image.Height > maxSize)
                    image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(maxSize, maxSize), Mode = ResizeMode.Max }));
                await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 85 });
                Service.ImagePath = "/img/services/" + fileName;
            }

            Service.UpdatedAt = DateTime.Now;
            _db.Services.Update(Service);
            await _db.SaveChangesAsync();
            TempData["Success"] = "อัปเดตบริการเรียบร้อยแล้ว";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"เกิดข้อผิดพลาด: {ex.Message}");
            return Page();
        }
    }

    private void DeleteImageFile(string imagePath)
    {
        var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
    }
}
