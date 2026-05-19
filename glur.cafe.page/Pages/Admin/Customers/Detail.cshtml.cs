using glur.cafe.page.Data;
using glur.cafe.page.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace glur.cafe.page.Pages.Admin.Customers;

[Authorize]
public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DetailModel(ApplicationDbContext db) => _db = db;

    public Customer Customer { get; set; } = null!;
    public List<Quotation> Quotations { get; set; } = [];
    public List<ContactMessage> ContactMessages { get; set; } = [];

    [BindProperty] public string InteractionType { get; set; } = "note";
    [BindProperty] public string? InteractionSubject { get; set; }
    [BindProperty] public string? InteractionNotes { get; set; }
    [BindProperty] public DateTime? FollowUpDate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ViewData["ActiveMenu"] = "customers";
        ViewData["UnreadCount"] = await _db.ContactMessages.CountAsync(m => !m.IsRead);

        var c = await _db.Customers
            .Include(x => x.Interactions.OrderByDescending(i => i.CreatedAt))
            .FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();

        Customer = c;
        Quotations = await _db.Quotations.Where(q => q.CustomerId == id).OrderByDescending(q => q.CreatedAt).ToListAsync();
        ContactMessages = await _db.ContactMessages.Where(m => m.CustomerId == id).OrderByDescending(m => m.CreatedAt).ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAddInteractionAsync(int id)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c is null) return NotFound();

        var interaction = new CustomerInteraction
        {
            CustomerId = id,
            Type = InteractionType,
            Subject = InteractionSubject,
            Notes = InteractionNotes,
            FollowUpDate = FollowUpDate,
            CreatedByAdmin = User.Identity?.Name,
            CreatedAt = DateTime.Now
        };

        c.LastContactedAt = DateTime.Now;
        c.UpdatedAt = DateTime.Now;

        _db.CustomerInteractions.Add(interaction);
        await _db.SaveChangesAsync();

        TempData["Success"] = "บันทึกการติดต่อเรียบร้อย";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteInteractionAsync(int id, int interactionId)
    {
        var interaction = await _db.CustomerInteractions.FindAsync(interactionId);
        if (interaction != null)
        {
            _db.CustomerInteractions.Remove(interaction);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostMarkFollowUpDoneAsync(int id, int interactionId)
    {
        var interaction = await _db.CustomerInteractions.FindAsync(interactionId);
        if (interaction != null)
        {
            interaction.IsFollowUpDone = true;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(int id, string status)
    {
        var c = await _db.Customers.FindAsync(id);
        if (c is null) return NotFound();

        c.Status = status;
        c.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        TempData["Success"] = "อัปเดตสถานะเรียบร้อย";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostConvertFromMessageAsync(int id, int messageId)
    {
        // Link a ContactMessage to this customer
        var msg = await _db.ContactMessages.FindAsync(messageId);
        if (msg is null) return NotFound();

        msg.CustomerId = id;
        await _db.SaveChangesAsync();

        TempData["Success"] = "เชื่อมข้อความกับลูกค้าเรียบร้อย";
        return RedirectToPage(new { id });
    }
}
