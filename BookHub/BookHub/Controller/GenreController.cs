using BusinessLayer.Dto.Genre;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/genres")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _service;
    public GenresController(IGenreService service) => _service = service;

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenreDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGenreAsync(int id)
    {
        var genre = await _service.GetGenreAsync(id);
        return genre.Match<IActionResult>(Ok, ex => NotFound(ex.Message));
    }

    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetGenres([FromQuery] ODataQueryOptions<GenreDto>? options)
    {
        var genres = await _service.GetGenres(options);
        return genres.Match<IActionResult>(Ok, ex => NotFound(ex.Message));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenreDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateGenreDto dto)
    {
        var genre = await _service.CreateAsync(dto);
        return genre.Match<IActionResult>(
            Ok,
            ex => BadRequest(ex.Message)
        );
    }

    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenreDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGenreDto dto)
    {
        var genre = await _service.UpdateAsync(id, dto);
        return genre.Match<IActionResult>(Ok, ex => NotFound(ex.Message));
    }

    [HttpGet]
    [Route("{id:int}/can-delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CanDelete(int id)
    {
        var res = await _service.CanDeleteGenreAsync(id);

        return res.Match<IActionResult>(
            Succ: can => Ok(can),
            Fail: ex => NotFound(ex.Message)
        );
    }

    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var res = await _service.DeleteGenreAsync(id);

        return res.Match<IActionResult>(
            Succ: _ => NoContent(),
            Fail: ex =>
            {
                var msg = ex.Message ?? "";

                if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(msg);

                if (msg.Contains("cannot be deleted", StringComparison.OrdinalIgnoreCase)
                    || msg.Contains("used by", StringComparison.OrdinalIgnoreCase))
                    return Conflict(msg);

                return BadRequest(msg);
            }
        );
    }
}
