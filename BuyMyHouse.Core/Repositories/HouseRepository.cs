using BuyMyHouse.Core.Data;
using BuyMyHouse.Core.Models;
using BuyMyHouse.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BuyMyHouse.Core.Repositories
{
    public class HouseRepository : IHouseRepository
    {
        private readonly HouseDbContext _context;
        public HouseRepository(HouseDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<House>> FetchAllPropertiesAsync()
        {
            return await _context.Houses
            .Where(h => h.CurrentlyAvailable)
            .OrderByDescending(h => h.DateListed)
            .ToListAsync();
        }
        public async Task<IEnumerable<House>> QueryByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Houses
            .Where(h => h.CurrentlyAvailable && h.ListingPrice >= minPrice && h.ListingPrice <= maxPrice)
            .OrderBy(h => h.ListingPrice)
            .ToListAsync();
        }
        public async Task<House?> RetrievePropertyByIdAsync(int id)
        {
            return await _context.Houses.FindAsync(id);
        }

        public async Task ModifyPropertyAsync(House house)
        {
            _context.Houses.Update(house);
            await _context.SaveChangesAsync();
        }
    }
}