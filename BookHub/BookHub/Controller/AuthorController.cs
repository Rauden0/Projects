using BusinessLayer.Dto.Author;
using BusinessLayer.Service;
using LanguageExt.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/authors")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _service;
    public AuthorsController(IAuthorService service) => _service = service;

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuthor(int id)
    {
        var item = await _service.GetAuthorByIdAsync(id);
        return item.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AuthorDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetAuthors([FromQuery] ODataQueryOptions<AuthorDto>? options)
    {
        var authors = await _service.GetAuthorsAsync(options);
        return authors.Match<IActionResult>(Ok, ex => NotFound(ex.Message)
        );
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDto))]
    public async Task<IActionResult> AddAuthor([FromBody] AddAuthorDto dto)
    {
        var created = await _service.AddAuthor(dto);
        return created.Match<IActionResult>(
            Ok,
            ex => BadRequest(ex.Message)
            );
    }

    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthorDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAuthorDto dto)
    {
        var author = await _service.UpdateAuthor(id, dto);
        return author.Match<IActionResult>(Ok, ex => NotFound(ex.Message));
    }


    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _service.DeleteAuthorAsync(id);

        return res.Match<IActionResult>(
            _ => NoContent(),
            ex =>
            {
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ex.Message);

                if (ex.Message.Contains("used by at least one book", StringComparison.OrdinalIgnoreCase))
                    return Conflict(ex.Message);

                return BadRequest(ex.Message);
            }
        );
    }
}
