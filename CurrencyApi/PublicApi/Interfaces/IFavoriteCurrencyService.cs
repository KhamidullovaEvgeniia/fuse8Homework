using Fuse8.BackendInternship.PublicApi.Models;

namespace Fuse8.BackendInternship.PublicApi.Interfaces;

public interface IFavoriteCurrencyService
{
   Task<FavoriteCurrencyRateDTO> GetFavoriteCurrencyRateByNameAsync(string name); 
   Task<FavoriteCurrencyRateDTO[]> GetAllFavoriteCurrencyRatesAsync(); 
   Task AddFavoriteCurrencyRateAsync(FavoriteCurrencyRateDTO currencyRateDTO);
   Task UpdateFavoriteCurrencyRateAsync(FavoriteCurrencyRateDTO rateDto);
   Task DeleteFavoriteCurrencyRateByNameAsync(string name);
}