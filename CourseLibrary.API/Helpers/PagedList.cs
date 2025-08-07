using Microsoft.EntityFrameworkCore;

namespace CourseLibrary.API.Helpers;

public class PagedList<T> : List<T>
{
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
        AddRange(items);
    }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;


    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
