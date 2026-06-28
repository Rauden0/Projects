using BusinessLayer.Dto.Genre;
using BusinessLayer.Dto.GlobalSearch;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

public class GenreService : IGenreService
{
    private readonly IUnitOfWork _uow;
    private const int GenreSearchResultLimit = 5;

    public GenreService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<GenreDto>> GetGenreAsync(int id)
    {
        var genre = await _uow.Genres.GetByIdAsync(id);
        return genre is not null
            ? GenreMapper.ToDto(genre)
            : new Result<GenreDto>(new Exception($"Genre {id} not found"));
    }

    public async Task<Result<List<GenreDto>>> GetGenres(ODataQueryOptions<GenreDto>? options)
    {
        return await GenreMapper.ProjectToDto(_uow.Genres.Query())
            .ApplyIfNotNull(options)
            .ToListAsync();
    }

    public async Task<Result<GenreDto>> CreateAsync(CreateGenreDto req)
    {
        var genre = new Genre { Name = req.Name };
        _uow.Genres.Add(genre);
        await _uow.SaveChangesAsync();
        return GenreMapper.ToDto(genre);
    }

    public async Task<Result<GenreDto>> UpdateAsync(int id, UpdateGenreDto req)
    {
        var genre = await _uow.Genres.GetByIdAsync(id);
        if (genre is null)
            return new Result<GenreDto>(new Exception($"Genre {id} not found"));

        genre.Name = req.Name;
        _uow.Genres.Update(genre);
        await _uow.SaveChangesAsync();
        return GenreMapper.ToDto(genre);
    }

    // původní delete - může zůstat pro interní použití
    public async Task DeleteAsync(int id)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var genre = await _uow.Genres.GetByIdAsync(id);
            if (genre != null)
                _uow.Genres.Remove(genre);

            await _uow.SaveChangesAsync();
        });
    }

    // =========================
    // SAFE DELETE
    // =========================

    public async Task<Result<bool>> CanDeleteGenreAsync(int id)
    {
        var genre = await _uow.Genres.GetByIdAsync(id);
        if (genre is null)
            return new Result<bool>(new Exception($"Genre {id} not found"));

        var usedByAnyBook = await _uow.Books
            .Query()
            .AnyAsync(b => b.Genres.Any(g => g.Id == id));

        return !usedByAnyBook;
    }

    public async Task<Result<Unit>> DeleteGenreAsync(int id)
    {
        var genre = await _uow.Genres.GetByIdAsync(id);
        if (genre is null)
            return new Result<Unit>(new Exception($"Genre {id} not found"));

        var canDeleteResult = await CanDeleteGenreAsync(id);
        if (canDeleteResult.IsFaulted)
        {
            var ex = canDeleteResult.Match<Exception?>(
                Succ: _ => null,
                Fail: e => e);

            return new Result<Unit>(ex!);
        }

        var canDelete = canDeleteResult.Match(x => x, _ => false);
        if (!canDelete)
            return new Result<Unit>(new Exception("Genre cannot be deleted because it is used by at least one book."));

        _uow.Genres.Remove(genre);
        await _uow.SaveChangesAsync();
        return Unit.Default;
    }

    public async Task<Result<List<GenreSearchItemDto>>> SearchGenres(string term)
    {
        return await _uow.Genres.Query()
            .Where(g => g.Name.ToLower().Contains(term.ToLower()))
            .Select(g => new GenreSearchItemDto(g.Id, g.Name))
            .Take(GenreSearchResultLimit).ToListAsync();
       
    }

    public async Task<Result<List<GenreDto>>> GetGenresByIdsAsync(IEnumerable<int> genreIds)
    {
        return await GenreMapper.ProjectToDto(
            _uow.Genres.Query()
                .Where(g => genreIds.Contains(g.Id)))
            .ToListAsync();
    }
}
