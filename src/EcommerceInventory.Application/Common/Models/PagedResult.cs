namespace EcommerceInventory.Application.Common.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
    public int  TotalCount  { get; set; }
    public int  PageNumber  { get; set; }
    public int  PageSize    { get; set; }
    public int  TotalPages  => PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext     => PageNumber < TotalPages;

    public static PagedResult<T> Create(
        IReadOnlyList<T> items, int totalCount,
        int pageNumber, int pageSize)
    {
        return new PagedResult<T>
        {
            Items      = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize   = pageSize
        };
    }
}
