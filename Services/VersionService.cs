using TaskFlow.Interfaces;
using TaskFlow.Models;
namespace TaskFlow.Services;

public class VersionService : IVersionService
{
    public ApiSettings GetVersion()
    {
        return new ApiSettings(
            Version: Environment.GetEnvironmentVariable("API_SETTINGS_VERSION") ?? "1.0.0"
        );
    }
}
