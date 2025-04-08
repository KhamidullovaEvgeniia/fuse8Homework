using Fuse8.BackendInternship.PublicApi.Settings;

namespace Fuse8.BackendInternship.PublicApi.Helpers;

public static class CurrencyHelper
{
    public static decimal RoundCurrencyValue(double value, int accuracy) => Math.Round((decimal)value, accuracy);
}