namespace Framework.Exceptions;

public class ApiRequestLimitException : Exception
{
    public ApiRequestLimitException()
    {
    }

    public ApiRequestLimitException(string message) : base(message)
    {
    }

    public ApiRequestLimitException(string message, Exception innerException) : base(message, innerException)
    {
    }
}