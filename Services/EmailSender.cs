using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthBackend.Services
{
    public class EmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _fromPassword;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"];
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
            _fromEmail = configuration["EmailSettings:FromEmail"];
            _fromPassword = configuration["EmailSettings:FromPassword"];
            _logger = logger;
        }

        public void SendEmail(string toEmail, string code)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail),
                    Subject = "Код подтверждения",
                    Body = $"Ваш код подтверждения: {code}",
                    IsBodyHtml = false
                };
                message.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                    EnableSsl = true
                };

                smtpClient.Send(message);
                _logger.LogInformation($"[{DateTime.Now}] Email с кодом {code} отправлен на {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка отправки email: {ex.Message}");
            }
        }
    }
}
