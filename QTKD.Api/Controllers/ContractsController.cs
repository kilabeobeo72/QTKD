using Microsoft.AspNetCore.Mvc;
using QTKD.Api.DTOs;
using QTKD.Api.Services;

namespace QTKD.Api.Controllers;

[ApiController]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService)
    {
        _contractService = contractService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ContractResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ContractResponseDto>>> GetAllContracts(CancellationToken cancellationToken)
    {
        var contracts = await _contractService.GetAllContractsAsync(cancellationToken);
        return Ok(contracts);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponseDto>> GetContractById(int id, CancellationToken cancellationToken)
    {
        var contract = await _contractService.GetContractByIdAsync(id, cancellationToken);
        return Ok(contract);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponseDto>> CreateContract([FromBody] CreateContractDto dto, CancellationToken cancellationToken)
    {
        var created = await _contractService.CreateContractAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetContractById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContractResponseDto>> UpdateContract(int id, [FromBody] UpdateContractDto dto, CancellationToken cancellationToken)
    {
        var updated = await _contractService.UpdateContractAsync(id, dto, cancellationToken);
        return Ok(updated);
    }

    [HttpPost("{id:int}/submit")]
    [ProducesResponseType(typeof(SubmitContractResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmitContractResponseDto>> SubmitContract(int id, CancellationToken cancellationToken)
    {
        var result = await _contractService.SubmitContractAsync(id, cancellationToken);
        return Ok(result);
    }
}
