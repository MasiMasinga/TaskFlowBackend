using TaskFlow.Models;
namespace TaskFlow.Interfaces;

public interface ISystemInfoService
{
    SystemInfo GetSystemInfo();
    TimeSpan GetUptime();
}
