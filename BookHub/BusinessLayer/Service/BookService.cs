using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.GlobalSearch;
using BusinessLayer.Extension;
using BusinessLayer.Mapping;
using DataAccessLayer;
using DataAccessLayer.Models;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;


namespace BusinessLayer.Service;

public class BookService : IBookService
{
    private readonly IUnitOfWork _uow;
    private readonly IImageService _imageService;
    private const int BookSearchResultLimit = 5;
    
    public BookService(IUnitOfWork uow, IImageService imageService)
    {
        _uow = uow;
        _imageService = imageService;
    }

    public async Task<Result<BookDto>> GetBook(int id)
    {
        var book = await _uow.Books.GetBookByIdwithAuthorAndGenre(id);
        if (book is null)
            return new Result<BookDto>(new Exception($"Book {id} not found"));
        var dto = BookMapper.ToDto(book);
        if (!string.IsNullOrEmpty(book.ImagePath))
        {
            dto.ImageBase64 = await _imageService.GetImageAsBase64Async(book.ImagePath);
        }
        return dto;
    }

    public async Task<Result<List<BookDto>>> GetBooks(ODataQueryOptions<BookDto>? options, bool includeImages = false)
    {
        var books = await BookMapper.ProjectToDto(_uow.Books.Query())
            .ApplyIfNotNull(options)
            .ToListAsync();

        if (includeImages)
        {
            await Task.WhenAll(books.Select(async dto => 
            {
                dto.ImageBase64 = await _imageService.GetImageAsBase64Async(dto.ImagePath);
            }));
        }

        return books;
    }

    public async Task<Result<BookDto>> AddBook(RequestBookDto requestBookDto)
    {
        var authors = await _uow.Books.GetAuthorsByListIds(requestBookDto.AuthorIds);
        var genres = await _uow.Books.GetGenresByListIds(requestBookDto.GenreIds);

        if (authors.Count == 0)
            return new Result<BookDto>(new Exception("Authors do not exist"));

        if (genres.Count == 0)
            return new Result<BookDto>(new Exception("Genres do not exist"));
        
        if (requestBookDto.PrimaryGenreId != null)
        {
            var primaryGenre = genres.FirstOrDefault(g => g.Id == requestBookDto.PrimaryGenreId);
            if (primaryGenre == null)
            {
                return new Result<BookDto>(new Exception("Primary genre must be one of the selected genres"));
            }
        }


        var book = new Book
        {
            Authors = authors,
            Genres = genres,
            PublisherId = requestBookDto.PublisherId,
            Name = requestBookDto.Name,
            Description = requestBookDto.Description,
            Price = requestBookDto.Price,
            StockQuantity = requestBookDto.StockQuantity,
            PrimaryGenreId = requestBookDto.PrimaryGenreId
        };

        string? uploadedPath = null;

        try
        {
            if (requestBookDto.Image != null)
            {
                using var stream = requestBookDto.Image.OpenReadStream();
                var extension = Path.GetExtension(requestBookDto.Image.FileName);
                var fileName = $"book_{Guid.NewGuid()}{extension}";
            
                uploadedPath = await _uow.Images.UploadAsync(stream, "images/books", fileName);
                book.ImagePath = uploadedPath;
            }

            _uow.Books.Add(book);
            await _uow.SaveChangesAsync();
            return BookMapper.ToDto(book);
        }
        catch (Exception ex)
        {
            if (uploadedPath != null)
            {
                _uow.Images.Delete(uploadedPath);
            }
            return new Result<BookDto>(ex);
        }
    }

    public async Task<Result<BookDto>> UpdateBook(int id, RequestBookDto requestBookDto)
    {
        var book = await _uow.Books.GetBookByIdwithAuthorAndGenre(id);
        if (book is null)
        {
            return new Result<BookDto>(new Exception($"Book {id} not found"));
        }

        var authors = await _uow.Books.GetAuthorsByListIds(requestBookDto.AuthorIds);
        var genres = await _uow.Books.GetGenresByListIds(requestBookDto.GenreIds);

        if (authors.Count == 0)
            return new Result<BookDto>(new Exception($"Authors not exist"));

        if (genres.Count == 0)
            return new Result<BookDto>(new Exception($"Genres not exist"));

        book.Authors.Clear();
        book.Genres.Clear();

        foreach (var author in authors)
            book.Authors.Add(author);

        foreach (var genre in genres)
            book.Genres.Add(genre);

        book.PublisherId = requestBookDto.PublisherId;
        book.Name = requestBookDto.Name;
        book.Description = requestBookDto.Description;
        book.Price = requestBookDto.Price;
        book.StockQuantity = requestBookDto.StockQuantity;
        book.PrimaryGenreId = requestBookDto.PrimaryGenreId;

        _uow.Books.Update(book);

        if (requestBookDto.Image != null)
        {  
            if (!string.IsNullOrEmpty(book.ImagePath))
            {
                _imageService.DeleteBookImageAsync(book.ImagePath);
            }
            book.ImagePath = await _imageService.SaveBookImageAsync(requestBookDto.Image, book.Name);
        }
        
        await _uow.SaveChangesAsync();
        return BookMapper.ToDto(book);
    }

    public async Task<Result<BookDto>> UpdateBookStockQuantityUnsaved(int id, int stockQuantity)
    {
        var book = await _uow.Books.GetBookByIdwithAuthorAndGenre(id);
        if (book is null)
        {
            return new Result<BookDto>(new Exception($"Book {id} not found"));
        }
        
        book.StockQuantity = stockQuantity;

        _uow.Books.Update(book);
        return BookMapper.ToDto(book);
    }

    public async Task DeleteBook(int id)
    {
        await _uow.ExecuteInTransactionAsync(async () =>
        {
            var book = await _uow.Books.GetByIdAsync(id);
            if (book != null)
                _uow.Books.Remove(book);
            await _uow.SaveChangesAsync();
        });
    }

    public async Task<Result<List<BookSearchItemDto>>> SearchBooks(string searchTerm, int? bookLimit = null)
    {
        return await _uow.Books.Query()
            .Where(b => b.Name.ToLower().Contains(searchTerm.ToLower()))
            .Select(b => new BookSearchItemDto(b.Id, b.Name, b.ImagePath))
            .Take(bookLimit ?? BookSearchResultLimit).ToListAsync();

    }

    public async Task<Result<List<BookDto>>> GetBooksByIdsAsync(IEnumerable<int> bookIds, bool includeImages = false)
    {
        var books = await BookMapper.ProjectToDto(
            _uow.Books
                .Query()
                .Where(b => bookIds.Contains(b.Id)))
                .ToListAsync();
        
        if (includeImages)
        {
            await Task.WhenAll(books.Select(async dto => 
            {
                dto.ImageBase64 = await _imageService.GetImageAsBase64Async(dto.ImagePath);
            }));
        }
        
        return books;
    }
}