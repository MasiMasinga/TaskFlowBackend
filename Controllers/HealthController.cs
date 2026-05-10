using Microsoft.AspNetCore.Mvc;
using TaskFlow.Interfaces;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;
    
    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
    }

    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(_healthService.GetHealth());
    }

    [HttpGet("details")]
    public IActionResult GetHealthDetails()
    {
        return Ok(_healthService.GetHealthDetails());
    }
}