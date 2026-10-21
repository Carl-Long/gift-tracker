using Asp.Versioning;
using GiftTracker.Application.Features.Recipients.Dtos;
using GiftTracker.Application.Features.Recipients.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/recipients")]
public class RecipientsController(IRecipientService recipientService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RecipientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecipientDto>>> GetRecipients([FromQuery] bool includeArchived = false, CancellationToken cancellationToken = default)
    {
        var recipients = await recipientService.GetRecipientsAsync(includeArchived, cancellationToken);
        return Ok(recipients);
    }
}
