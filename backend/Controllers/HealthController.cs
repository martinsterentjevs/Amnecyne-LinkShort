using Microsoft.AspNetCore.Mvc;

namespace Amnecyne.LinkShort.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealthStatus()
    {
        return Ok(new { status = "Healthy" });
    }
}