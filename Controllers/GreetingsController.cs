using Microsoft.AspNetCore.Mvc;
using TaskFlow.Interfaces;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreetingsController : ControllerBase
{
    private readonly IGreetingService _greetingService;

    public GreetingsController(IGreetingService greetingService)
    {
        _greetingService = greetingService;
    }

    [HttpGet]
    public IActionResult GetGreetings()
    {
        return Ok(_greetingService.GetGreetings());
    }
}
