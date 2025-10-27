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

        public async Task<IEnumerable<House>> GetAllAsync()
        {
            return await _context.Houses
            .Where(h => h.IsAvailable)
            .OrderByDescending(h => h.ListedDate)
            .ToListAsync();
        }
        public async Task<IEnumerable<House>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Houses
            .Where(h => h.IsAvailable && h.Price >= minPrice && h.Price <= maxPrice)
            .OrderBy(h => h.Price)
            .ToListAsync();
        }
        public async Task<House?> GetByIdAsync(int id)
        {
            return await _context.Houses.FindAsync(id);
        }

        public async Task UpdateAsync(House house)
        {
            _context.Houses.Update(house);
            await _context.SaveChangesAsync();
        }
    }
}