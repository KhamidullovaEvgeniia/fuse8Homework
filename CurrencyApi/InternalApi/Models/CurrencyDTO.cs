using General.Enums;

namespace InternalApi.Models;

public sealed record CurrencyDTO(CurrencyType CurrencyType, decimal Value);