using BuyMyHouse.Core.Models;

namespace BuyMyHouse.Core.Interfaces
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> GetAllAsync();
        Task<IEnumerable<House>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<House?> GetByIdAsync(int id);
        Task UpdateAsync(House house);
    }
}