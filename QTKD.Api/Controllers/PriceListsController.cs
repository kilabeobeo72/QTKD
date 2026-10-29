using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/pricelists")]
public class PriceListsController : ControllerBase
{
    private readonly IPriceListService _priceListService;

    public PriceListsController(IPriceListService priceListService)
    {
        _priceListService = priceListService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<PriceListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PriceListDto>>> GetPriceLists(CancellationToken cancellationToken)
    {
        var priceLists = await _priceListService.GetAllPriceListsAsync(cancellationToken);
        return Ok(priceLists);
    }
}
