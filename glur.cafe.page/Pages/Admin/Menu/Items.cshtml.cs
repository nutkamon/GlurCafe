using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace glur.cafe.page.Pages.Admin.Menu;

[Authorize]
[RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
public class ItemsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public ItemsModel(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public MenuFolder? Folder { get; set; }

    private static readonly string[] VideoExts = [".mp4", ".webm"];
    private static readonly string[] RawImageExts = [".gif"]; // keep original, no re-encode

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Folder = await _db.MenuFolders
            .Include(f => f.Items.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(f => f.Id == id);
        if (Folder == null) return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(int id, IFormFile[]? MediaFiles)
    {
        var folder = await _db.MenuFolders.Include(f => f.Items).FirstOrDefaultAsync(f => f.Id == id);
        if (folder == null) return RedirectToPage("Index");

        var files = MediaFiles?.Where(f => f.Length > 0).ToList() ?? new();
        if (files.Count == 0) return RedirectToPage(new { id });

        var dir = Path.Combine(_env.WebRootPath, "img", "menu", id.ToString());
        Directory.CreateDirectory(dir);

        int baseOrder = folder.Items.Count > 0 ? folder.Items.Max(i => i.DisplayOrder) + 1 : 1;

        for (int i = 0; i < files.Count; i++)
        {
            var ext = Path.GetExtension(files[i].FileName).ToLowerInvariant();
            var isVideo = VideoExts.Contains(ext);
            var keepRaw = RawImageExts.Contains(ext);

            string fileName;
            if (isVideo || keepRaw)
            {
                fileName = Guid.NewGuid().ToString("N") + ext;
                var filePath = Path.Combine(dir, fileName);
                using var outStream = System.IO.File.Create(filePath);
                await files[i].CopyToAsync(outStream);
            }
            else
            {
                fileName = Guid.NewGuid().ToString("N") + ".jpg";
                var filePath = Path.Combine(dir, fileName);
                using var inputStream = files[i].OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);

                const int maxSize = 1600;
                if (image.Width > maxSize || image.Height > maxSize)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(maxSize, maxSize),
                        Mode = ResizeMode.Max
                    }));

                await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 85 });
            }

            _db.MenuItems.Add(new MenuItem
            {
                FolderId     = id,
                FilePath     = $"/img/menu/{id}/{fileName}",
                MediaType    = isVideo ? "video" : "image",
                DisplayOrder = baseOrder + i,
                IsActive     = true,
                CreatedAt    = DateTime.Now
            });
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"อัปโหลดเรียบร้อยแล้ว ({files.Count} ไฟล์)";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostEditItemAsync(
        int id, int itemId, string? Title, decimal? Price, string? Description, string? Options)
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == itemId && i.FolderId == id);
        if (item != null)
        {
            item.Title = string.IsNullOrWhiteSpace(Title) ? null : Title.Trim();
            item.Price = Price;
            item.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            item.Options = string.IsNullOrWhiteSpace(Options) ? null : Options.Trim();
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostToggleRecoAsync(int id, int itemId)
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == itemId && i.FolderId == id);
        if (item != null)
        {
            item.IsRecommended = !item.IsRecommended;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUploadItemMediaAsync(int id, int itemId, IFormFile? MediaFile)
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == itemId && i.FolderId == id);
        if (item == null) return RedirectToPage(new { id });

        if (MediaFile == null || MediaFile.Length == 0)
        {
            TempData["Error"] = "กรุณาเลือกไฟล์";
            return RedirectToPage(new { id });
        }

        var ext = Path.GetExtension(MediaFile.FileName).ToLowerInvariant();
        var isVideo = VideoExts.Contains(ext);
        var keepRaw = RawImageExts.Contains(ext);

        var dir = Path.Combine(_env.WebRootPath, "img", "menu", id.ToString());
        Directory.CreateDirectory(dir);

        string fileName;
        try
        {
            if (isVideo || keepRaw)
            {
                fileName = Guid.NewGuid().ToString("N") + ext;
                var filePath = Path.Combine(dir, fileName);
                using var outStream = System.IO.File.Create(filePath);
                await MediaFile.CopyToAsync(outStream);
            }
            else
            {
                fileName = Guid.NewGuid().ToString("N") + ".jpg";
                var filePath = Path.Combine(dir, fileName);
                using var inputStream = MediaFile.OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);

                const int maxSize = 1600;
                if (image.Width > maxSize || image.Height > maxSize)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(maxSize, maxSize),
                        Mode = ResizeMode.Max
                    }));

                await image.SaveAsJpegAsync(filePath, new JpegEncoder { Quality = 85 });
            }
        }
        catch
        {
            TempData["Error"] = "ไฟล์ไม่ถูกต้อง — รองรับรูปภาพ/GIF/วิดีโอ (mp4, webm) เท่านั้น";
            return RedirectToPage(new { id });
        }

        // ลบไฟล์เก่าถ้ามี
        if (!string.IsNullOrEmpty(item.FilePath))
        {
            var oldPath = Path.Combine(_env.WebRootPath, item.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
        }

        item.FilePath = $"/img/menu/{id}/{fileName}";
        item.MediaType = isVideo ? "video" : "image";
        await _db.SaveChangesAsync();

        TempData["ToastMessage"] = "อัปโหลดรูปเรียบร้อยแล้ว";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostToggleItemAsync(int id, int itemId)
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == itemId && i.FolderId == id);
        if (item != null)
        {
            item.IsActive = !item.IsActive;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostMoveAsync(int id, int itemId, int dir)
    {
        var items = await _db.MenuItems
            .Where(i => i.FolderId == id)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync();

        var idx = items.FindIndex(i => i.Id == itemId);
        if (idx >= 0)
        {
            var swapIdx = idx + dir;
            if (swapIdx >= 0 && swapIdx < items.Count)
            {
                (items[idx].DisplayOrder, items[swapIdx].DisplayOrder) =
                    (items[swapIdx].DisplayOrder, items[idx].DisplayOrder);
                await _db.SaveChangesAsync();
            }
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteItemAsync(int id, int itemId)
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(i => i.Id == itemId && i.FolderId == id);
        if (item != null)
        {
            var fullPath = Path.Combine(_env.WebRootPath, item.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

            _db.MenuItems.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "ลบสื่อเรียบร้อยแล้ว";
        }
        return RedirectToPage(new { id });
    }
}
