using BusinessLayer.Dto.Author;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IAuthorService
{
    Task<Result<AuthorDto>> GetAuthorByIdAsync(int id);
    Task<Result<List<AuthorDto>>> GetAuthorsAsync(ODataQueryOptions<AuthorDto>? options);

    Task<Result<AuthorDto>> AddAuthor(AddAuthorDto req);
    Task<Result<AuthorDto>> UpdateAuthor(int id, UpdateAuthorDto req);

    Task<Result<bool>> CanDeleteAuthorAsync(int id);
    Task<Result<Unit>> DeleteAuthorAsync(int id);
    Task <Result<List<AuthorDto>>> GetAuthorsByIdsAsync(IEnumerable<int> authorIds);
}