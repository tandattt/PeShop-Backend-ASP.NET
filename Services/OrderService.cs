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
        IUserRepository userRepository
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
    }
    public async Task<CreateVirtualOrderResponse> CreateVirtualOrder(OrderVirtualRequest request, string userId)
    {
        try
        {
            // Nhóm các sản phẩm theo shop
            var itemShops = await _orderHelper.GroupItemsByShopAsync(request.Items);

            // Tính tổng giá trị đơn hàng
            // decimal orderTotal = itemShops.Sum(shop => shop.PriceOriginal);

            var userAddress = await _userAddressRepository.GetUserAddressByIdAsync(request.UserAddressId, userId);
            // Tạo đơn hàng ảo
            var order = new OrderVirtualDto
            {
                OrderId = Guid.NewGuid().ToString(),
                ItemShops = itemShops,
                // OrderTotal = orderTotal,
                // AmountTotal = orderTotal,
                UserId = userId,
                RecipientName = userAddress?.RecipientName ?? string.Empty,
                RecipientPhone = userAddress?.Phone ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UserFullNewAddress = userAddress?.FullNewAddress ?? string.Empty
            };

            // Lưu vào Redis
            bool isSaved = await _redisUtil.SetAsync($"order_{userId}_{order.OrderId}", JsonSerializer.Serialize(order));
            // Xóa đơn hàng sau 60 phút
            BackgroundJob.Schedule<IJobService>(service => service.DeleteOrderOnRedisAsync(order.OrderId, userId, false), TimeSpan.FromMinutes(60));
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

            // Nhóm các sản phẩm mới theo shop
            var itemShops = await _orderHelper.GroupItemsByShopAsync(request.Items);

            // Tính tổng giá trị đơn hàng mới
            decimal orderTotal = itemShops.Sum(shop => shop.PriceOriginal);

            // Cập nhật order với dữ liệu mới
            existingOrder.ItemShops = itemShops;
            existingOrder.OrderTotal = orderTotal;
            existingOrder.AmountTotal = orderTotal;
            existingOrder.FeeShippingTotal = 0; // Reset shipping fee

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
            BackgroundJob.Enqueue<IJobService>(service => service.DeleteOrderOnRedisAsync(orderId, userId, false));
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Template", "PaymentSuccess.html");
            string htmlBody;
            if (File.Exists(templatePath))
            {
                htmlBody = await File.ReadAllTextAsync(templatePath);
                htmlBody = htmlBody.Replace("{CustomerName}", orders.RecipientName);
                // htmlBody = htmlBody.Replace("{OrderId}", orderId);
                htmlBody = htmlBody.Replace("{OrderDate}", DateTime.UtcNow.ToString("dd/MM/yyyy"));
                htmlBody = htmlBody.Replace("{PaymentMethod}", "COD");
                htmlBody = htmlBody.Replace("{TotalAmount}", orders.AmountTotal.ToString("N0"));
                BackgroundJob.Enqueue<IEmailUtil>(service => service.SendEmailAsync(user.Email, "Đơn hàng đã được tạo thành công", htmlBody, true));
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
                List<string> createdOrderIds = new List<string>(); // Lưu tất cả OrderIds đã tạo

                foreach (var itemShop in orders.ItemShops)
                {
                    decimal platformFeeTotal = 0;
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
                        OrderCode = itemShop.OrderCode,
                        ShippingFee = itemShop.FeeShipping,
                        DeliveryAddress = orders.UserFullNewAddress,
                        DeliveryStatus = DeliveryStatus.NotDelivered,
                        StatusOrder = OrderStatus.Pending,
                        ShopId = itemShop.ShopId,
                        UserId = userId,
                        OriginalPrice = itemShop.PriceOriginal,
                        FinalPrice = itemShop.PriceOriginal + itemShop.FeeShipping - itemShop.VoucherValue,
                        PaymentMethod = paymentMethod,
                        StatusPayment = paymentStatus,

                    };
                    // Console.WriteLine($"[DEBUG] Creating Order for ShopId: {itemShop.ShopId}");
                    var orderDB = await _orderRepository.CreateOrderAsync(order);
                    if (orderDB == null)
                    {
                        // Console.WriteLine($"[ERROR] Failed to create Order for ShopId: {itemShop.ShopId}");
                        throw new Exception("Lỗi khi tạo đơn hàng");
                    }
                    // Console.WriteLine($"[DEBUG] Order created successfully with ID: {orderDB.Id}");

                    // Lưu OrderId để tạo OrderVoucherSystem sau này
                    createdOrderIds.Add(orderDB.Id);
                    if (itemShop.VoucherId != null)
                    {
                        var voucher = await _voucherRepository.GetUserVoucherShopsByVoucherShopIdAsync(userId, itemShop.VoucherId);
                        if (voucher != null)
                        {
                            if (voucher.UsedCount >= voucher.ClaimedCount)
                            {
                                throw new Exception("bạn đã dùng hết voucher này");
                            }
                            voucher.UsedCount = voucher.UsedCount + 1;
                            await _voucherRepository.UpdateUserVoucherShopAsync(voucher);
                        }
                        else
                        {
                            var voucherShop = await _voucherRepository.GetVoucherShopByIdAsync(itemShop.VoucherId);
                            if (voucherShop == null)
                            {
                                throw new Exception("Voucher không tồn tại");
                            }
                            var userVoucherShop = new UserVoucherShop
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserId = userId,
                                VoucherShopId = itemShop.VoucherId,
                                ClaimedCount = voucherShop.LimitForUser ?? 0,
                                UsedCount = 1,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = userId,
                                UpdatedAt = DateTime.UtcNow,
                                UpdatedBy = userId
                            };
                            // Console.WriteLine($"[DEBUG] Creating UserVoucherShop for UserId: {userId}, VoucherShopId: {itemShop.VoucherId}");
                            await _voucherRepository.CreateUserVoucherShopAsync(userVoucherShop);
                            // Console.WriteLine($"[DEBUG] UserVoucherShop created successfully");

                            // Kiểm tra số lượng voucher shop trước khi trừ
                            if (voucherShop.Quantity == null || voucherShop.Quantity <= 0)
                            {
                                throw new Exception("Voucher shop đã hết số lượng");
                            }
                            voucherShop.Quantity = voucherShop.Quantity - 1;
                            await _voucherRepository.UpdateVoucherShopAsync(voucherShop);
                        }
                        var orderVoucher = new OrderVoucher
                        {
                            OrderId = orderDB.Id,
                            VoucherShopId = itemShop.VoucherId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = userId,
                            VoucherSystemId = orders.VoucherSystemId,
                        };
                        
                       
                        // Console.WriteLine($"[DEBUG] Creating OrderVoucher for OrderId: {orderDB.Id}, VoucherId: {itemShop.VoucherId}");
                        var orderVoucherDB = await _orderRepository.CreateOrderVoucherAsync(orderVoucher);
                        if (orderVoucherDB == null)
                        {
                            // Console.WriteLine($"[ERROR] Failed to create OrderVoucher for OrderId: {orderDB.Id}");
                            throw new Exception("Lỗi khi tạo đơn hàng");
                        }
                        // Console.WriteLine($"[DEBUG] OrderVoucher created successfully with ID: {orderVoucherDB.Id}");

                    }
                    if (itemShop.Gifts.IsNullOrEmpty() == false)
                    {
                        // Nhóm gifts theo PromotionId để chỉ tăng used_count 1 lần cho mỗi promotion
                        var giftsByPromotion = itemShop.Gifts
                            .Where(g => g.PromotionId != null)
                            .GroupBy(g => g.PromotionId!)
                            .ToList();

                        foreach (var promotionGroup in giftsByPromotion)
                        {
                            var promotionId = promotionGroup.Key;

                            // Kiểm tra giới hạn sử dụng promotion (chỉ check 1 lần cho mỗi promotion)
                            var promotion = await _promotionRepository.GetPromotionByIdAsync(promotionId);
                            if (promotion != null && promotion.TotalUsageLimit.HasValue && promotion.TotalUsageLimit == promotion.UsedCount)
                            {
                                throw new Exception("Promotion đã đạt giới hạn sử dụng");
                            }

                            // Tăng used_count 1 lần cho promotion này
                            if (promotion != null)
                            {
                                promotion.UsedCount = (promotion.UsedCount ?? 0) + 1;
                                promotion.UpdatedAt = DateTime.UtcNow;
                                await _promotionRepository.UpdatePromotionAsync(promotion);
                            }

                            // Tạo PromotionUsage cho tất cả gifts trong promotion này (nhưng không tăng used_count nữa)
                            foreach (var gift in promotionGroup)
                            {
                                var promotionUsage = new PromotionUsage
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    PromotionId = gift.PromotionId,
                                    PromotionGiftId = gift.PromotionGiftId,
                                    OrderId = orderDB.Id,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedBy = userId,
                                    UpdatedAt = DateTime.UtcNow,
                                    UpdatedBy = userId
                                };
                                // Tạo PromotionUsage mà không tăng used_count (vì đã tăng ở trên)
                                await _promotionRepository.CreatePromotionUsageWithoutIncrementAsync(promotionUsage);
                            }
                        }
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
                            OriginalPrice = product.PriceOriginal,
                            Quantity = product.Quantity,
                            VariantId = product.VariantId,
                            Note = product.Note,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = userId
                        };
                        // Console.WriteLine($"[DEBUG] Creating OrderDetail for ProductId: {product.ProductId}");
                        var orderDetailDB = await _orderDetailRepository.CreateOrderDetailAsync(orderDetail);
                        if (orderDetailDB == null)
                        {
                            //      Console.WriteLine($"[ERROR] Failed to create OrderDetail for ProductId: {product.ProductId}");
                            throw new Exception("Lỗi khi tạo đơn hàng");
                        }
                        // Console.WriteLine($"[DEBUG] OrderDetail created successfully with ID: {orderDetailDB.Id}");
                        var platformFee = await _platformFeeRepository.GetPlatformFeeByCategoryIdAsync(product.CategoryId);
                        if (platformFee == null)
                        {
                            throw new Exception("Phí platform không tồn tại");
                        }
                        platformFeeTotal += product.PriceOriginal * (platformFee / 100m) * product.Quantity;
                        // var productDb = await _productRepository.GetProductByIdAsync(product.ProductId);
                        // if (productDb == null)
                        // {
                        //     throw new Exception("Sản phẩm không tồn tại");
                        // }
                        // productDb.BoughtCount = productDb.BoughtCount + product.Quantity;
                        // await _productRepository.UpdateProductAsync(productDb);
                        // if (product.VariantId == null)
                        // {
                        //     throw new Exception("VariantId không được để trống");
                        // }

                        var variantDb = await _variantRepository.GetVariantByIdAsync(product.VariantId.Value.ToString());
                        if (variantDb == null)
                        {
                            throw new Exception("Biến thể không tồn tại");
                        }
                        if (variantDb.Status != VariantStatus.Show)
                        {
                            throw new Exception("Biến thể sản phẩm không khả dụng");
                        }
                        // Kiểm tra số lượng variant trước khi trừ
                        if (variantDb.Quantity == null || variantDb.Quantity < product.Quantity)
                        {
                            throw new Exception($"Số lượng sản phẩm không đủ");
                        }
                        variantDb.Quantity = variantDb.Quantity - product.Quantity;
                        await _variantRepository.UpdateVariantAsync(variantDb);
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
                // Console.WriteLine($"[DEBUG] Created {createdOrderIds.Count} Orders");
                // Tạo OrderVoucherSystem cho TẤT CẢ các Orders (vì VoucherSystem áp dụng cho toàn bộ đơn hàng)
                if (orders.VoucherSystemId != null)
                {
                    // foreach (var orderId in createdOrderIds)
                    // {
                    //     var orderVoucherSystem = new OrderVoucher
                    //     {
                    //         OrderId = orderId,
                    //         VoucherId = orders.VoucherSystemId,
                    //         CreatedAt = DateTime.UtcNow,
                    //         CreatedBy = userId,
                    //         UpdatedAt = DateTime.UtcNow,
                    //         UpdatedBy = userId,
                    //         VoucherPrice = orders.VoucherSystemValue,
                    //         VoucherName = orders.VoucherSystemName,
                    //         Type = OrderVoucherType.System,
                    //     };
                    //  var orderVoucherSystemDB = await _orderRepository.CreateOrderVoucherAsync(orderVoucherSystem);
                    //     if (orderVoucherSystemDB == null)
                    //     {
                    //         throw new Exception("Lỗi khi tạo OrderVoucherSystem");
                    //     }
                    
                    // }

                    // // Xử lý UserVoucherSystem sau khi tạo tất cả orders
                    // if (orders.VoucherSystemId != null)
                    // {
                    var userVoucherSystem = await _voucherRepository.GetUserVoucherSystemByVoucherSystemIdAsync(userId, orders.VoucherSystemId);
                    var voucherSystem = await _voucherRepository.GetVoucherSystemByIdAsync(orders.VoucherSystemId);
                    if (voucherSystem == null)
                    {
                        throw new Exception("Voucher system không tồn tại");
                    }
                    if (userVoucherSystem != null)
                    {
                        if (userVoucherSystem.UsedCount >= userVoucherSystem.ClaimedCount)
                        {
                            throw new Exception("bạn đã dùng hết voucher system này");
                        }
                        userVoucherSystem.UsedCount = userVoucherSystem.UsedCount + 1;
                        await _voucherRepository.UpdateUserVoucherSystemAsync(userVoucherSystem);
                        // Console.WriteLine($"[DEBUG] Updated UserVoucherSystem UsedCount to: {userVoucherSystem.UsedCount}");
                    }
                    else
                    {
                        var newUserVoucherSystem = new UserVoucherSystem
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = userId,
                            VoucherSystemId = orders.VoucherSystemId,
                            ClaimedCount = voucherSystem.LimitForUser ?? 0,
                            UsedCount = 1,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = userId
                        };
                        // Console.WriteLine($"[DEBUG] Creating UserVoucherSystem for UserId: {userId}, VoucherSystemId: {orders.VoucherSystemId}");
                        await _voucherRepository.CreateUserVoucherSystemAsync(newUserVoucherSystem);
                    }


                    // Console.WriteLine($"[DEBUG] UserVoucherSystem created successfully");

                    // Kiểm tra số lượng voucher system trước khi trừ
                    if (voucherSystem.Quantity == null || voucherSystem.Quantity <= 0)
                    {
                        throw new Exception("Voucher system đã hết số lượng");
                    }
                    voucherSystem.Quantity = voucherSystem.Quantity - 1;
                    await _voucherRepository.UpdateVoucherSystemAsync(voucherSystem);

                }

                // Console.WriteLine($"[DEBUG] All operations completed successfully");
                return new StatusResponse<List<string>> { Status = true, Message = "Đơn hàng đã được tạo thành công", Data = createdOrderIds.ToList() };
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
        await _orderRepository.UpdatePaymentStatusInOrderAsync(order);

        return new StatusResponse { Status = true, Message = "Đơn hàng đã được cập nhật trạng thái thành công" };
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
        if (orderDetailResponse.PaymentMethod == PaymentMethod.VNPay && orderDetailResponse.PaymentStatus == PaymentStatus.Processing)
        {
            var paymentLink = await _redisUtil.GetAsyncWithTtl($"order_payment_processing_{userId}_{orderDetailResponse.OrderId}");
            if (paymentLink.Key != null)
            {
                orderDetailResponse.PaymentProcessing = new OrderPaymentProcessing
                {
                    Time = paymentLink.Value.Value.TotalSeconds,
                    PaymentLink = paymentLink.Key ?? string.Empty,
                };
            }
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
                Items = x.OrderDetails
                .Select(y => new OrderItemResponse
                {
                    ProductId = y.ProductId,
                    ProductName = y.Product?.Name,
                    ProductImage = y.Product?.ImgMain,
                    VariantId = y.VariantId.ToString(),
                    VariantValues = y.Variant?.VariantValues?.Select(z => new PropertyValueForCartDto { Value = z.PropertyValue?.Value ?? string.Empty, ImgUrl = z.PropertyValue?.ImgUrl ?? string.Empty, Level = z.PropertyValue?.Level ?? 0 }).ToList() ?? new List<PropertyValueForCartDto>(),
                    Price = y.OriginalPrice ?? 0,
                    Quantity = (int)(y.Quantity ?? 0),

                })
                    .ToList()
            }).ToList();
        var orderPaymentProcessing = orderResponses.Where(x => x.PaymentMethod == PaymentMethod.VNPay && x.PaymentStatus == PaymentStatus.Processing).ToList();
        foreach (var item in orderResponses)
        {
            if (item.PaymentMethod == PaymentMethod.VNPay && item.PaymentStatus == PaymentStatus.Processing)
            {
                var paymentLink = await _redisUtil.GetAsyncWithTtl($"order_payment_processing_{userId}_{item.OrderId}");
                if (paymentLink.Key != null)
                {
                    item.PaymentProcessing = new OrderPaymentProcessing
                    {
                        Time = paymentLink.Value.Value.TotalSeconds,
                        PaymentLink = paymentLink.Key ?? string.Empty,
                    };
                }
            }

            foreach (var orderItem in item.Items)
            {
                orderItem.IsAllowReview = await _reviewService.IsAllowReviewAsync(item.OrderId, orderItem.ProductId, userId);
            }
        }

        return orderResponses;
    }
}