using BusinessLayer.Dto.Book;
using BusinessLayer.Service.Logging;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
namespace BookHub.Controller;

[ApiController]
[Route("/books")]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly IAuditLogService _auditLogService;

    public BookController(IBookService service, IAuditLogService auditLogService)
    {
        _bookService = service;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _bookService.GetBook(id);
        return book.Match<IActionResult>(
            Ok,
        ex => NotFound(ex.Message)
            );
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookDto>))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetBooks([FromQuery] ODataQueryOptions<BookDto>? options)
    {
        var books = await _bookService.GetBooks(options);
        return books.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
    public async Task<IActionResult> AddBook([FromForm] RequestBookDto requestBookDto)
    {
        var book = await _bookService.AddBook(requestBookDto);
        return book.Match<IActionResult>(
            Ok,
            ex => BadRequest(ex.Message)
        );
    }

    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBook(int id, [FromForm] RequestBookDto requestBookDto)
    {
        var book = await _bookService.UpdateBook(id, requestBookDto);
        
        var user = User.FindFirst("Id")?.Value ?? "unknown";
        await _auditLogService.LogAsync(user, "Book", id.ToString(), "Edit", book.IsSuccess.ToString());
        
        return book.Match<IActionResult>( 
            Ok, 
            ex => NotFound(ex.Message)
        );
    }

    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBook(int id)
    {
        await _bookService.DeleteBook(id);
        return NoContent();
    }
}