using BusinessLayer.Dto.Author;
using BusinessLayer.Service;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Cache;

public class CachedAuthorService : IAuthorService
{
    private readonly IAuthorService _inner;
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _settings;
    private const string CachePrefix = "AUTHOR_";
    private const string AllAuthorsKey = "AUTHOR_LIST_ALL";

    public CachedAuthorService(IAuthorService inner, IMemoryCache cache,  IOptions<CacheSettings>  settings)
    {
        _inner = inner;
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<Result<AuthorDto>> GetAuthorByIdAsync(int id)
    {
        return await _cache.GetOrCreateAsync($"{CachePrefix}{id}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _settings.AuthorDetail;
            return _inner.GetAuthorByIdAsync(id);
        });
    }

    public async Task<Result<List<AuthorDto>>> GetAuthorsAsync(ODataQueryOptions<AuthorDto>? options)
    {
        // Cache the full list (navbar/dropdowns) only if no filters are applied
        if (options == null || options.Filter == null)
        {
            return await _cache.GetOrCreateAsync(AllAuthorsKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _settings.AuthorList;
                return _inner.GetAuthorsAsync(options);
            });
        }
        return await _inner.GetAuthorsAsync(options);
    }

    public async Task<Result<AuthorDto>> AddAuthor(AddAuthorDto dto)
    {
        var result = await _inner.AddAuthor(dto);
        _cache.Remove(AllAuthorsKey);
        return result;
    }

    public async Task<Result<AuthorDto>> UpdateAuthor(int id, UpdateAuthorDto dto)
    {
        var result = await _inner.UpdateAuthor(id, dto);
        if (result.IsSuccess)
        {
            _cache.Remove($"{CachePrefix}{id}");
            _cache.Remove(AllAuthorsKey);
        }
        return result;
    }

    public Task<Result<bool>> CanDeleteAuthorAsync(int id)
    {
        return _inner.CanDeleteAuthorAsync(id);
    }

    public Task<Result<Unit>> DeleteAuthorAsync(int id)
    {
        _cache.Remove($"{CachePrefix}{id}");
        _cache.Remove(AllAuthorsKey);
        return _inner.DeleteAuthorAsync(id);    }

    public Task<Result<List<AuthorDto>>> GetAuthorsByIdsAsync(IEnumerable<int> authorIds)
    {
        var enumerable = authorIds.ToList();
        return _cache.GetOrCreateAsync(
            $"{CachePrefix}BULK_{string.Join("_", enumerable.OrderBy(id => id))}",
            entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _settings.AuthorList;
                return _inner.GetAuthorsByIdsAsync(enumerable);
            });
    }
}