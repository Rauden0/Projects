using BusinessLayer.Dto.Genre;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class GenreMapper
{
    public static GenreDto ToDto(Genre entity) =>
        EntityMapper.ToDto<Genre, GenreDto>(entity);

    public static IQueryable<GenreDto> ProjectToDto(IQueryable<Genre> query) =>
        EntityMapper.ProjectToDto<Genre, GenreDto>(query);
}