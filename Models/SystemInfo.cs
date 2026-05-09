namespace TaskFlow.Models;

public record SystemInfo(
    string MachineName,
    string OsVersion,
    int ProcessorCount,
    string DotNetVersion,
    DateTime ServerTimeUtc
);