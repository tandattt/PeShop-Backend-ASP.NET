
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Dtos.Requests;
using PeShop.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace PeShop.Data.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly PeShopDbContext _context;
        public CartRepository(PeShopDbContext context)
        {
            _context = context;
        }
        public async Task<List<Cart>?> GetCartAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Product!)
                    .ThenInclude(p => p!.Shop)
                .Include(c => c.Variant!)
                    .ThenInclude(v => v!.VariantValues)
                        .ThenInclude(vv => vv!.PropertyValue)
                            .ThenInclude(pv => pv!.PropertyProduct)
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
        public async Task<Cart?> AddCartAsync(Cart cart,string userId)
        {
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }
        public async Task<Cart?> GetCartByIdAsync(string cartId)
        {
            return await _context.Carts
                .Include(c => c.Product!)
                    .ThenInclude(p => p!.Shop)
                .Include(c => c.Variant!)
                    .ThenInclude(v => v!.VariantValues)
                        .ThenInclude(vv => vv!.PropertyValue)
                            .ThenInclude(pv => pv!.PropertyProduct)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }

        public async Task<Cart?> UpdateCartAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<Cart?> DeleteCartAsync(string cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var carts = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
            if (carts.Any())
            {
                _context.Carts.RemoveRange(carts);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}