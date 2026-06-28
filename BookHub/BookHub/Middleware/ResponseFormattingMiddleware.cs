using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace BookHub.Middleware;

public class JsonOrXmlOutputFormatter : IOutputFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
        }
    };

    private static readonly ConcurrentDictionary<Type, XmlSerializer> XmlSerializerCache = new();

    public bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        return true; // Handle all object types
    }

    public async Task WriteAsync(OutputFormatterWriteContext context)
    {
        var httpContext = context.HttpContext;

        var formatQuery = httpContext.Request.Query["format"].ToString().ToLowerInvariant();
        var acceptHeader = httpContext.Request.Headers["Accept"].ToString().ToLowerInvariant();
        var wantsXml = formatQuery == "xml" || acceptHeader.Contains("application/xml");

        httpContext.Response.ContentType = wantsXml
            ? "application/xml; charset=UTF-8"
            : "application/json; charset=UTF-8";

        var obj = context.Object;
  
        Task writeTask = wantsXml switch
        {
            true => WriteXml(httpContext, obj),
            false => WriteJson(httpContext, obj)
        };

        await writeTask;
    }

    private async Task  WriteXml(HttpContext httpContext, object? obj)
    {
        if (obj == null)
        {
            await httpContext.Response.WriteAsync(@"<?xml version=""1.0"" encoding=""UTF-8""?><response />", Encoding.UTF8);
            return;
        }

        var serializer = XmlSerializerCache.GetOrAdd(obj.GetType(), t => new XmlSerializer(t));

        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8,
            OmitXmlDeclaration = false,
            Async = true
        };

        await using var xmlStream = httpContext.Response.BodyWriter.AsStream();
        await using var xmlWriter = XmlWriter.Create(xmlStream, settings);
        serializer.Serialize(xmlWriter, obj);
    }
    
    private async Task WriteJson(HttpContext httpContext, object? obj)
    {
        var json = JsonSerializer.Serialize(obj, JsonOptions);
        await httpContext.Response.WriteAsync(json, Encoding.UTF8);
    }
}
