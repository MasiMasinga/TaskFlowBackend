using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public HealthController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
        
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "TaskFlow.Api",
        });
    }

    [HttpGet("details")]
    public IActionResult GetHealthDetails()
    {
        DateTime startTime = System.Diagnostics.Process.GetCurrentProcess().StartTime;
        
        return Ok(new
        {
            ProcessTime = DateTime.Now - startTime,
            MachineName = Environment.MachineName,
            CurrentEnvironment = _environment.EnvironmentName,
        });
    }
}