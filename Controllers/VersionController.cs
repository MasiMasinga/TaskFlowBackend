using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VersionController : ControllerBase
{
    private readonly IVersionService _versionService;

    public VersionController(IVersionService versionService)
    {
        _versionService = versionService;
    }

    [HttpGet]
    public IActionResult GetVersion()
    {
        return Ok(_versionService.GetVersion());
    }
}