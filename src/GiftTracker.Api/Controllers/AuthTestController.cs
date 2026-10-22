using GiftTracker.Application.Features.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.Api.Controllers;

[ApiController]
[Route("api/auth-test")]
[Authorize]
public class AuthTestController(IUserResolutionService userResolutionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = await userResolutionService.GetOrCreateUserIdAsync(cancellationToken);

        return Ok(new { Message = "Authenticated successfully", UserId = userId });
    }
}
