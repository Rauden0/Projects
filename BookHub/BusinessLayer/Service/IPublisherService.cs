using BusinessLayer.Dto.GlobalSearch;
using BusinessLayer.Dto.Publisher;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IPublisherService
{
    Task<Result<PublisherDto>> GetPublisherAsync(int id);
    Task<Result<List<PublisherDto>>> GetPublishers(ODataQueryOptions<PublisherDto> options);
    Task<Result<PublisherDto>> CreateAsync(CreatePublisherDto req);
    Task<Result<PublisherDto>> UpdateAsync(int id, UpdatePublisherDto req);

    Task<Result<bool>> CanDeleteAsync(int id);
    Task<Result<Unit>> DeleteAsync(int id);
    
    Task<Result<List<PublisherSearchItemDto>>> SearchPublishers(string searchTerm);
}