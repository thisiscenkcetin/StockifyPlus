namespace StockifyPlus.Configurations
{
    public class GeminiOptions
    {
        public const string SectionName = "Gemini";

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gemini-2.0-flash";

        public string SystemPrompt { get; set; } = string.Empty;
    }
}
