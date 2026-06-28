using BusinessLayer.Dto.Genre;
using BusinessLayer.Dto.GlobalSearch;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IGenreService
{
    Task<Result<GenreDto>> GetGenreAsync(int id);
    Task<Result<List<GenreDto>>> GetGenres(ODataQueryOptions<GenreDto>? queryOptions);
    Task<Result<GenreDto>> CreateAsync(CreateGenreDto req);
    Task<Result<GenreDto>> UpdateAsync(int id, UpdateGenreDto req);
    Task<Result<bool>> CanDeleteGenreAsync(int id);
    Task<Result<Unit>> DeleteGenreAsync(int id);
    Task<Result<List<GenreSearchItemDto>>> SearchGenres(string term);
    
    Task<Result<List<GenreDto>>> GetGenresByIdsAsync(IEnumerable<int> genreIds);
}