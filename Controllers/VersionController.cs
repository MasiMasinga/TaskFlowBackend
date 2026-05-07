using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public VersionController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Version = _configuration["ApiSettings:Version"]
        });
    }
}