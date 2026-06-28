using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Extensions.Logging;

namespace BusinessLayer.Mapping;

public static class EntityMapper
{
    private static readonly ILoggerFactory LoggerFactoryInstance =
        LoggerFactory.Create(builder => builder.AddFilter("AutoMapper", LogLevel.Error));

    public static readonly MapperConfiguration Config =
        new(cfg =>
        {
            cfg.AddMaps(typeof(EntityMapper).Assembly);
        }, LoggerFactoryInstance);

    private static readonly IMapper Mapper = Config.CreateMapper();

    public static TDto ToDto<TEntity, TDto>(TEntity entity)
        where TEntity : class
        where TDto : class, new() =>
        Mapper.Map<TDto>(entity);

    public static IQueryable<TDto> ProjectToDto<TEntity, TDto>(IQueryable<TEntity> query)
        where TEntity : class
        where TDto : class, new() =>
        query.ProjectTo<TDto>(Config);
}