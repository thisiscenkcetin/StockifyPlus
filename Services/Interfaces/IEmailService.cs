namespace StockifyPlus.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
