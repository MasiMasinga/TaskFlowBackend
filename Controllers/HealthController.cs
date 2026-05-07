using Microsoft.AspNetCore.Mvc;

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
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "TaskFlow.Api"
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