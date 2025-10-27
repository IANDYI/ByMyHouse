using BuyMyHouse.Core.Models;

namespace BuyMyHouse.Core.Interfaces
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> FetchAllPropertiesAsync();
        Task<IEnumerable<House>> QueryByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<House?> RetrievePropertyByIdAsync(int id);
        Task ModifyPropertyAsync(House house);
    }
}