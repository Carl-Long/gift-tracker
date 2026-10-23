using Asp.Versioning;
using GiftTracker.Application.Features.Occasions.Dtos;
using GiftTracker.Application.Features.Occasions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/occasions")]
[Authorize]
public class OccasionsController(IOccasionService occasionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OccasionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OccasionSummaryDto>>> GetOccasions(
        [FromQuery] bool includeArchived,
        CancellationToken cancellationToken)
    {
        var occasions = await occasionService.GetOccasionsAsync(includeArchived, cancellationToken);
        return Ok(occasions);
    }

    [HttpGet("{id:guid}", Name = nameof(GetOccasionById))]
    [ProducesResponseType(typeof(OccasionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OccasionDto>> GetOccasionById(Guid id, CancellationToken cancellationToken)
    {
        var occasion = await occasionService.GetOccasionByIdAsync(id,  cancellationToken);
        return Ok(occasion);
    }
}
