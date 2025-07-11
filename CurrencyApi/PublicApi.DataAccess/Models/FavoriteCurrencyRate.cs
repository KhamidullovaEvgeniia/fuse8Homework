﻿namespace PublicApi.DataAccess.Models;

public class FavoriteCurrencyRate
{
    public int Id { get; set; }

    public required string Name { get; init; }

    public required string Currency { get; set; }

    public required string BaseCurrency { get; set; }
}