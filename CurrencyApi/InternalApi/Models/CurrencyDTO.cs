using General.Enums;

namespace InternalApi.Models;

public record CurrencyDTO(CurrencyType CurrencyType, decimal Value);