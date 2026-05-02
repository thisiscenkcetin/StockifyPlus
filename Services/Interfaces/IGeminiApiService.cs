namespace StockifyPlus.Services.Interfaces
{
    public interface IGeminiApiService
    {
        Task<string> GenerateResponseAsync(string userMessage, CancellationToken cancellationToken = default);
    }
}
