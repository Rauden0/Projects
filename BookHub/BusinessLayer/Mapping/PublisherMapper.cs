using BusinessLayer.Dto.Publisher;
using DataAccessLayer.Models;

namespace BusinessLayer.Mapping;

public static class PublisherMapper
{
    public static PublisherDto ToDto(Publisher entity) =>
        EntityMapper.ToDto<Publisher, PublisherDto>(entity);

    public static IQueryable<PublisherDto> ProjectToDto(IQueryable<Publisher> query) =>
        EntityMapper.ProjectToDto<Publisher, PublisherDto>(query);
}