using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using StockifyPlus.Configurations;
using StockifyPlus.Services.Interfaces;

namespace StockifyPlus.Services.Implementations
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _smtpOptions;

        public SmtpEmailService(IOptions<SmtpOptions> smtpOptions)
        {
            _smtpOptions = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
        }

        public async Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Hedef e-posta adresi bos olamaz.", nameof(toEmail));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("E-posta konusu bos olamaz.", nameof(subject));

            if (string.IsNullOrWhiteSpace(_smtpOptions.Host))
                throw new InvalidOperationException("SMTP host ayari eksik.");

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpOptions.SenderEmail, _smtpOptions.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            using var smtpClient = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
            {
                EnableSsl = _smtpOptions.UseSsl,
                Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password)
            };

            cancellationToken.ThrowIfCancellationRequested();
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
