using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Models.Entities;
using PeShop.Exceptions;
using PeShop.Dtos.Shared;
namespace PeShop.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IVariantValueRepository _variantValueRepository;
        public CartService(ICartRepository cartRepository, IVariantValueRepository variantValueRepository)
        {
            _cartRepository = cartRepository;
            _variantValueRepository = variantValueRepository;
        }
        public async Task<List<CartResponse>> GetCartAsync(string userId)
        {
            var carts = await _cartRepository.GetCartAsync(userId) ?? new List<Cart>();
            
            var cartResponses = new List<CartResponse>();
            
            foreach (var cart in carts)
            {
                var cartResponse = new CartResponse
                {
                    CartId = cart.Id,
                    ShopId = cart.Product?.ShopId ?? string.Empty,
                    ShopName = cart.Product?.Shop?.Name ?? string.Empty,
                    Price = cart.Price ?? 0,
                    Quantity = cart.Quantity ?? 0,
                    ProductId = cart.ProductId ?? string.Empty,
                    ProductName = cart.Product?.Name ?? string.Empty,
                    ProductImage = cart.Product?.ImgMain ?? string.Empty,
                    VariantId = cart.VariantId?.ToString() ?? string.Empty,
                    VariantValues = GetVariantValues(cart.Variant)
                };
                
                cartResponses.Add(cartResponse);
            }
            
            return cartResponses;
        }
        
        private List<VariantValueForCartDto> GetVariantValues(Variant? variant)
        {
            var variantValues = new List<VariantValueForCartDto>();
            
            if (variant?.VariantValues == null || !variant.VariantValues.Any())
                return variantValues;
            
            foreach (var variantValue in variant.VariantValues)
            {
                if (variantValue?.PropertyValue == null) continue;
                
                var variantValueForCart = new VariantValueForCartDto
                {
                    PropertyValue = new PropertyValueForCartDto
                    {
                        Value = variantValue.PropertyValue.Value ?? string.Empty,
                        ImgUrl = variantValue.PropertyValue.ImgUrl ?? string.Empty,
                        Level = variantValue.PropertyValue.Level
                    },
                    Property = new PropertyForCartDto
                    {
                        Name = variantValue.PropertyValue.PropertyProduct?.Name ?? string.Empty
                    }
                };
                
                variantValues.Add(variantValueForCart);
            }
                
            return variantValues;
        }
        public async Task<Dictionary<string, int>> AddCartAsync(CartRequest request, string userId)
        {
            var existingCarts = await _cartRepository.GetCartAsync(userId) ?? new List<Cart>();
            Cart? existingCart=null;
            if(request.VariantId != null){  
                 // thêm vào giỏ hàng có variant
                Console.WriteLine("thêm vào giỏ hàng có variant");
                existingCart = existingCarts.FirstOrDefault(c => 
                c.ProductId == request.ProductId && c.VariantId == request.VariantId);
                
            }
            else{
               // thêm vào giỏ hàng mà không có variant
                Console.WriteLine("thêm vào giỏ hàng mà không có variant");
                existingCart = existingCarts.FirstOrDefault(c => 
                c.ProductId == request.ProductId);
            }
            
            if (existingCart != null)
            {
                existingCart.Quantity = (existingCart.Quantity ?? 0) + request.Quantity;
                existingCart.UpdatedAt = DateTime.UtcNow;
                
                await _cartRepository.UpdateCartAsync(existingCart);
            }
            else
            {
                var newCart = new Cart
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = request.ProductId,
                    VariantId = request.VariantId ?? null,
                    Quantity = request.Quantity,
                    Price = request.Price,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await _cartRepository.AddCartAsync(newCart, userId);
            }            
            var allUserCarts = await _cartRepository.GetCartAsync(userId) ?? new List<Cart>();
            var totalQuantity = allUserCarts.Sum(c => c.Quantity ?? 0);
            
            return new Dictionary<string, int> 
            { 
                { "totalCart", totalQuantity } 
            };
        }
        public async Task<CartResponse> UpdateCartAsync(string cartId, int quantity, string userId)
        {
            var cart = await _cartRepository.GetCartByIdAsync(cartId);
            if (cart == null)
            {
                throw new NotFoundException("Sản phẩm không tồn tại trong giỏ hàng");
            }

            if (cart.UserId != userId)
            {
                throw new ForBidenException("Bạn không có quyền cập nhật sản phẩm này");
            }

            if (quantity <= 0)
            {
                throw new BadRequestException("Số lượng phải lớn hơn 0");
            }

            cart.Quantity = quantity;
            cart.UpdatedAt = DateTime.UtcNow;

            var updatedCart = await _cartRepository.UpdateCartAsync(cart);
            if (updatedCart == null)
            {
                throw new BadRequestException("Cập nhật giỏ hàng thất bại");
            }

            return new CartResponse
            {
                ShopId = updatedCart.Product?.ShopId ?? string.Empty,
                ShopName = updatedCart.Product?.Shop?.Name ?? string.Empty,
                Slug = updatedCart.Product?.Slug ?? string.Empty,
                Price = updatedCart.Price ?? 0,
                Quantity = updatedCart.Quantity ?? 0,
                ProductId = updatedCart.ProductId ?? string.Empty,
                ProductName = updatedCart.Product?.Name ?? string.Empty,
                ProductImage = updatedCart.Product?.ImgMain ?? string.Empty,
                VariantId = updatedCart.VariantId?.ToString() ?? string.Empty,
                VariantValues = GetVariantValues(updatedCart.Variant)
            };
        }

        public async Task<string> DeleteCartAsync(string cartId, string userId)
        {
            var cart = await _cartRepository.GetCartByIdAsync(cartId);
            if (cart == null)
            {
                throw new NotFoundException("Sản phẩm không tồn tại trong giỏ hàng");
            }

            if (cart.UserId != userId)
            {
                throw new ForBidenException("Bạn không có quyền xóa sản phẩm này");
            }

            var result = await _cartRepository.DeleteCartAsync(cartId);
            if (result == null)
            {
                throw new BadRequestException("Xóa sản phẩm khỏi giỏ hàng thất bại");
            }

            return "Xóa sản phẩm khỏi giỏ hàng thành công";
        }

        public async Task<string> ClearCartAsync(string userId)
        {
            var result = await _cartRepository.ClearCartAsync(userId);
            if (!result)
            {
                throw new BadRequestException("Xóa giỏ hàng thất bại");
            }

            return "Xóa toàn bộ giỏ hàng thành công";
        }

        public async Task<Dictionary<string, int>> GetCartCountAsync(string userId)
        {
            var carts = await _cartRepository.GetCartAsync(userId) ?? new List<Cart>();
            var totalQuantity = carts.Sum(c => c.Quantity ?? 0);
            var totalItems = carts.Count;

            return new Dictionary<string, int>
            {
                { "totalQuantity", totalQuantity },
                { "totalItems", totalItems }
            };
        }
    }
}