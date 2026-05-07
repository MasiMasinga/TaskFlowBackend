using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;
using Microsoft.Extensions.Options;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    private readonly IOptions<ApiSettings> _apiSettings;

    public VersionController(IOptions<ApiSettings> apiSettings)
    {
        _apiSettings = apiSettings;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Version = _apiSettings.Value.Version
        });
    }
}