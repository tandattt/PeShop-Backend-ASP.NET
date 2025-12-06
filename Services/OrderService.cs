namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
using System.Text.Json;
using Hangfire;
using System.Linq;
using PeShop.Models.Entities;
using PeShop.Models.Enums;
using Microsoft.IdentityModel.Tokens;
using PeShop.Data.Repositories;
using PeShop.Utilities;
using Microsoft.AspNetCore.Hosting;
public class OrderService : IOrderService
{
    private readonly IOrderHelper _orderHelper;
    private readonly IVoucherRepository _voucherRepository;
    private readonly IRedisUtil _redisUtil;
    private readonly IUserAddressRepository _userAddressRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IPlatformFeeRepository _platformFeeRepository;
    private readonly IPayoutRepository _payoutRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IVariantRepository _variantRepository;
    private readonly IPromotionService _promotionService;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IReviewService _reviewService;
    private readonly IUserRepository _userRepository;
    private readonly IJobService _jobService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFlashSaleRepository _flashSaleRepository;
    private readonly IGHNUtil _ghnUtil;
    private readonly IShopRepository _shopRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        IOrderHelper orderHelper,
        IRedisUtil redisUtil,
        IVoucherRepository voucherRepository,
        IUserAddressRepository userAddressRepository,
        IOrderRepository orderRepository,
        IOrderDetailRepository orderDetailRepository,
        IPlatformFeeRepository platformFeeRepository,
        IPayoutRepository payoutRepository,
        ITransactionRepository transactionRepository,
        IVariantRepository variantRepository,
        IPromotionService promotionService,
        IPromotionRepository promotionRepository,
        IReviewService reviewService,
        IUserRepository userRepository,
        IJobService jobService,
        IWebHostEnvironment webHostEnvironment,
        IFlashSaleRepository flashSaleRepository,
        IGHNUtil ghnUtil,
        IShopRepository shopRepository,
        IProductRepository productRepository
        )
    {
        _orderHelper = orderHelper;
        _redisUtil = redisUtil;
        _voucherRepository = voucherRepository;
        _userAddressRepository = userAddressRepository;
        _orderRepository = orderRepository;
        _orderDetailRepository = orderDetailRepository;
        _platformFeeRepository = platformFeeRepository;
        _payoutRepository = payoutRepository;
        _transactionRepository = transactionRepository;
        _variantRepository = variantRepository;
        _promotionService = promotionService;
        _promotionRepository = promotionRepository;
        _reviewService = reviewService;
        _userRepository = userRepository;
        _jobService = jobService;
        _webHostEnvironment = webHostEnvironment;
        _flashSaleRepository = flashSaleRepository;
        _ghnUtil = ghnUtil;
        _shopRepository = shopRepository;
        _productRepository = productRepository;
    }

    // Helper class để flatten data
    private class OrderProductItem
    {
        public ItemShop Shop { get; set; } = null!;
        public OrderRequest Product { get; set; } = null!;
    }

    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<(int VariantId, uint Quantity)> VariantUpdates { get; set; } = new();
        public List<(string FlashSaleProductId, uint Quantity)> FlashSaleUpdates { get; set; } = new();
    }
    public async Task<CreateVirtualOrderResponse> CreateVirtualOrder(OrderVirtualRequest request, string userId)
    {
        try
        {
            // SECURITY: Reset FlashSale fields từ Frontend request
            // Backend sẽ tự động tính lại, không tin tưởng data từ Frontend
            foreach (var item in request.Items)
            {
                item.FlashSaleProductId = null;
                item.FlashSalePercentDecrease = null;
                item.FlashSalePrice = null;
                item.PriceOriginal = 0; // Sẽ được tính lại
                item.CategoryId = string.Empty; // Sẽ được fill lại
            }

            // Nhóm các sản phẩm theo shop (Backend tự động check và apply FlashSale)
            var itemShops = await _orderHelper.GroupItemsByShopAsync(request.Items);

            // Xác định HasFlashSale ở cấp Order dựa trên việc có ít nhất một shop có FlashSaleDiscount > 0
            bool hasFlashSale = itemShops.Any(shop => shop.FlashSaleDiscount > 0);

            var userAddress = await _userAddressRepository.GetUserAddressByIdAsync(request.UserAddressId, userId);
            
            // Fallback: Nếu RecipientName trống, lấy tên từ User
            string recipientName = userAddress?.RecipientName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(recipientName))
            {
                var user = await _userRepository.GetByIdAsync(userId);
                recipientName = user?.Name ?? user?.Username ?? "Khách hàng";
            }
            
            // Tạo đơn hàng ảo
            var order = new OrderVirtualDto
            {
                OrderId = Guid.NewGuid().ToString(),
                ItemShops = itemShops,
                // OrderTotal = orderTotal,
                // AmountTotal = orderTotal,
                UserId = userId,
                RecipientName = recipientName,
                RecipientPhone = userAddress?.Phone ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UserFullNewAddress = userAddress?.FullNewAddress ?? string.Empty,
                HasFlashSale = hasFlashSale,
                // Address details for GHN shipping (using Old address - 3 levels)
                ToWardCode = userAddress?.OldWardId,
                ToDistrictId = !string.IsNullOrEmpty(userAddress?.OldDistrictId) ? int.Parse(userAddress.OldDistrictId) : null,
                StreetLine = userAddress?.StreetLine
            };

            // Lưu vào Redis
            bool isSaved = await _redisUtil.SetAsync($"order_{userId}_{order.OrderId}", JsonSerializer.Serialize(order));
            // Xóa đơn hàng sau 60 phút (không block nếu Hangfire lỗi)
            try
            {
                BackgroundJob.Schedule<IJobService>(service => service.DeleteOrderOnRedisAsync(order.OrderId, userId, false), TimeSpan.FromMinutes(60));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Failed to schedule background job: {ex.Message}");
            }
            if (isSaved)
            {
                return new CreateVirtualOrderResponse
                {
                    Status = true,
                    Order = order,
                    Message = "Đơn hàng đã được tạo thành công"
                };
            }
            else
            {
                return new CreateVirtualOrderResponse
                {
                    Status = false,
                    Message = "Lỗi khi lưu đơn hàng vào Redis"
                };
            }
        }
        catch (Exception ex)
        {
            return new CreateVirtualOrderResponse
            {
                Status = false,
                Message = $"Lỗi khi tạo đơn hàng: {ex.Message}"
            };
        }
    }
    public async Task<CreateVirtualOrderResponse> UpdateVirtualOrder(UpdateVirtualOrderRequest request, string userId)
    {
        try
        {
            // Lấy order hiện tại từ Redis
            var existingOrder = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{request.OrderId}");
            if (existingOrder == null)
            {
                return new CreateVirtualOrderResponse
                {
                    Status = false,
                    Message = "Đơn hàng không tồn tại"
                };
            }

            // SECURITY: Reset FlashSale fields từ Frontend request
            // Backend sẽ tự động tính lại, không tin tưởng data từ Frontend
            foreach (var item in request.Items)
            {
                item.FlashSaleProductId = null;
                item.FlashSalePercentDecrease = null;
                item.FlashSalePrice = null;
                item.PriceOriginal = 0;
                item.CategoryId = string.Empty;
            }

            // Nhóm các sản phẩm mới theo shop (Backend tự động check và apply FlashSale)
            var itemShops = await _orderHelper.GroupItemsByShopAsync(request.Items);

            // Xác định HasFlashSale ở cấp Order dựa trên việc có ít nhất một shop có FlashSaleDiscount > 0
            bool hasFlashSale = itemShops.Any(shop => shop.FlashSaleDiscount > 0);

            // Tính tổng giá trị đơn hàng mới
            decimal orderTotal = itemShops.Sum(shop => shop.PriceOriginal);

            // Cập nhật order với dữ liệu mới
            existingOrder.ItemShops = itemShops;
            existingOrder.OrderTotal = orderTotal;
            existingOrder.AmountTotal = orderTotal;
            existingOrder.FeeShippingTotal = 0; // Reset shipping fee
            existingOrder.HasFlashSale = hasFlashSale; // Cập nhật HasFlashSale khi update order

            // Lưu lại vào Redis
            bool isSaved = await _redisUtil.SetAsync($"order_{userId}_{existingOrder.OrderId}", JsonSerializer.Serialize(existingOrder));
            if (!isSaved)
            {
                return new CreateVirtualOrderResponse
                {
                    Status = false,
                    Message = "Lỗi khi cập nhật đơn hàng vào Redis"
                };
            }

            // Gọi service check promotion để cập nhật promotion
            await _promotionService.CheckPromotionsInOrderAsync(existingOrder.OrderId, userId);

            // Gọi service tính order để cập nhật lại giá trị
            var calculateResult = await CalclulateOrderTotal(existingOrder.OrderId, userId);

            return calculateResult;
        }
        catch (Exception ex)
        {
            return new CreateVirtualOrderResponse
            {
                Status = false,
                Message = $"Lỗi khi cập nhật đơn hàng: {ex.Message}"
            };
        }
    }
    public async Task<StatusResponse> DeleteVirtualOrder(string orderId, string userId)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{orderId}");
        if (order == null)
        {
            return new StatusResponse { Status = false, Message = "Đơn hàng không tồn tại" };
        }
        await _jobService.DeleteOrderOnRedisAsync(orderId, userId, false);
        return new StatusResponse { Status = true, Message = "Đơn hàng đã được xóa thành công" };
    }
    public async Task<CreateVirtualOrderResponse> CalclulateOrderTotal(string orderId, string userId)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{orderId}");
        if (order == null)
        {
            return new CreateVirtualOrderResponse
            {
                Status = false,
                Message = "Đơn hàng không tồn tại"
            };
        }
        order.OrderTotal = order.ItemShops.Sum(x => x.PriceOriginal);
        order.FeeShippingTotal = order.ItemShops.Sum(x => x.FeeShipping);
        order.FlashSaleDiscountTotal = order.ItemShops.Sum(x => x.FlashSaleDiscount);
        var Promotion = await _redisUtil.GetAsync<List<PromotionInOrderResponse>>($"promotion_in_order_{userId}_{orderId}");
        if (Promotion == null)
        {
            Promotion = new List<PromotionInOrderResponse>();
        }

        foreach (var itemShop in order.ItemShops)
        {
            foreach (var promotion in Promotion)
            {
                if (promotion.ShopId == itemShop.ShopId)
                {
                    if (promotion.Products.IsNullOrEmpty() == true)
                    {
                        // Add tất cả gifts với PromotionGiftId
                        if (promotion.PromotionGiftsList != null && promotion.PromotionGiftsList.Any())
                        {
                            foreach (var gift in promotion.PromotionGiftsList)
                            {
                                itemShop.Gifts.Add(new GiftInOrder
                                {
                                    Product = gift.Product != null ? new OrderRequest
                                    {
                                        ProductId = gift.Product.Id,
                                        Quantity = (uint)gift.GiftQuantity,
                                        PriceOriginal = 0,
                                        ShopId = itemShop.ShopId
                                    } : null,
                                    PromotionName = promotion.PromotionName,
                                    PromotionId = promotion.PromotionId,
                                    PromotionGiftId = gift.Id
                                });
                            }
                        }
                        // Backward compatibility với PromotionGifts (single gift)
                        else if (promotion.PromotionGifts != null)
                        {
                            itemShop.Gifts.Add(new GiftInOrder
                            {
                                Product = promotion.PromotionGifts.Product != null ? new OrderRequest
                                {
                                    ProductId = promotion.PromotionGifts.Product.Id,
                                    Quantity = (uint)promotion.PromotionGifts.GiftQuantity,
                                    PriceOriginal = 0,
                                    ShopId = itemShop.ShopId
                                } : null,
                                PromotionName = promotion.PromotionName,
                                PromotionId = promotion.PromotionId,
                                PromotionGiftId = promotion.PromotionGifts.Id
                            });
                        }
                    }
                }
            }
            // itemShop.PriceAfterVoucher = itemShop.PriceOriginal - 0;
            if (itemShop.VoucherId == null) continue;

            var voucherShop = await _voucherRepository.GetVoucherShopByIdAsync(itemShop.VoucherId);
            if (voucherShop.Type == VoucherValueType.Percentage)
            {
                decimal discountAmount = itemShop.PriceOriginal * (voucherShop.DiscountValue ?? 0) > (voucherShop.MaxdiscountAmount ?? 0) ? (voucherShop.MaxdiscountAmount ?? 0) : itemShop.PriceOriginal * (voucherShop.DiscountValue ?? 0);
                // itemShop.PriceAfterVoucher = itemShop.PriceOriginal - discountAmount;
                order.DiscountTotal = order.DiscountTotal + discountAmount;
            }
            else if (voucherShop.Type == VoucherValueType.FixedAmount)
            {
                order.DiscountTotal = order.DiscountTotal + voucherShop.DiscountValue ?? 0;
            }
        }

        if (order.VoucherSystemId != null)
        {
            var voucherSystem = await _voucherRepository.GetVoucherSystemByIdAsync(order.VoucherSystemId);
            if (voucherSystem.Type == VoucherValueType.Percentage)
            {
                decimal discountAmount = order.OrderTotal * (voucherSystem.DiscountValue ?? 0) > (voucherSystem.MaxdiscountAmount ?? 0) ? (voucherSystem.MaxdiscountAmount ?? 0) : order.OrderTotal * (voucherSystem.DiscountValue ?? 0);
                order.DiscountTotal = order.DiscountTotal + discountAmount;
            }
            else if (voucherSystem.Type == VoucherValueType.FixedAmount)
            {
                order.DiscountTotal = order.DiscountTotal + voucherSystem.DiscountValue ?? 0;
            }
        }
        order.AmountTotal = order.OrderTotal + order.FeeShippingTotal - order.DiscountTotal;
        await _redisUtil.SetAsync($"calculated_order_{userId}_{orderId}", JsonSerializer.Serialize(order));
        return new CreateVirtualOrderResponse
        {
            Status = true,
            Order = order,
            Message = "Đơn hàng đã được tính toán thành công"
        };
    }
    public async Task<StatusResponse> CreateOrderCODAsync(string orderId, string userId)
    {
        // Console.WriteLine($"[DEBUG] Starting CreateOrderCODAsync for OrderId: {orderId}, UserId: {userId}");

        var orders = await _redisUtil.GetAsync<OrderVirtualDto>($"calculated_order_{userId}_{orderId}");
        if (orders == null)
        {
            // Console.WriteLine($"[DEBUG] Order not found in Redis for OrderId: {orderId}");
            return new StatusResponse { Status = false, Message = "Đơn hàng không tồn tại" };
        }

        // Console.WriteLine($"[DEBUG] Order found in Redis, starting transaction");
        var result = await SaveOrderAsync(orders, userId, PaymentStatus.Unpaid, PaymentMethod.COD);
        if (result.Status)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new StatusResponse { Status = false, Message = "Người dùng không tồn tại" };
            }
            try
            {
                BackgroundJob.Enqueue<IJobService>(service => service.DeleteOrderOnRedisAsync(orderId, userId, false));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Failed to enqueue delete job: {ex.Message}");
            }
            var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Template", "PaymentSuccess.html");
            Console.WriteLine($"[OrderService] ContentRootPath: {_webHostEnvironment.ContentRootPath}");
            Console.WriteLine($"[OrderService] TemplatePath: {templatePath}");
            Console.WriteLine($"[OrderService] File exists: {File.Exists(templatePath)}");
            string htmlBody;
            if (File.Exists(templatePath))
            {
                htmlBody = await File.ReadAllTextAsync(templatePath);
                htmlBody = htmlBody.Replace("{CustomerName}", orders.RecipientName);
                htmlBody = htmlBody.Replace("{RecipientName}", orders.RecipientName);
                htmlBody = htmlBody.Replace("{RecipientPhone}", orders.RecipientPhone ?? "");
                htmlBody = htmlBody.Replace("{ShippingAddress}", orders.UserFullNewAddress);
                htmlBody = htmlBody.Replace("{OrderDate}", DateTime.UtcNow.ToString("dd/MM/yyyy"));
                htmlBody = htmlBody.Replace("{PaymentMethod}", "COD");
                htmlBody = htmlBody.Replace("{TotalAmount}", orders.AmountTotal.ToString("N0") + " VNĐ");
                htmlBody = htmlBody.Replace("{SubTotal}", orders.OrderTotal.ToString("N0") + " VNĐ");
                htmlBody = htmlBody.Replace("{ShippingFee}", orders.FeeShippingTotal.ToString("N0") + " VNĐ");
                htmlBody = htmlBody.Replace("{Discount}", orders.DiscountTotal.ToString("N0") + " VNĐ");
                htmlBody = htmlBody.Replace("{Items}", ""); // Tạm thời để trống vì cần thông tin sản phẩm chi tiết
                htmlBody = htmlBody.Replace("{OrderTrackingUrl}", "#"); // URL tracking sẽ được thêm sau khi có BaseUrlFrontend
                try
                {
                    BackgroundJob.Enqueue<IEmailUtil>(service => service.SendEmailAsync(user.Email, "Đơn hàng đã được tạo thành công", htmlBody, true));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Warning] Failed to enqueue email job: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[OrderService] Template file not found at: {templatePath}");
            }
        }
        return result;
    }
    public async Task<StatusResponse<List<string>>> SaveOrderAsync(OrderVirtualDto orders, string userId, PaymentStatus paymentStatus, PaymentMethod paymentMethod)
    {
        try
        {
            var result = await _transactionRepository.ExecuteInTransactionAsync(async () =>
            {
                List<string> createdOrderIds = new List<string>();

                // BƯỚC 0: Flatten data - Chỉ loop 1 lần
                var allProducts = orders.ItemShops
                    .SelectMany(shop => shop.Products.Select(p => new OrderProductItem
                    {
                        Shop = shop,
                        Product = p
                    }))
                    .ToList();

                // BƯỚC 1: Collect tất cả IDs cần validate
                var variantIds = allProducts
                    .Where(x => x.Product.VariantId.HasValue)
                    .Select(x => x.Product.VariantId!.Value)
                    .Distinct()
                    .ToList();

                var flashSaleProductIds = allProducts
                    .Where(x => !string.IsNullOrEmpty(x.Product.FlashSaleProductId))
                    .Select(x => x.Product.FlashSaleProductId!)
                    .Distinct()
                    .ToList();

                var productIds = allProducts
                    .Where(x => !string.IsNullOrEmpty(x.Product.FlashSaleProductId))
                    .Select(x => x.Product.ProductId)
                    .Distinct()
                    .ToList();

                // BƯỚC 2: Batch query - Giảm database calls
                var variants = await _variantRepository.GetVariantsByIdsAsync(variantIds);
                var flashSaleProducts = flashSaleProductIds.Any()
                    ? await _flashSaleRepository.GetFlashSaleProductsByIdsAsync(flashSaleProductIds)
                    : new Dictionary<string, FlashSaleProduct>();
                var activeFlashSales = productIds.Any()
                    ? await _flashSaleRepository.GetActiveFlashSaleProductsAsync(productIds)
                    : new Dictionary<string, FlashSaleProduct>();

                // BƯỚC 3: Validate tất cả trong 1 loop
                var validationResult = await ValidateOrderProducts(
                    allProducts,
                    variants,
                    flashSaleProducts,
                    activeFlashSales,
                    userId);

                if (!validationResult.IsValid)
                {
                    throw new Exception(validationResult.ErrorMessage);
                }

                // BƯỚC 4: Batch decrease quantities (atomic operations)
                await BatchDecreaseQuantitiesAsync(validationResult);

                // BƯỚC 5: Tạo Orders - Group lại theo shop
                Console.WriteLine($"[DEBUG] Total shops in order: {orders.ItemShops.Count}");
                foreach (var itemShop in orders.ItemShops)
                {
                    decimal platformFeeTotal = 0;
                    // Xác định HasFlashSale dựa trên việc có product nào có FlashSaleProductId != null
                    bool shopHasFlashSale = itemShop.Products.Any(p => !string.IsNullOrEmpty(p.FlashSaleProductId));

                    Console.WriteLine($"[DEBUG] Processing shop: {itemShop.ShopId} - {itemShop.ShopName}");
                    
                    // Sử dụng thông tin shop từ Redis (đã được cache trong ItemShop)
                    if (itemShop.ShopGHNId == null)
                    {
                        throw new Exception($"Shop chưa đăng ký GHN: {itemShop.ShopId}");
                    }

                    // Tính tổng cân nặng, kích thước và tạo items từ thông tin đã cache trong Redis
                    int totalWeight = 0;
                    int maxLength = 0;
                    int maxWidth = 0;
                    int maxHeight = 0;
                    var ghnItems = new List<PeShop.Dtos.GHN.GHNOrderItem>();
                    foreach (var p in itemShop.Products)
                    {
                        var productWeight = (int)(p.ProductWeight ?? 200); // Default 200g nếu không có
                        var productLength = (int)(p.ProductLength ?? 20); // Default 20cm
                        var productWidth = (int)(p.ProductWidth ?? 20); // Default 20cm
                        var productHeight = (int)(p.ProductHeight ?? 10); // Default 10cm
                        
                        totalWeight += productWeight * (int)p.Quantity;
                        // Lấy kích thước lớn nhất trong các sản phẩm
                        if (productLength > maxLength) maxLength = productLength;
                        if (productWidth > maxWidth) maxWidth = productWidth;
                        maxHeight += productHeight * (int)p.Quantity; // Cộng dồn chiều cao
                        
                        ghnItems.Add(new PeShop.Dtos.GHN.GHNOrderItem
                        {
                            name = p.ProductName ?? p.ProductId,
                            quantity = (int)p.Quantity,
                            weight = productWeight
                        });
                    }

                    // Sử dụng from_district_id từ cache
                    int fromDistrictId = itemShop.ShopDistrictId ?? 0;

                    // Gọi service để lấy service_id
                    var serviceRequest = new PeShop.Dtos.GHN.GetServiceRequest
                    {
                        shop_id = (int)itemShop.ShopGHNId,
                        from_district = fromDistrictId,
                        to_district = orders.ToDistrictId ?? 0
                    };
                    var serviceResponse = await _ghnUtil.GetServiceAsync(serviceRequest);
                    var serviceId = serviceResponse?.data?.FirstOrDefault(x => x.short_name == "Hàng nhẹ")?.service_id ?? 0;

                    // Validate to_name - GHN yêu cầu bắt buộc
                    string toName = orders.RecipientName;
                    if (string.IsNullOrWhiteSpace(toName))
                    {
                        // Fallback: Lấy tên từ User nếu RecipientName trống
                        var user = await _userRepository.GetByIdAsync(userId);
                        toName = user?.Name ?? user?.Username ?? "Khách hàng";
                    }

                    // Tạo đơn hàng trên GHN để lấy OrderCode (sử dụng data từ Redis)
                    var ghnRequest = new PeShop.Dtos.GHN.GHNCreateOrderRequest
                    {
                        ShopId = (int)itemShop.ShopGHNId,
                        payment_type_id = paymentMethod == PaymentMethod.COD ? 2 : 1, // 2 = COD, 1 = Đã thanh toán
                        note = "",
                        required_note = "KHONGCHOXEMHANG",
                        from_name = itemShop.ShopName ?? "",
                        from_phone = itemShop.ShopPhone ?? "",
                        from_address = itemShop.ShopAddress ?? "",
                        from_ward_name = "",
                        from_district_name = "",
                        from_province_name = "",
                        to_name = toName,
                        to_phone = orders.RecipientPhone ?? "",
                        to_address = orders.StreetLine ?? orders.UserFullNewAddress,
                        to_ward_code = orders.ToWardCode ?? "",
                        to_district_id = orders.ToDistrictId ?? 0,
                        cod_amount = paymentMethod == PaymentMethod.COD ? (int)(itemShop.PriceOriginal + itemShop.FeeShipping - itemShop.VoucherValue) : 0,
                        service_id = serviceId,
                        service_type_id = 2, // Hàng nhẹ
                        length = maxLength > 0 ? maxLength : 20, // Chiều dài lớn nhất
                        width = maxWidth > 0 ? maxWidth : 20, // Chiều rộng lớn nhất
                        height = maxHeight > 0 ? maxHeight : 10, // Tổng chiều cao
                        weight = totalWeight > 0 ? totalWeight : 200, // Tổng cân nặng thực tế
                        items = ghnItems
                    };
                    Console.WriteLine(JsonSerializer.Serialize(ghnRequest));
                    var ghnResponse = await _ghnUtil.CreateOrderAsync(ghnRequest);
                    if (ghnResponse == null || ghnResponse.code != 200)
                    {
                        throw new Exception($"Lỗi khi tạo đơn hàng GHN: {ghnResponse?.message ?? "Unknown error"}");
                    }

                    var ghnOrderCode = ghnResponse.data.order_code;

                    var order = new Order
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = userId,
                        DiscountPrice = orders.VoucherSystemValue + itemShop.VoucherValue,
                        SystemVoucherDiscount = orders.VoucherSystemValue,
                        ShopVoucherDiscount = itemShop.VoucherValue,
                        OrderCode = ghnOrderCode, // Sử dụng OrderCode từ GHN
                        ShippingFee = itemShop.FeeShipping,
                        DeliveryAddress = orders.UserFullNewAddress,
                        DeliveryStatus = DeliveryStatus.Ready_To_Pick,
                        StatusOrder = OrderStatus.Pending,
                        ShopId = itemShop.ShopId,
                        UserId = userId,
                        OriginalPrice = itemShop.PriceOriginal,
                        FinalPrice = itemShop.PriceOriginal + itemShop.FeeShipping - itemShop.VoucherValue,
                        PaymentMethod = paymentMethod,
                        StatusPayment = paymentStatus,
                        HasFlashSale = shopHasFlashSale
                    };
                    // Console.WriteLine($"[DEBUG] Creating Order for ShopId: {itemShop.ShopId}");
                    var orderDB = await _orderRepository.CreateOrderAsync(order);
                    if (orderDB == null)
                    {
                        // Console.WriteLine($"[ERROR] Failed to create Order for ShopId: {itemShop.ShopId}");
                        throw new Exception("Lỗi khi tạo đơn hàng");
                    }
                    // Console.WriteLine($"[DEBUG] Order created successfully with ID: {orderDB.Id}");

                    createdOrderIds.Add(orderDB.Id);

                    // Xử lý voucher shop
                    if (itemShop.VoucherId != null)
                    {
                        await ProcessShopVoucherAsync(itemShop.VoucherId, userId, orderDB.Id);
                    }

                    // Xử lý promotion gifts
                    if (itemShop.Gifts?.Any() == true)
                    {
                        await ProcessPromotionGiftsAsync(itemShop.Gifts, orderDB.Id, userId);
                    }
                    // foreach (var product in itemShop.Products)
                    // {
                    //     Id = Guid.NewGuid().ToString(),
                    //     OrderId = orderDB.Id,
                    //     ProductId = gift.Product?.ProductId,
                    //     OriginalPrice = gift.Product?.PriceOriginal ?? 0,
                    //     Quantity = gift.Product?.Quantity ?? 0,
                    //     VariantId = gift.Product?.VariantId,
                    //     Note = gift.Product?.Note,
                    //     CreatedAt = DateTime.UtcNow,
                    //     CreatedBy = userId,
                    //     UpdatedAt = DateTime.UtcNow,
                    //     UpdatedBy = userId
                    // };
                    // var orderDetailDB = await _orderDetailRepository.CreateOrderDetailAsync(orderDetail);
                    // if (orderDetailDB == null)
                    // {
                    //     throw new Exception("Lỗi khi tạo đơn hàng");
                    // }
                    // Console.WriteLine($"[DEBUG] Creating OrderDetail for ProductId: {gift.Product?.ProductId}");
                    // }
                    // }
                    foreach (var product in itemShop.Products)
                    {
                        var orderDetail = new OrderDetail
                        {
                            Id = Guid.NewGuid().ToString(),
                            OrderId = orderDB.Id,
                            ProductId = product.ProductId,
                            OriginalPrice = product.PriceOriginal / product.Quantity,
                            Quantity = product.Quantity,
                            VariantId = product.VariantId,
                            FlashSaleProductId = !string.IsNullOrEmpty(product.FlashSaleProductId) ? product.FlashSaleProductId : null,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = userId
                        };

                        var orderDetailDB = await _orderDetailRepository.CreateOrderDetailAsync(orderDetail);
                        if (orderDetailDB == null)
                        {
                            throw new Exception("Lỗi khi tạo đơn hàng");
                        }

                        var platformFee = await _platformFeeRepository.GetPlatformFeeByCategoryIdAsync(product.CategoryId);
                        if (platformFee == null)
                        {
                            throw new Exception("Phí platform không tồn tại");
                        }
                        platformFeeTotal += (product.PriceOriginal / product.Quantity) * (platformFee / 100m) * product.Quantity;
                    }

                    // Tạo Payout cho shop này
                    var payout = new Payout
                    {
                        OrderId = orderDB.Id,
                        ShopId = itemShop.ShopId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = userId,
                        GrossAmount = itemShop.PriceOriginal + itemShop.FeeShipping - itemShop.VoucherValue,
                        NetAmount = (itemShop.PriceOriginal + itemShop.FeeShipping - itemShop.VoucherValue) - itemShop.FeeShipping - platformFeeTotal,
                        PlatformFee = platformFeeTotal,
                        ShippingFee = itemShop.FeeShipping,
                        Status = PayoutStatus.Pending,
                    };
                    // Console.WriteLine($"[DEBUG] Creating Payout for OrderId: {orderDB.Id}, ShopId: {itemShop.ShopId}");
                    // Sử dụng Add trực tiếp thay vì CreatePayoutAsync để tránh SaveChangesAsync trong transaction
                    await _payoutRepository.AddPayoutAsync(payout);
                    // Console.WriteLine($"[DEBUG] Payout created successfully");



                }
                // Xử lý VoucherSystem
                if (orders.VoucherSystemId != null)
                {
                    await ProcessSystemVoucherAsync(orders.VoucherSystemId, userId);
                }

                return new StatusResponse<List<string>>
                {
                    Status = true,
                    Message = "Đơn hàng đã được tạo thành công",
                    Data = createdOrderIds
                };
            });

            // Console.WriteLine($"[DEBUG] Transaction completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception occurred in CreateOrderCODAsync: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            Console.WriteLine($"[DEBUG] Transaction was automatically rolled back");

            return new StatusResponse<List<string>> { Status = false, Message = $"Lỗi khi tạo đơn hàng: {ex.Message}", Data = null };
        }
    }
    public async Task<StatusResponse> UpdatePaymentStatusInOrderAsync(string orderId, string userId, PaymentStatus paymentStatus)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
        if (order == null)
        {
            return new StatusResponse { Status = false, Message = "Đơn hàng không tồn tại" };
        }
        order.StatusPayment = paymentStatus;
        order.UpdatedAt = DateTime.UtcNow;
        order.UpdatedBy = userId;
        var result = await _orderRepository.UpdatePaymentStatusInOrderAsync(order);
        if (!result)
        {
            return new StatusResponse { Status = false, Message = "Lỗi khi cập nhật trạng thái thanh toán" };
        }
        return new StatusResponse { Status = true, Message = "Đơn hàng đã được cập nhật trạng thái thanh toán thành công" };

    }

    public async Task<OrderDetailResponse> GetOrderDetailAsync(string orderId, string userId)
    {
        var order = await _orderRepository.GetOrderDetailAsync(orderId, userId);
        if (order == null)
        {
            return new OrderDetailResponse();
        }
        var orderDetailResponse = new OrderDetailResponse
        {
            OrderId = order.Id,
            ShopId = order.ShopId,
            ShopName = order.Shop?.Name ?? string.Empty,
            FinalPrice = order.FinalPrice ?? 0,
            PaymentMethod = order.PaymentMethod ?? PaymentMethod.COD,
            PaymentStatus = order.StatusPayment ?? PaymentStatus.Unpaid,
            RecipientName = order.RecipientName ?? string.Empty,
            RecipientPhone = order.RecipientPhone ?? string.Empty,
            RecipientAddress = order.DeliveryAddress ?? string.Empty,
            CreatedAt = order.CreatedAt ?? DateTime.UtcNow,
            DiscountPrice = order.DiscountPrice ?? 0,
            ShippingFee = order.ShippingFee ?? 0,
            OriginalPrice = order.OriginalPrice ?? 0,
            OrderStatus = order.StatusOrder ?? OrderStatus.Pending,
            OrderCode = order.OrderCode ?? string.Empty,
            HasFlashSale = order.HasFlashSale,
            Items = order.OrderDetails
            .Select(y => new OrderItemResponse
            {
                ProductId = y.ProductId,
                ProductName = y.Product?.Name,
                ProductImage = y.Product?.ImgMain,
                VariantId = y.VariantId.ToString(),
                VariantValues = y.Variant?.VariantValues?.Select(z => new PropertyValueForCartDto { Value = z.PropertyValue?.Value ?? string.Empty, ImgUrl = z.PropertyValue?.ImgUrl ?? string.Empty, Level = z.PropertyValue?.Level ?? 0 }).ToList() ?? new List<PropertyValueForCartDto>(),
                Price = y.OriginalPrice ?? 0,
                Quantity = (int)(y.Quantity ?? 0)
            }).ToList()
        };

        // Batch check review status để tránh N+1 query
        var reviewItems = orderDetailResponse.Items
            .Select(item => (orderId, item.ProductId))
            .ToList();
        var reviewStatuses = await _reviewService.GetAllowReviewStatusBatchAsync(reviewItems, userId);

        foreach (var item in orderDetailResponse.Items)
        {
            item.IsAllowReview = reviewStatuses.GetValueOrDefault((orderId, item.ProductId), false);
        }

        return orderDetailResponse;

    }
    public async Task<List<OrderResponse>> GetOrderAsync(string userId)
    {
        var order = await _orderRepository.GetOrderByUserIdAsync(userId);
        if (order.IsNullOrEmpty())
        {
            return new List<OrderResponse>();
        }
        var orderResponses = order
            .Select(x => new OrderResponse
            {
                OrderCode = x.OrderCode ?? string.Empty,
                OrderId = x.Id,
                ShopId = x.ShopId,
                ShopName = x.Shop?.Name ?? string.Empty,
                FinalPrice = x.FinalPrice ?? 0,
                // RecipientName = x.RecipientName ?? string.Empty,
                // RecipientPhone = x.RecipientPhone ?? string.Empty,
                // RecipientAddress = x.DeliveryAddress ?? string.Empty,
                PaymentMethod = x.PaymentMethod ?? PaymentMethod.COD,
                PaymentStatus = x.StatusPayment ?? PaymentStatus.Unpaid,
                OrderStatus = x.StatusOrder ?? OrderStatus.Pending,
                HasFlashSale = x.HasFlashSale,
                Items = x.OrderDetails
                .Select(y => new OrderItemResponse
                {
                    ProductId = y.ProductId,
                    ProductName = y.Product?.Name,
                    ProductImage = y.Product?.ImgMain,
                    VariantId = y.VariantId.ToString(),
                    VariantValues = y.Variant?.VariantValues?.Select(z => new PropertyValueForCartDto { Value = z.PropertyValue?.Value ?? string.Empty, ImgUrl = z.PropertyValue?.ImgUrl ?? string.Empty, Level = z.PropertyValue?.Level ?? 0 }).ToList() ?? new List<PropertyValueForCartDto>(),
                    Price = y.OriginalPrice ?? 0,
                    Quantity = (int)(y.Quantity ?? 0)

                })
                    .ToList()
            }).ToList();

        // Batch check review status để tránh N+1 query
        var reviewItems = orderResponses
            .SelectMany(order => order.Items.Select(item => (order.OrderId, item.ProductId)))
            .ToList();
        var reviewStatuses = await _reviewService.GetAllowReviewStatusBatchAsync(reviewItems, userId);

        foreach (var item in orderResponses)
        {
            foreach (var orderItem in item.Items)
            {
                orderItem.IsAllowReview = reviewStatuses.GetValueOrDefault((item.OrderId, orderItem.ProductId), false);
            }
        }

        return orderResponses;
    }

    public async Task<StatusResponse> CancleOrder(string orderId, string userId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
        if (order == null)
        {
            return new StatusResponse { Status = false, Message = "order không tồn tại" };
        }
        if (order.StatusOrder != 0)
        {
            return new StatusResponse { Status = false, Message = "không được hủy đơn hàng đã xác nhận" };
        }
        var result = await _transactionRepository.ExecuteInTransactionAsync(async () =>
            {
                order.StatusOrder = OrderStatus.Cancelled;
                order.DeliveryStatus = DeliveryStatus.Cancel;
                order.UpdatedAt = DateTime.UtcNow;
                var resultGHN = await _ghnUtil.CancelOrderAsync(order.OrderCode);
                if (resultGHN?.data.FirstOrDefault()?.result == false)
                {
                    throw new Exception("Lỗi khi hủy đơn hàng");
                }
                var resultOrder = await _orderRepository.UpdatePaymentStatusInOrderAsync(order); // dùng chung được cho update cả order nhưng đặt tên sai á nha
                if (resultOrder == false)
                {
                    throw new Exception("Lỗi khi cập nhật đơn hàng");
                }
                return new StatusResponse { Status = true, Message = "cập nhật thành công" };
            }
            );
        return result;

    }

    // Helper methods for optimized SaveOrderAsync
    private async Task<ValidationResult> ValidateOrderProducts(
        List<OrderProductItem> allProducts,
        Dictionary<int, Variant> variants,
        Dictionary<string, FlashSaleProduct> flashSaleProducts,
        Dictionary<string, FlashSaleProduct> activeFlashSales,
        string userId)
    {
        var result = new ValidationResult { IsValid = true };

        foreach (var item in allProducts)
        {
            var product = item.Product;

            // Validate Variant
            if (!product.VariantId.HasValue)
            {
                result.IsValid = false;
                result.ErrorMessage = "VariantId không được để trống";
                return result;
            }

            if (!variants.TryGetValue(product.VariantId.Value, out var variant))
            {
                result.IsValid = false;
                result.ErrorMessage = "Biến thể không tồn tại";
                return result;
            }

            if (variant.Status != VariantStatus.Show)
            {
                result.IsValid = false;
                result.ErrorMessage = "Biến thể sản phẩm không khả dụng";
                return result;
            }

            if (variant.Quantity < product.Quantity)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Số lượng sản phẩm không đủ";
                return result;
            }

            result.VariantUpdates.Add((product.VariantId.Value, product.Quantity));

            // Validate FlashSale - Sử dụng FlashSaleProductId thay vì IsFlashSale
            if (!string.IsNullOrEmpty(product.FlashSaleProductId))
            {
                if (!activeFlashSales.TryGetValue(product.ProductId, out var activeFlashSale) ||
                    activeFlashSale.Id != product.FlashSaleProductId)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Flash sale cho sản phẩm {product.ProductId} đã hết hạn";
                    return result;
                }

                if (!flashSaleProducts.TryGetValue(product.FlashSaleProductId, out var flashSaleProduct))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Flash sale không tồn tại";
                    return result;
                }

                var remainingQuantity = (flashSaleProduct.Quantity ?? 0) - (flashSaleProduct.UsedQuantity ?? 0);
                if (remainingQuantity < product.Quantity)
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Số lượng flash sale không đủ";
                    return result;
                }

                if (flashSaleProduct.OrderLimit.HasValue)
                {
                    var userPurchaseCount = await _flashSaleRepository.GetUserFlashSalePurchaseCountAsync(
                        userId, product.FlashSaleProductId);

                    if (userPurchaseCount + product.Quantity > flashSaleProduct.OrderLimit.Value)
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Bạn chỉ được mua tối đa {flashSaleProduct.OrderLimit.Value} sản phẩm flash sale này";
                        return result;
                    }
                }

                result.FlashSaleUpdates.Add((product.FlashSaleProductId, product.Quantity));
            }
        }

        return result;
    }

    private async Task BatchDecreaseQuantitiesAsync(ValidationResult validationResult)
    {
        var variantTasks = validationResult.VariantUpdates
            .Select(update => _variantRepository.DecreaseVariantQuantityAsync(update.Item1, update.Item2));

        var variantResults = await Task.WhenAll(variantTasks);
        if (variantResults.Any(r => !r))
        {
            throw new Exception("Lỗi khi cập nhật số lượng sản phẩm");
        }

        if (validationResult.FlashSaleUpdates.Any())
        {
            var flashSaleTasks = validationResult.FlashSaleUpdates
                .Select(update => _flashSaleRepository.DecreaseFlashSaleQuantityAsync(update.Item1, update.Item2));

            var flashSaleResults = await Task.WhenAll(flashSaleTasks);
            if (flashSaleResults.Any(r => !r))
            {
                throw new Exception("Lỗi khi cập nhật số lượng flash sale");
            }
        }
    }

    private async Task ProcessShopVoucherAsync(string voucherId, string userId, string orderId)
    {
        var voucher = await _voucherRepository.GetUserVoucherShopsByVoucherShopIdAsync(userId, voucherId);
        if (voucher != null)
        {
            if (voucher.UsedCount >= voucher.ClaimedCount)
            {
                throw new Exception("Bạn đã dùng hết voucher này");
            }
            voucher.UsedCount += 1;
            await _voucherRepository.UpdateUserVoucherShopAsync(voucher);
        }
        else
        {
            var voucherShop = await _voucherRepository.GetVoucherShopByIdAsync(voucherId);
            if (voucherShop == null)
            {
                throw new Exception("Voucher không tồn tại");
            }

            var userVoucherShop = new UserVoucherShop
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                VoucherShopId = voucherId,
                ClaimedCount = voucherShop.LimitForUser ?? 0,
                UsedCount = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };
            await _voucherRepository.CreateUserVoucherShopAsync(userVoucherShop);

            if (voucherShop.Quantity == null || voucherShop.Quantity <= 0)
            {
                throw new Exception("Voucher shop đã hết số lượng");
            }
            voucherShop.Quantity -= 1;
            await _voucherRepository.UpdateVoucherShopAsync(voucherShop);
        }

        var orderVoucher = new OrderVoucher
        {
            OrderId = orderId,
            VoucherShopId = voucherId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = userId,
        };
        await _orderRepository.CreateOrderVoucherAsync(orderVoucher);
    }

    private async Task ProcessPromotionGiftsAsync(List<GiftInOrder> gifts, string orderId, string userId)
    {
        var giftsByPromotion = gifts
            .Where(g => g.PromotionId != null)
            .GroupBy(g => g.PromotionId!)
            .ToList();

        foreach (var promotionGroup in giftsByPromotion)
        {
            var promotionId = promotionGroup.Key;
            var promotion = await _promotionRepository.GetPromotionByIdAsync(promotionId);

            if (promotion != null && promotion.TotalUsageLimit.HasValue &&
                promotion.TotalUsageLimit == promotion.UsedCount)
            {
                throw new Exception("Promotion đã đạt giới hạn sử dụng");
            }

            if (promotion != null)
            {
                promotion.UsedCount = (promotion.UsedCount ?? 0) + 1;
                promotion.UpdatedAt = DateTime.UtcNow;
                await _promotionRepository.UpdatePromotionAsync(promotion);
            }

            foreach (var gift in promotionGroup)
            {
                var promotionUsage = new PromotionUsage
                {
                    Id = Guid.NewGuid().ToString(),
                    PromotionId = gift.PromotionId,
                    PromotionGiftId = gift.PromotionGiftId,
                    OrderId = orderId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = userId
                };
                await _promotionRepository.CreatePromotionUsageWithoutIncrementAsync(promotionUsage);
            }
        }
    }

    private async Task ProcessSystemVoucherAsync(string voucherSystemId, string userId)
    {
        var userVoucherSystem = await _voucherRepository.GetUserVoucherSystemByVoucherSystemIdAsync(userId, voucherSystemId);
        var voucherSystem = await _voucherRepository.GetVoucherSystemByIdAsync(voucherSystemId);

        if (voucherSystem == null)
        {
            throw new Exception("Voucher system không tồn tại");
        }

        if (userVoucherSystem != null)
        {
            if (userVoucherSystem.UsedCount >= userVoucherSystem.ClaimedCount)
            {
                throw new Exception("Bạn đã dùng hết voucher system này");
            }
            userVoucherSystem.UsedCount += 1;
            await _voucherRepository.UpdateUserVoucherSystemAsync(userVoucherSystem);
        }
        else
        {
            var newUserVoucherSystem = new UserVoucherSystem
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                VoucherSystemId = voucherSystemId,
                ClaimedCount = voucherSystem.LimitForUser ?? 0,
                UsedCount = 1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = userId
            };
            await _voucherRepository.CreateUserVoucherSystemAsync(newUserVoucherSystem);
        }

        if (voucherSystem.Quantity == null || voucherSystem.Quantity <= 0)
        {
            throw new Exception("Voucher system đã hết số lượng");
        }
        voucherSystem.Quantity -= 1;
        await _voucherRepository.UpdateVoucherSystemAsync(voucherSystem);
    }
}