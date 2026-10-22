using Asp.Versioning;
using GiftTracker.Application.Features.OccasionTypes.Dtos;
using GiftTracker.Application.Features.OccasionTypes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/occasion-types")]
public class OccasionTypesController(IOccasionTypeService occasionTypeService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OccasionTypeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OccasionTypeDto>>> GetAllOccasionTypesAsync(
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var occasionTypes = await occasionTypeService.GetOccasionTypesAsync(includeArchived, cancellationToken);
        return Ok(occasionTypes);
    }

    [HttpGet("{id:guid}", Name = nameof(GetOccasionTypeById))]
    [ProducesResponseType(typeof(OccasionTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OccasionTypeDto>> GetOccasionTypeById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var occasionType = await occasionTypeService.GetOccasionTypeByIdAsync(id, cancellationToken);
        return Ok(occasionType);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OccasionTypeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OccasionTypeDto>> CreateOccasionType(
        [FromBody] CreateOccasionTypeDto createOccasionTypeDto,
        CancellationToken cancellationToken)
    {
        var occasionType = await occasionTypeService.CreateOccasionTypeAsync(createOccasionTypeDto, cancellationToken);
        var version = HttpContext.RequestedApiVersion?.ToString();
        return CreatedAtAction(nameof(GetOccasionTypeById), new { version, id = occasionType.Id }, occasionType);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OccasionTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OccasionTypeDto>> UpdateOccasionType(
        [FromRoute] Guid id,
        [FromBody] UpdateOccasionTypeDto updateOccasionTypeDto,
        CancellationToken cancellationToken)
    {
        var occasionType =
            await occasionTypeService.UpdateOccasionTypeAsync(id, updateOccasionTypeDto, cancellationToken);
        return Ok(occasionType);
    }

    [HttpPatch("{id:guid}")]
    [EndpointSummary("Update occasion type archive status")]
    [EndpointDescription("Set isArchived to true to archive the occasion type, or false to restore it.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOccasionTypeArchiveStatus(
        Guid id,
        [FromBody] UpdateOccasionTypeArchiveStatusDto updateOccasionTypeArchiveStatusDto,
        CancellationToken cancellationToken)
    {
        await occasionTypeService.SetOccasionTypeArchivedStatusAsync(
            id,
            updateOccasionTypeArchiveStatusDto.IsArchived,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOccasionType(Guid id, CancellationToken cancellationToken)
    {
        await occasionTypeService.DeleteOccasionTypeAsync(id, cancellationToken);
        return NoContent();
    }
}
