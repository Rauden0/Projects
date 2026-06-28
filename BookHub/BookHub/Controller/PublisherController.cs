using BusinessLayer.Dto.Publisher;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/publishers")]
public class PublishersController : ControllerBase
{
    private readonly IPublisherService _service;
    public PublishersController(IPublisherService service) => _service = service;

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PublisherDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublisherAsync(int id)
    {
        var publisher = await _service.GetPublisherAsync(id);
        return publisher.Match<IActionResult>(Ok, ex => NotFound(ex.Message));
    }

    [HttpGet]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetPublishers([FromQuery] ODataQueryOptions<PublisherDto>? options)
    {
        var publishers = await _service.GetPublishers(options);
        return publishers.Match<IActionResult>(Ok, ex => NotFound(ex.Message)
        );
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PublisherDto))]
    public async Task<IActionResult> CreateAsync([FromBody] CreatePublisherDto dto)
    {
        var publisher = await _service.CreateAsync(dto);
        return Ok(publisher);
    }

    [HttpPut]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PublisherDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePublisherDto dto)
    {
        var publisher = await _service.UpdateAsync(id, dto);
        return publisher.Match<IActionResult>(Ok, ex => NotFound(ex.Message));
    }

    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
