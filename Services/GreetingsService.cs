using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Services;

public class GreetingsService : IGreetingService
{
    public Greetings GetGreetings()
    {
        return new Greetings(
            name: "Hello, Masibonge Masinga!"
        );
    }
}
