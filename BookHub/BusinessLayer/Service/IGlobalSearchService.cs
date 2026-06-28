using BusinessLayer.Dto.GlobalSearch;

namespace BusinessLayer.Service;

public interface IGlobalSearchService
{
    public Task<GlobalSearchDto> SearchAll(string term);

}