using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using glur.cafe.page.Data;
using glur.cafe.page.Services;
// GLUR CAFE — hot reload trigger

namespace glur.cafe.page
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== Database (SQLite) =====
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? "Data Source=glurcafe.db;Mode=ReadWriteCreate";
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString, o => o.CommandTimeout(60)));

            // ===== Email Service =====
            builder.Services.AddSingleton<IEmailService, EmailService>();

            // ===== Cookie Authentication =====
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Admin/Login";
                    options.LogoutPath = "/Admin/Logout";
                    options.AccessDeniedPath = "/Admin/Login";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.Cookie.Name = "GlurCafe.Auth";
                    options.Cookie.HttpOnly = true;
                });

            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddAntiforgery();

            // File uploads — no size limit (images are resized server-side)
            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = long.MaxValue;
            });
            builder.WebHost.ConfigureKestrel(k =>
            {
                k.Limits.MaxRequestBodySize = null;
            });

            var app = builder.Build();

            // ===== Seed Database & configure SQLite =====
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Set WAL mode once at startup — prevents SQLite locking on Linux
                context.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
                context.Database.ExecuteSqlRaw("PRAGMA busy_timeout=10000;");
                context.Database.ExecuteSqlRaw("PRAGMA synchronous=NORMAL;");
                DbSeeder.EnsureMissingTables(context);
                DbSeeder.Seed(context);
                DbSeeder.UpdateContactSettings(context);
                DbSeeder.UpdatePricingPlans(context);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}
