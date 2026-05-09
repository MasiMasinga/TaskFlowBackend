using Microsoft.AspNetCore.Mvc;
using TaskFlow.Interfaces;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController : ControllerBase
{
    private readonly ISystemInfoService _systemInfo;

    public SystemController(ISystemInfoService systemInfo)
    {
        _systemInfo = systemInfo;
    }

    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        return Ok(_systemInfo.GetSystemInfo());
    }

    [HttpGet("uptime")]
    public IActionResult GetUptime()
    {
        var uptime = _systemInfo.GetUptime();
        return Ok(new { uptime = uptime.ToString(@"dd\.hh\:mm\:ss") });
    }
}
