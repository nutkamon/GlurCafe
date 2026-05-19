using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace glur.cafe.page.Services
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = "";
        public string SenderPassword { get; set; } = "";
        public string SenderName { get; set; } = "GLUR CAFE";
        public string AdminEmail { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
    }

    public interface IEmailService
    {
        Task SendContactNotificationAsync(string customerName, string phone, string? email, string? serviceType, string? message);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _settings = new EmailSettings();
            configuration.GetSection("EmailSettings").Bind(_settings);
            _logger = logger;
        }

        public async Task SendContactNotificationAsync(string customerName, string phone, string? email, string? serviceType, string? message)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost) || string.IsNullOrWhiteSpace(_settings.AdminEmail))
            {
                _logger.LogWarning("Email settings not configured. Skipping notification.");
                return;
            }

            try
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                mimeMessage.To.Add(MailboxAddress.Parse(_settings.AdminEmail));
                mimeMessage.Subject = $"☕ ข้อความใหม่จาก {customerName} - GLUR CAFE";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                    <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background: linear-gradient(135deg, #00A59A, #007A72); padding: 20px; border-radius: 12px 12px 0 0;'>
                            <h2 style='color: #fff; margin: 0;'>☕ ข้อความติดต่อใหม่</h2>
                            <p style='color: rgba(255,255,255,0.8); margin: 5px 0 0;'>GLUR CAFE — โรงคั่วกาแฟ & กาแฟสด</p>
                        </div>
                        <div style='background: #fff; padding: 24px; border: 1px solid #e2e8f0; border-top: none; border-radius: 0 0 12px 12px;'>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr>
                                    <td style='padding: 8px 12px; font-weight: 600; color: #007A72; width: 140px;'>ชื่อ:</td>
                                    <td style='padding: 8px 12px;'>{System.Net.WebUtility.HtmlEncode(customerName)}</td>
                                </tr>
                                <tr style='background: #f8fafc;'>
                                    <td style='padding: 8px 12px; font-weight: 600; color: #007A72;'>เบอร์โทร:</td>
                                    <td style='padding: 8px 12px;'>{System.Net.WebUtility.HtmlEncode(phone)}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 12px; font-weight: 600; color: #007A72;'>อีเมล:</td>
                                    <td style='padding: 8px 12px;'>{System.Net.WebUtility.HtmlEncode(email ?? "-")}</td>
                                </tr>
                                <tr style='background: #f8fafc;'>
                                    <td style='padding: 8px 12px; font-weight: 600; color: #007A72;'>ประเภทที่สนใจ:</td>
                                    <td style='padding: 8px 12px;'>{System.Net.WebUtility.HtmlEncode(serviceType ?? "-")}</td>
                                </tr>
                                <tr>
                                    <td style='padding: 8px 12px; font-weight: 600; color: #007A72; vertical-align: top;'>ข้อความ:</td>
                                    <td style='padding: 8px 12px;'>{System.Net.WebUtility.HtmlEncode(message ?? "-")}</td>
                                </tr>
                            </table>
                        </div>
                        <p style='text-align:center; color: #94a3b8; font-size: 0.8rem; margin-top: 16px;'>
                            ส่งจาก GLUR CAFE Admin System
                        </p>
                    </div>"
                };

                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort,
                    _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notification");
            }
        }
    }
}
