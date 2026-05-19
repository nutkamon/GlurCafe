using glur.cafe.page.Data;
using glur.cafe.page.Models;
using glur.cafe.page.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace glur.cafe.page.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;

        public IndexModel(ApplicationDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        public List<Service> Services { get; set; } = new();
        public List<Portfolio> Portfolios { get; set; } = new();
        public List<PricingPlan> PricingPlans { get; set; } = new();
        public List<ProcessStep> ProcessSteps { get; set; } = new();
        public List<WhyUsItem> WhyUsItems { get; set; } = new();
        public List<StatItem> Stats { get; set; } = new();
        public Dictionary<string, string> Site { get; set; } = new();

        [BindProperty] public ContactMessage? ContactInput { get; set; }

        public async Task OnGetAsync()
        {
            Services     = await _db.Services.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder).ToListAsync();
            Portfolios   = await _db.Portfolios.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToListAsync();
            PricingPlans = await _db.PricingPlans.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder).ToListAsync();
            ProcessSteps = await _db.ProcessSteps.Where(p => p.IsActive).OrderBy(p => p.StepNumber).ToListAsync();
            WhyUsItems   = await _db.WhyUsItems.Where(w => w.IsActive).OrderBy(w => w.DisplayOrder).ToListAsync();
            Stats        = await _db.StatItems.Where(s => s.IsActive).OrderBy(s => s.DisplayOrder).ToListAsync();
            Site         = await _db.SiteSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
        }

        public async Task<IActionResult> OnPostContactAsync()
        {
            if (ContactInput == null ||
                string.IsNullOrWhiteSpace(ContactInput.FullName) ||
                string.IsNullOrWhiteSpace(ContactInput.Phone))
            {
                return new JsonResult(new { success = false, message = "กรุณากรอกชื่อและเบอร์โทรให้ครบถ้วน" });
            }

            ContactInput.CreatedAt = DateTime.Now;
            ContactInput.IsRead    = false;
            _db.ContactMessages.Add(ContactInput);
            await _db.SaveChangesAsync();

            await OnGetAsync();

            try
            {
                await _email.SendContactNotificationAsync(
                    ContactInput.FullName,
                    ContactInput.Phone,
                    ContactInput.Email,
                    ContactInput.ServiceType,
                    ContactInput.Message
                );
            }
            catch { /* email failure should not break the submission */ }

            return new JsonResult(new { success = true, message = "ขอบคุณที่ติดต่อเรา ☕ เราจะตอบกลับโดยเร็วที่สุด" });
        }

        public static List<string> ParseFeatures(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new();
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
            catch { return new(); }
        }
    }
}
