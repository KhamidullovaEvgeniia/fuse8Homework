namespace Fuse8.BackendInternship.PublicApi.Exceptions;

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