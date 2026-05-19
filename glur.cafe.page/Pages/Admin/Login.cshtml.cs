using glur.cafe.page.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace glur.cafe.page.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public string ErrorMessage { get; set; } = string.Empty;

    public LoginModel(ApplicationDbContext db) => _db = db;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "กรุณากรอกชื่อผู้ใช้และรหัสผ่าน";
            return Page();
        }

        var hash = HashPassword(password);
        var user = await _db.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash && u.IsActive);

        if (user == null)
        {
            ErrorMessage = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
            return Page();
        }

        user.LastLoginAt = DateTime.Now;
        await _db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

        return LocalRedirect(Url.Content("~/Admin"));
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }
}
