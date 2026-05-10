using System.Diagnostics;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Services;

public class SystemInfoService : ISystemInfoService
{
    private readonly IClockService _clockService;

    public SystemInfoService(IClockService clockService)
    {
        _clockService = clockService;
    }

    public SystemInfo GetSystemInfo()
    {
        return new SystemInfo(
            MachineName: Environment.MachineName,
            OsVersion: Environment.OSVersion.ToString(),
            ProcessorCount: Environment.ProcessorCount,
            DotNetVersion: Environment.Version.ToString(),
            ServerTimeUtc: _clockService.GetClock().UtcNow
        );
    }

    public TimeSpan GetUptime()
    {
        var startTime = Process.GetCurrentProcess().StartTime;
        return DateTime.Now - startTime;
    }
}
