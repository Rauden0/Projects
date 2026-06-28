using BusinessLayer.Dto.GlobalSearch;
using BusinessLayer.Dto.Publisher;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Service;

public class PublisherService : IPublisherService
{
    private readonly IUnitOfWork _uow;
    private const int PublisherSearchResultLimit = 10;

    public PublisherService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<PublisherDto>> GetPublisherAsync(int id)
    {
        var publisher = await _uow.Publishers.GetByIdAsync(id);
        return publisher is not null
            ? PublisherMapper.ToDto(publisher)
            : new Result<PublisherDto>(new Exception($"Publisher {id} not found"));
    }

    public async Task<Result<List<PublisherDto>>> GetPublishers(ODataQueryOptions<PublisherDto>? options)
    {
        return await PublisherMapper.ProjectToDto(_uow.Publishers.Query())
            .ApplyIfNotNull(options)
            .ToListAsync();
    }

    public async Task<Result<PublisherDto>> CreateAsync(CreatePublisherDto req)
    {
        var publisher = new Publisher { Name = req.Name };
        _uow.Publishers.Add(publisher);
        await _uow.SaveChangesAsync();
        return PublisherMapper.ToDto(publisher);
    }

    public async Task<Result<PublisherDto>> UpdateAsync(int id, UpdatePublisherDto req)
    {
        var publisher = await _uow.Publishers.GetByIdAsync(id);
        if (publisher is null)
            return new Result<PublisherDto>(new Exception($"Publisher {id} not found"));

        publisher.Name = req.Name;
        _uow.Publishers.Update(publisher);
        await _uow.SaveChangesAsync();
        return PublisherMapper.ToDto(publisher);
    }

    public async Task<Result<bool>> CanDeleteAsync(int id)
    {
        var publisher = await _uow.Publishers.GetByIdAsync(id);
        if (publisher is null)
            return new Result<bool>(new Exception($"Publisher {id} not found"));

        var usedByAnyBook = await _uow.Books
            .Query()
            .AnyAsync(b => b.PublisherId == id);

        return !usedByAnyBook;
    }

    public async Task<Result<Unit>> DeleteAsync(int id)
    {
        var publisher = await _uow.Publishers.GetByIdAsync(id);
        if (publisher is null)
            return new Result<Unit>(new Exception($"Publisher {id} not found"));

        var canDeleteResult = await CanDeleteAsync(id);

        if (canDeleteResult.IsFaulted)
        {
            var ex = canDeleteResult.Match<Exception?>(
                Succ: _ => null,
                Fail: e => e);

            return new Result<Unit>(ex!);
        }

        var canDelete = canDeleteResult.Match(x => x, _ => false);
        if (!canDelete)
            return new Result<Unit>(new Exception("Publisher cannot be deleted because it is used by at least one book."));

        _uow.Publishers.Remove(publisher);
        await _uow.SaveChangesAsync();
        return Unit.Default;
    }

    public async Task<Result<List<PublisherSearchItemDto>>> SearchPublishers(string searchTerm)
    {
        return await _uow.Publishers.Query()
            .Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()))
            .Select(p => new PublisherSearchItemDto(p.Id, p.Name))
            .Take(PublisherSearchResultLimit)
            .ToListAsync();
    }
}
