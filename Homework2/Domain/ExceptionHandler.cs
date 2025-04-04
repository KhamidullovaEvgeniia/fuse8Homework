using System.Net;

namespace Fuse8.BackendInternship.Domain;

public static class ExceptionHandler
{
    /// <summary>
    /// Обрабатывает исключение, которое может возникнуть при выполнении <paramref name="action"/>
    /// </summary>
    /// <param name="action">Действие, которое может породить исключение</param>
    /// <returns>Сообщение об ошибке</returns>
    public static string? Handle(Action action)
    {
        // ToDo: Реализовать обработку исключений

        string? errorMessage = null;
        try
        {
            action();
        }
        catch (HttpRequestException httpRequestException) when (httpRequestException.StatusCode == HttpStatusCode.NotFound)
        {
            errorMessage = "Ресурс не найден";
        }
        catch (HttpRequestException httpRequestException)
        {
            errorMessage = httpRequestException.StatusCode.ToString();
        }
        catch (MoneyException exception)
        {
            errorMessage = exception.Message;
        }

        catch (Exception)
        {
            return "Произошла непредвиденная ошибка";
        }

        return errorMessage;
    }
}

public class MoneyException : Exception
{
    public MoneyException()
    {
    }

    public MoneyException(string? message) : base(message)
    {
    }
}

public class NotValidKopekCountException : MoneyException
{
    private const string DefaultMessage = "Количество копеек должно быть больше 0 и меньше 99";

    public NotValidKopekCountException() : base(DefaultMessage)
    {
    }
}

public class NegativeRubleCountException : MoneyException
{
    private const string DefaultMessage = "Число рублей не может быть отрицательным";

    public NegativeRubleCountException() : base(DefaultMessage)
    {
    }
}