using Microsoft.AspNetCore.OData.Query;
namespace BusinessLayer.Extension;

public static class ODataExtensions
{
    public static IQueryable<T> ApplyIfNotNull<T>(
        this IQueryable<T> query,
        ODataQueryOptions<T>? options,
        ODataQuerySettings? settings = null)
        where T : class
    {
        if (options == null) return query;

        settings ??= new ODataQuerySettings
        {
            HandleNullPropagation = HandleNullPropagationOption.False
        };

        return options.ApplyTo(query, settings) as IQueryable<T> ?? query;
    }
}
