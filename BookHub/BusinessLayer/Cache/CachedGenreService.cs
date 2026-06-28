using BusinessLayer.Dto.Genre;
using BusinessLayer.Dto.GlobalSearch;
using BusinessLayer.Service;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Cache;

public class CachedGenreService : IGenreService
{
    private readonly IGenreService _inner;
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _settings;
    private const string CachePrefix = "GENRE_";
    private const string AllGenresKey = "GENRE_ALL";
    
    public CachedGenreService(IGenreService inner, IMemoryCache cache,  IOptions<CacheSettings> settings)
    {
        _inner = inner;
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<Result<GenreDto>> GetGenreAsync(int id)
    {
        return await _cache.GetOrCreateAsync($"{CachePrefix}{id}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _settings.GenreDetail;
            return _inner.GetGenreAsync(id);
        });
    }

    public async Task<Result<List<GenreDto>>> GetGenres(ODataQueryOptions<GenreDto>? options)
    {
        if (options == null || (options.Filter == null && options.OrderBy == null))
        {
            return await _cache.GetOrCreateAsync(AllGenresKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _settings.GenreList;
                return _inner.GetGenres(options);
            });
        }
        return await _inner.GetGenres(options);
    }

    public async Task<Result<GenreDto>> CreateAsync(CreateGenreDto dto)
    {
        var result = await _inner.CreateAsync(dto);
        _cache.Remove(AllGenresKey);
        return result;
    }

    public async Task<Result<GenreDto>> UpdateAsync(int id, UpdateGenreDto dto)
    {
        var result = await _inner.UpdateAsync(id, dto);
        if (result.IsSuccess)
        {
            _cache.Remove($"{CachePrefix}{id}");
            _cache.Remove(AllGenresKey);
        }
        return result;
    }

    public Task<Result<bool>> CanDeleteGenreAsync(int id)
    {
        return _inner.CanDeleteGenreAsync(id);
    }

    public Task<Result<Unit>> DeleteGenreAsync(int id)
    {
        _cache.Remove($"{CachePrefix}{id}");
        _cache.Remove(AllGenresKey);
        return _inner.DeleteGenreAsync(id);
    }

    public Task<Result<List<GenreSearchItemDto>>> SearchGenres(string term)
    {
        return _inner.SearchGenres(term);
    }

    public Task<Result<List<GenreDto>>> GetGenresByIdsAsync(IEnumerable<int> genreIds)
    {
        var enumerable = genreIds.ToList();
        return _cache.GetOrCreateAsync($"{CachePrefix}BULK_{string.Join("_", enumerable)}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _settings.GenreList;
            return _inner.GetGenresByIdsAsync(enumerable);
        });
    }
}