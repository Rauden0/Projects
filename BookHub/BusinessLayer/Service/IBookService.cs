using BusinessLayer.Dto.Book;
using BusinessLayer.Dto.GlobalSearch;
using LanguageExt.Common;
using Microsoft.AspNetCore.OData.Query;

namespace BusinessLayer.Service;

public interface IBookService
{
    public Task<Result<BookDto>> GetBook(int id);
    public Task<Result<List<BookDto>>> GetBooks(ODataQueryOptions<BookDto>? options, bool includeImages = false);
    public Task<Result<BookDto>> AddBook(RequestBookDto createBookDto);
    public Task<Result<BookDto>> UpdateBook(int id, RequestBookDto createBookDto);
    public Task<Result<BookDto>> UpdateBookStockQuantityUnsaved(int id, int stockQuantity);
    public Task DeleteBook(int id);
    public Task<Result<List<BookSearchItemDto>>> SearchBooks(string searchTerm, int? bookLimit = null);
    
    public Task<Result<List<BookDto>>> GetBooksByIdsAsync(IEnumerable<int> bookIds, bool includeImages = false);

}