using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.GlobalSearch;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SQLitePCL;

namespace BusinessLayer.Cache;

public class CachedBookService : IBookService
{
    private readonly IBookService _inner;
    private readonly IMemoryCache _cache;
    private const string CachePrefix = "BOOK_";
    private const string AllBooksKey = "BOOK_LIST_DEFAULT";
    private readonly CacheSettings _settings;

    public CachedBookService(IBookService inner, IMemoryCache cache, IOptions<CacheSettings> settings)
    {
        _inner = inner;
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<Result<BookDto>> GetBook(int id)
    {
        return await _cache.GetOrCreateAsync($"{CachePrefix}{id}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _settings.BookDetail;
            return _inner.GetBook(id);
        });
    }

    public async Task<Result<List<BookDto>>> GetBooks(ODataQueryOptions<BookDto>? options, bool includeImages)
    {
        if (options == null || options.Filter == null)
        {
            return await _cache.GetOrCreateAsync(AllBooksKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _settings.BookList;
                return _inner.GetBooks(options,includeImages);
            });
        }
        return await _inner.GetBooks(options, includeImages);
    }

    public async Task<Result<BookDto>> AddBook(RequestBookDto requestBookDto)
    {
        var result = await _inner.AddBook(requestBookDto);
        _cache.Remove(AllBooksKey);
        return result;
    }

    public async Task<Result<BookDto>> UpdateBook(int id, RequestBookDto requestBookDto)
    {
        var result = await _inner.UpdateBook(id, requestBookDto);
        if (result.IsSuccess)
        {
            _cache.Remove($"{CachePrefix}{id}"); 
            _cache.Remove(AllBooksKey);
        }
        return result;
    }

    public Task<Result<BookDto>> UpdateBookStockQuantityUnsaved(int id, int stockQuantity)
    {
        return _inner.UpdateBookStockQuantityUnsaved(id, stockQuantity);
    }

    public async Task DeleteBook(int id)
    {
        await _inner.DeleteBook(id);
        _cache.Remove($"{CachePrefix}{id}");
        _cache.Remove(AllBooksKey);
    }

    public Task<Result<List<BookSearchItemDto>>> SearchBooks(string searchTerm, int? bookLimit = null)
    {
        return _inner.SearchBooks(searchTerm, bookLimit);
    }

    public Task<Result<List<BookDto>>> GetBooksByIdsAsync(IEnumerable<int> bookIds, bool includeImages)
    {
        var enumerable = bookIds.ToList();
        return _cache.GetOrCreateAsync($"{CachePrefix}BY_IDS_{string.Join("_", enumerable)}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _settings.BookList;
            return _inner.GetBooksByIdsAsync(enumerable, includeImages);
        });
    }
}