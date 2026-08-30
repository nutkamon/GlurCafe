using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using glur.cafe.page.Data;
using glur.cafe.page.Services;
using glur.cafe.page.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

            // ===== Facebook Conversions API =====
            builder.Services.AddHttpClient("meta");
            builder.Services.AddScoped<FacebookConversionsService>();

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
                DbSeeder.EnsureFacebookSettings(context);
                DbSeeder.EnsureMenuDefaults(context);
                DbSeeder.EnsurePosMenuImported(context);
            }

            // ===== Facebook Messenger Webhook =====
            // GET /api/facebook/webhook — verification by Facebook
            app.MapGet("/api/facebook/webhook", (HttpContext ctx, IConfiguration config) =>
            {
                var mode      = ctx.Request.Query["hub.mode"].ToString();
                var token     = ctx.Request.Query["hub.verify_token"].ToString();
                var challenge = ctx.Request.Query["hub.challenge"].ToString();
                var expected  = config["FacebookSecrets:WebhookVerifyToken"] ?? "GlurCafeWebhook2026";

                if (mode == "subscribe" && token == expected)
                    return Results.Content(challenge, "text/plain");

                return Results.Forbid();
            }).AllowAnonymous();

            // POST /api/facebook/webhook — receive messages from Facebook
            app.MapPost("/api/facebook/webhook", async (HttpContext ctx, ApplicationDbContext db,
                IConfiguration config, IHttpClientFactory httpFactory, ILogger<Program> logger) =>
            {
                // Verify X-Hub-Signature-256 for security
                var appSecret = config["FacebookSecrets:AppSecret"] ?? "";
                if (!string.IsNullOrEmpty(appSecret))
                {
                    ctx.Request.EnableBuffering();
                    using var ms = new MemoryStream();
                    await ctx.Request.Body.CopyToAsync(ms);
                    var bodyBytes = ms.ToArray();
                    ctx.Request.Body.Position = 0;

                    var sigHeader = ctx.Request.Headers["X-Hub-Signature-256"].ToString();
                    if (!string.IsNullOrEmpty(sigHeader))
                    {
                        var expectedSig = "sha256=" + Convert.ToHexString(
                            HMACSHA256.HashData(Encoding.UTF8.GetBytes(appSecret), bodyBytes)).ToLower();
                        if (!sigHeader.Equals(expectedSig, StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogWarning("Messenger webhook: invalid signature");
                            return Results.Forbid();
                        }
                    }
                }

                string body;
                using (var reader = new StreamReader(ctx.Request.Body, leaveOpen: true))
                    body = await reader.ReadToEndAsync();

                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    if (root.GetProperty("object").GetString() != "page")
                        return Results.Ok("EVENT_RECEIVED");

                    foreach (var entry in root.GetProperty("entry").EnumerateArray())
                    {
                        if (!entry.TryGetProperty("messaging", out var messaging)) continue;

                        foreach (var evt in messaging.EnumerateArray())
                        {
                            var psid = evt.GetProperty("sender").GetProperty("id").GetString() ?? "";
                            if (string.IsNullOrEmpty(psid)) continue;

                            // Ignore echo (messages sent by the page itself)
                            if (evt.TryGetProperty("message", out var msgEl) &&
                                msgEl.TryGetProperty("is_echo", out var echo) && echo.GetBoolean())
                                continue;

                            string? text = null;
                            string? mid  = null;
                            long timestamp = 0;

                            if (evt.TryGetProperty("timestamp", out var ts))
                                timestamp = ts.GetInt64();

                            if (evt.TryGetProperty("message", out var msgProp))
                            {
                                msgProp.TryGetProperty("text", out var textEl);
                                text = textEl.ValueKind == JsonValueKind.String ? textEl.GetString() : null;
                                msgProp.TryGetProperty("mid", out var midEl);
                                mid = midEl.ValueKind == JsonValueKind.String ? midEl.GetString() : null;
                            }
                            else if (evt.TryGetProperty("postback", out var pb))
                            {
                                pb.TryGetProperty("title", out var titleEl);
                                text = "[Postback] " + (titleEl.ValueKind == JsonValueKind.String ? titleEl.GetString() : "");
                            }

                            // Deduplicate by mid
                            if (mid != null && db.MessengerMessages.Any(m => m.MessageId == mid))
                                continue;

                            var sentAt = timestamp > 0
                                ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime
                                : DateTime.Now;

                            // Find or create conversation
                            var conv = db.MessengerConversations.FirstOrDefault(c => c.PsId == psid);
                            if (conv == null)
                            {
                                conv = new MessengerConversation
                                {
                                    PsId        = psid,
                                    DisplayName = "ผู้ใช้ Facebook",
                                    CreatedAt   = sentAt,
                                    LastMessageAt = sentAt
                                };
                                db.MessengerConversations.Add(conv);
                                await db.SaveChangesAsync();

                                // Try fetch user name from Graph API
                                var pageToken = config["FacebookSecrets:PageAccessToken"] ?? "";
                                if (!string.IsNullOrEmpty(pageToken))
                                {
                                    try
                                    {
                                        var client = httpFactory.CreateClient("meta");
                                        client.Timeout = TimeSpan.FromSeconds(5);
                                        var resp = await client.GetAsync(
                                            $"https://graph.facebook.com/v20.0/{psid}?fields=name&access_token={pageToken}");
                                        if (resp.IsSuccessStatusCode)
                                        {
                                            var json = await resp.Content.ReadAsStringAsync();
                                            using var nameDoc = JsonDocument.Parse(json);
                                            if (nameDoc.RootElement.TryGetProperty("name", out var nameEl))
                                                conv.DisplayName = nameEl.GetString() ?? conv.DisplayName;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.LogWarning(ex, "Could not fetch Messenger user profile for {PsId}", psid);
                                    }
                                }
                            }

                            var message = new MessengerMessage
                            {
                                ConversationId = conv.Id,
                                MessageId      = mid,
                                IsFromPage     = false,
                                Text           = text,
                                SentAt         = sentAt,
                                IsRead         = false
                            };
                            db.MessengerMessages.Add(message);

                            conv.LastMessageText = text ?? "(ไฟล์แนบ)";
                            conv.LastMessageAt   = sentAt;
                            conv.UnreadCount    += 1;

                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Messenger webhook processing error");
                }

                return Results.Ok("EVENT_RECEIVED");
            }).AllowAnonymous();

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

            app.MapRazorPages();

            app.Run();
        }
    }
}
