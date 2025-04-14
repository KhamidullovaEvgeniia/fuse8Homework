using Fuse8.BackendInternship.PublicApi.Settings;

namespace Fuse8.BackendInternship.PublicApi.Helpers;

public static class RoundHelper
{
    public static decimal RoundCurrencyValue(decimal value, int accuracy) => Math.Round(value, accuracy);
}