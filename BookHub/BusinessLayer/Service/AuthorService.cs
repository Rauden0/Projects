using BusinessLayer.Dto.Author;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

public class AuthorService : IAuthorService
{
    private readonly IUnitOfWork _uow;

    public AuthorService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<AuthorDto>> GetAuthorByIdAsync(int id)
    {
        var author = await _uow.Authors.GetByIdAsync(id);
        return author is not null
            ? AuthorMapper.ToDto(author)
            : new Result<AuthorDto>(new Exception($"Author {id} not found"));
    }

    public async Task<Result<List<AuthorDto>>> GetAuthorsAsync(ODataQueryOptions<AuthorDto>? options)
    {
        return await AuthorMapper
            .ProjectToDto(_uow.Authors.Query())
            .ApplyIfNotNull(options)
            .ToListAsync();
    }

    public async Task<Result<AuthorDto>> AddAuthor(AddAuthorDto req)
    {
        var author = new Author { Name = req.Name };
        _uow.Authors.Add(author);
        await _uow.SaveChangesAsync();
        return AuthorMapper.ToDto(author);
    }

    public async Task<Result<AuthorDto>> UpdateAuthor(int id, UpdateAuthorDto req)
    {
        var author = await _uow.Authors.GetByIdAsync(id);
        if (author is null)
            return new Result<AuthorDto>(new Exception($"Author {id} not found"));

        author.Name = req.Name;
        _uow.Authors.Update(author);
        await _uow.SaveChangesAsync();
        return AuthorMapper.ToDto(author);
    }

    // =========================
    // SAFE DELETE LOGIC
    // =========================

    public async Task<Result<bool>> CanDeleteAuthorAsync(int id)
    {
        var author = await _uow.Authors.GetByIdAsync(id);
        if (author is null)
            return new Result<bool>(new Exception($"Author {id} not found"));

        var usedByAnyBook = await _uow.Books
            .Query()
            .AnyAsync(b => b.Authors.Any(a => a.Id == id));

        return !usedByAnyBook;
    }

    public async Task<Result<Unit>> DeleteAuthorAsync(int id)
    {
        var author = await _uow.Authors.GetByIdAsync(id);
        if (author is null)
            return new Result<Unit>(new Exception($"Author {id} not found"));

        var canDeleteResult = await CanDeleteAuthorAsync(id);
        if (canDeleteResult.IsFaulted)
        {
            var ex = canDeleteResult.Match<Exception?>(
                Succ: _ => null,
                Fail: e => e);

            return new Result<Unit>(ex!);
        }

        var canDelete = canDeleteResult.Match(x => x, _ => false);
        if (!canDelete)
            return new Result<Unit>(
                new Exception("Author cannot be deleted because it is used by at least one book.")
            );

        _uow.Authors.Remove(author);
        await _uow.SaveChangesAsync();
        return Unit.Default;
    }

    public async Task<Result<List<AuthorDto>>> GetAuthorsByIdsAsync(IEnumerable<int> authorIds)
    {        
        return await AuthorMapper.ProjectToDto(
            _uow.Authors
                .Query()
                .Where(a => authorIds.Contains(a.Id))
        ).ToListAsync();
    }
}
