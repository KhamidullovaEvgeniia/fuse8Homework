namespace Fuse8.BackendInternship.Domain;

/// <summary>
/// Контейнер для значения, с отложенным получением
/// </summary>
public class Lazy<TValue>
{
    // ToDo: Реализовать ленивое получение значение при первом обращении к Value

    private readonly Func<TValue> _func;

    private bool _isCreated;

    private TValue? _value;

    public Lazy(Func<TValue> func)
    {
        _func = func;
    }

    public TValue? Value
    {
        get
        {
            if (!_isCreated)
            {
                _value = _func();
                _isCreated = true;
            }

            return _value;
        }
    }
}