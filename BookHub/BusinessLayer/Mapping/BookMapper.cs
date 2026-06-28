using BusinessLayer.Dto.Book;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class BookMapper
{
    public static BookDto ToDto(Book entity) =>
        EntityMapper.ToDto<Book, BookDto>(entity);

    public static IQueryable<BookDto> ProjectToDto(IQueryable<Book> query) =>
        EntityMapper.ProjectToDto<Book, BookDto>(query);
}