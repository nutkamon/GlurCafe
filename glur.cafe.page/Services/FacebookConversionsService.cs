using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace glur.cafe.page.Services
{
    public interface IFacebookConversionsService
    {
        Task<bool> SendEventAsync(string eventName, HttpContext httpContext, string pixelId, string? email = null, string? phone = null);
    }

    public class FacebookConversionsService : IFacebookConversionsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<FacebookConversionsService> _logger;

        public FacebookConversionsService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<FacebookConversionsService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config            = config;
            _logger            = logger;
        }

        // Call this from page models — reads pixelId from DB settings, token from IConfiguration
        public async Task<bool> SendEventAsync(
            string eventName,
            HttpContext httpContext,
            string pixelId,
            string? email = null,
            string? phone = null)
        {
            var accessToken = _config["FacebookSecrets:ConversionsToken"] ?? "";
            if (string.IsNullOrWhiteSpace(pixelId) || string.IsNullOrWhiteSpace(accessToken))
                return false;

            return await SendEventWithSettingsAsync(eventName, httpContext, pixelId, accessToken, email, phone);
        }

        public async Task<bool> SendEventWithSettingsAsync(
            string eventName,
            HttpContext httpContext,
            string pixelId,
            string accessToken,
            string? email = null,
            string? phone = null)
        {
            if (string.IsNullOrWhiteSpace(pixelId) || string.IsNullOrWhiteSpace(accessToken))
                return false;

            try
            {
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                var ua = httpContext.Request.Headers["User-Agent"].ToString();
                var pageUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}";

                var userData = new Dictionary<string, object?>();
                if (!string.IsNullOrWhiteSpace(email))
                    userData["em"] = new[] { HashSha256(email.Trim().ToLowerInvariant()) };
                if (!string.IsNullOrWhiteSpace(phone))
                    userData["ph"] = new[] { HashSha256(phone.Trim().Replace("-", "").Replace(" ", "")) };
                if (!string.IsNullOrWhiteSpace(ip))
                    userData["client_ip_address"] = ip;
                if (!string.IsNullOrWhiteSpace(ua))
                    userData["client_user_agent"] = ua;

                var eventPayload = new
                {
                    event_name       = eventName,
                    event_time       = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    action_source    = "website",
                    event_source_url = pageUrl,
                    user_data        = userData
                };

                var body = new { data = new[] { eventPayload } };
                var json = JsonSerializer.Serialize(body);

                var client = _httpClientFactory.CreateClient("meta");
                var url    = $"https://graph.facebook.com/v20.0/{Uri.EscapeDataString(pixelId)}/events?access_token={Uri.EscapeDataString(accessToken)}";

                var response = await client.PostAsync(url,
                    new StringContent(json, Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "FacebookConversionsService: SendEventWithSettingsAsync failed");
                return false;
            }
        }

        private static string HashSha256(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}
