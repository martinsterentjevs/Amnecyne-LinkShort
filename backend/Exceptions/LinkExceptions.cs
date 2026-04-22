namespace Amnecyne.LinkShort.Exceptions;

public class LinkTakenException : Exception
{
    public LinkTakenException(string message) : base(message)
    {
    }
}