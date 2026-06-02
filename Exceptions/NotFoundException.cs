namespace TaskFlow.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string resource, object key)
        : base($"{resource} with id '{key}' was not found.")
    {
        Resource = resource;
        Key = key;
    }

    public string Resource { get; }
    public object Key { get; }
}