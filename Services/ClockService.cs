using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Services;

public class ClockService : IClockService
{
    public Clock GetClock()
    {
        return new Clock(
            UtcNow: DateTime.UtcNow
        );
    }
}
