using PeShop.Data.Repositories.Interfaces;
using PeShop.Data.Contexts;
using PeShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace PeShop.Data.Repositories
{
    public class ImageProductRepository : IImageProductRepository
    {
        private readonly PeShopDbContext _context;
        public ImageProductRepository(PeShopDbContext context)
        {
            _context = context;
        }
        public async Task<List<ImageProduct>> GetListImageProductByProductIdAsync(string productId)
        {
            return await _context.ImageProducts.Where(x => x.ProductId == productId).OrderBy(x => x.SortOrder).AsNoTracking().ToListAsync();
        }
    }
}