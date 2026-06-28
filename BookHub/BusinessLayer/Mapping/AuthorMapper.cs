using BusinessLayer.Dto.Author;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class AuthorMapper
{
    public static AuthorDto ToDto(Author entity) =>
        EntityMapper.ToDto<Author, AuthorDto>(entity);

    public static IQueryable<AuthorDto> ProjectToDto(IQueryable<Author> query) =>
        EntityMapper.ProjectToDto<Author, AuthorDto>(query);
}