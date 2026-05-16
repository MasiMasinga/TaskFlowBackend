namespace TaskFlow.Extensions;

public static class HttpContextExtensions
{
    public static string GetClientIpAddress(this HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        if (address is null)
            return "unknown";

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        return address.ToString();
    }
}
