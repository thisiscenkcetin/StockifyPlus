namespace StockifyPlus.Configurations
{
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = "StockifyPlus";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
    }
}
