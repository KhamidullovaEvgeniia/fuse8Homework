﻿namespace General.Exceptions;

public class CurrencyNotFoundException : Exception
{
    public CurrencyNotFoundException()
    {
    }

    public CurrencyNotFoundException(string message)
        : base(message)
    {
    }

    public CurrencyNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}