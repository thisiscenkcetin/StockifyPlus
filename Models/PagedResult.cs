namespace StockifyPlus.Models
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;

        public int FirstItemIndex => TotalCount == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;

        public int LastItemIndex => Math.Min(CurrentPage * PageSize, TotalCount);
    }
}

