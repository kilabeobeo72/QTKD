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

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PriceListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceListDto>> GetPriceListById(int id, CancellationToken cancellationToken)
    {
        var priceList = await _priceListService.GetPriceListByIdAsync(id, cancellationToken);
        if (priceList == null)
        {
            return NotFound();
        }
        return Ok(priceList);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(PriceListDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PriceListDto>> CreatePriceList([FromBody] CreatePriceListDto createDto, CancellationToken cancellationToken)
    {
        var created = await _priceListService.CreatePriceListAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetPriceListById), new { id = created.Id }, created);
    }
    
    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(PriceItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceItemDto>> AddItemToPriceList(int id, [FromBody] CreatePriceItemDto createDto, CancellationToken cancellationToken)
    {
        var added = await _priceListService.AddPriceItemAsync(id, createDto, cancellationToken);
        return CreatedAtAction(nameof(GetPriceListById), new { id }, added);
    }
}
