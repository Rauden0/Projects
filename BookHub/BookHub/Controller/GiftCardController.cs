using BusinessLayer.Dto.GiftCard;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace BookHub.Controller;

[ApiController]
[Route("/giftcards")]
public class GiftCardController : ControllerBase
{
    private readonly IGiftCardService _giftCardService;

    public GiftCardController(IGiftCardService giftCardService)
    {
        _giftCardService = giftCardService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<GiftCardDto>))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetGiftCards([FromQuery] ODataQueryOptions<GiftCardDto>? options)
    {
        var giftCards = await _giftCardService.GetGiftCards(options);
        return giftCards.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GiftCardDto))]
    public async Task<IActionResult> CreateGiftCard([FromBody] GiftCardCreateDto giftCardCreateDto)
    {
        var giftCard = await _giftCardService.CreateGiftCard(giftCardCreateDto);
        return giftCard.Match<IActionResult>(
            Ok,
            ex => BadRequest(ex.Message)
        );
    }

    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GiftCardDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGiftCardByCode(int id)
    {
        var giftCard = await _giftCardService.GetGiftCard(id);
        return giftCard.Match<IActionResult>(
            Ok,
            ex => NotFound(ex.Message)
        );
    }

    [HttpDelete]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteGiftCard(int id)
    {
        await _giftCardService.DeleteGiftCard(id);
        return NoContent();
    }
    

}