using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookHub.Extension;

public class ODataQueryDocumentFilter<T>(string path, string tag) : IDocumentFilter
    where T : class
{
    private readonly string _path = path.StartsWith("/") ? path : "/" + path; // normalize path

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (!swaggerDoc.Paths.ContainsKey(_path))
        {
            swaggerDoc.Paths[_path] = new OpenApiPathItem();
        }

        var pathItem = swaggerDoc.Paths[_path];

        if (!pathItem.Operations.ContainsKey(OperationType.Get))
        {
            pathItem.Operations[OperationType.Get] = new OpenApiOperation
            {
                Tags = new List<OpenApiTag> { new() { Name = tag } },
                Summary = $"Get {tag} with OData query options",
                Description = "Supports $filter, $orderby, $top, $skip",
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = $"List of {typeof(T).Name}",
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["application/json"] = new()
                            {
                                Schema = context.SchemaGenerator.GenerateSchema(typeof(List<T>), context.SchemaRepository)
                            },
                            ["application/xml"] = new()
                            {
                                Schema = context.SchemaGenerator.GenerateSchema(typeof(CollectionWrapper<T>), context.SchemaRepository)
                            }
                        }
                    }
                },
                Parameters = new List<OpenApiParameter>
                {
                    new() { Name = "$filter", In = ParameterLocation.Query, Required = false, Schema = new OpenApiSchema { Type = "string" } },
                    new() { Name = "$orderby", In = ParameterLocation.Query, Required = false, Schema = new OpenApiSchema { Type = "string" } },
                    new() { Name = "$top", In = ParameterLocation.Query, Required = false, Schema = new OpenApiSchema { Type = "integer", Format = "int32" } },
                    new() { Name = "$skip", In = ParameterLocation.Query, Required = false, Schema = new OpenApiSchema { Type = "integer", Format = "int32" } },
                }
            };
        }
    }
}
