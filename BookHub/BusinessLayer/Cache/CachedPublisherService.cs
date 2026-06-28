using BusinessLayer.Dto.GlobalSearch;
using BusinessLayer.Dto.Publisher;
using BusinessLayer.Service;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace BusinessLayer.Cache;

public class CachedPublisherService : IPublisherService
{
    private readonly IPublisherService _inner;
    private readonly IMemoryCache _cache;
    private readonly CacheSettings _settings;
    private const string CachePrefix = "PUB_";
    private const string ListKey = "PUB_LIST_ALL";

    public CachedPublisherService(IPublisherService inner, IMemoryCache cache, IOptions<CacheSettings>  settings)
    {
        _inner = inner;
        _cache = cache;
        _settings = settings.Value;
    }

    public async Task<Result<PublisherDto>> GetPublisherAsync(int id)
    {
        return await _cache.GetOrCreateAsync($"{CachePrefix}{id}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _settings.PublisherDetail;
            return _inner.GetPublisherAsync(id);
        });
    }

    public async Task<Result<List<PublisherDto>>> GetPublishers(ODataQueryOptions<PublisherDto>? options)
    {
        if (options == null || options.Filter == null)
        {
            return await _cache.GetOrCreateAsync(ListKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _settings.PublisherList;
                return _inner.GetPublishers(options);
            });
        }
        return await _inner.GetPublishers(options);
    }

    public async Task<Result<PublisherDto>> CreateAsync(CreatePublisherDto dto)
    {
        var result = await _inner.CreateAsync(dto);
        _cache.Remove(ListKey);
        return result;
    }

    public async Task<Result<PublisherDto>> UpdateAsync(int id, UpdatePublisherDto dto)
    {
        var result = await _inner.UpdateAsync(id, dto);
        if (result.IsSuccess)
        {
            _cache.Remove($"{CachePrefix}{id}");
            _cache.Remove(ListKey);
        }
        return result;
    }

    public Task<Result<bool>> CanDeleteAsync(int id)
    {
        return _inner.CanDeleteAsync(id);
    }

    public Task<Result<Unit>> DeleteAsync(int id)
    {
        _cache.Remove($"{CachePrefix}{id}");
        _cache.Remove(ListKey);
        return _inner.DeleteAsync(id);;
    }

    public Task<Result<List<PublisherSearchItemDto>>> SearchPublishers(string searchTerm)
    {
        return _inner.SearchPublishers(searchTerm);
    }
}