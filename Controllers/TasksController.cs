using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksControllerController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok();
    }
}
