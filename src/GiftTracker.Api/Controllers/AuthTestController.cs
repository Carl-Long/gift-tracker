using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiftTracker.Api.Controllers;

[ApiController]
[Route("api/auth-test")]
[Authorize]
public class AuthTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "Authenticated successfully", SubjectId = User.FindFirst("sub")?.Value });
    }
}
