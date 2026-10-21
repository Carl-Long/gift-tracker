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

    [HttpGet("{id:guid}", Name = nameof(GetRecipientById))]
    [ProducesResponseType(typeof(RecipientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipientDto>> GetRecipientById(Guid id, CancellationToken cancellationToken)
    {
        var recipient = await recipientService.GetRecipientByIdAsync(id, cancellationToken);
        return Ok(recipient);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecipientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecipientDto>> CreateRecipient([FromBody] CreateRecipientDto createRecipientDto, CancellationToken cancellationToken)
    {
        var recipient = await recipientService.CreateRecipientAsync(createRecipientDto, cancellationToken);
        var version = HttpContext.RequestedApiVersion?.ToString();
        return CreatedAtAction(nameof(GetRecipientById), new { version, id = recipient.Id }, recipient);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RecipientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipientDto>> UpdateRecipient(Guid id, [FromBody] UpdateRecipientDto updateRecipientDto, CancellationToken cancellationToken)
    {
        var recipient = await recipientService.UpdateRecipientAsync(id, updateRecipientDto, cancellationToken);
        return Ok(recipient);
    }

    [HttpPatch("{id:guid}")]
    [EndpointSummary("Update recipient archive status")]
    [EndpointDescription("Set isArchived to true to archive the recipient, or false to restore it.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRecipientArchiveStatus(Guid id, [FromBody] UpdateRecipientArchiveStatusDto updateRecipientArchiveStatusDto, CancellationToken cancellationToken)
    {
        await recipientService.SetRecipientArchivedStatusAsync(id, updateRecipientArchiveStatusDto.IsArchived, cancellationToken);
        return NoContent();
    }
}
