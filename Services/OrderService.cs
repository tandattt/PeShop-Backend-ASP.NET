namespace PeShop.Services;
using PeShop.Services.Interfaces;
using PeShop.Dtos.Requests;
using PeShop.Dtos.Responses;
using PeShop.Interfaces;
using PeShop.Data.Repositories.Interfaces;
using PeShop.Dtos.Shared;
using System.Text.Json;

using System.Linq;
using PeShop.Models.Entities;
using PeShop.Models.Enums;
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
        IVariantRepository variantRepository
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
    }
    public async Task<CreateVirtualOrderResponse> CreateVirtualOrder(OrderVirtualRequest request, string userId)
    {
        try
        {
            // Nhóm các sản phẩm theo shop
            var itemShops = await _orderHelper.GroupItemsByShopAsync(request.Items);

            // Tính tổng giá trị đơn hàng
            decimal orderTotal = itemShops.Sum(shop => shop.PriceOriginal);

            var userAddress = await _userAddressRepository.GetUserAddressByIdAsync(request.UserAddressId, userId);
            // Tạo đơn hàng ảo
            var order = new OrderVirtualDto
            {
                OrderId = Guid.NewGuid().ToString(),
                ItemShops = itemShops,
                OrderTotal = orderTotal,
                AmountTotal = orderTotal,
                UserId = userId ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UserFullNewAddress = userAddress?.FullNewAddress ?? string.Empty
            };

            // Lưu vào Redis
            bool isSaved = await _redisUtil.SetAsync($"order_{userId}_{order.OrderId}", JsonSerializer.Serialize(order));

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
    public async Task<CreateVirtualOrderResponse> CalclulateOrderTotal(string orderId, string userId)
    {
        var order = await _redisUtil.GetAsync<OrderVirtualDto>($"order_{userId}_{orderId}");

        foreach (var itemShop in order.ItemShops)
        {
            itemShop.PriceAfterVoucher = itemShop.PriceOriginal - 0;
            if (itemShop.VoucherId == null) continue;

            var voucherShop = await _voucherRepository.GetVoucherShopByIdAsync(itemShop.VoucherId);
            if (voucherShop.Type == VoucherValueType.Percentage)
            {
                decimal discountAmount = itemShop.PriceOriginal * (voucherShop.DiscountValue ?? 0) > (voucherShop.MaxdiscountAmount ?? 0) ? (voucherShop.MaxdiscountAmount ?? 0) : itemShop.PriceOriginal * (voucherShop.DiscountValue ?? 0);
                itemShop.PriceAfterVoucher = itemShop.PriceOriginal - discountAmount;
            }
            else if (voucherShop.Type == VoucherValueType.FixedAmount)
            {
                itemShop.PriceAfterVoucher = itemShop.PriceOriginal - voucherShop.DiscountValue ?? 0;
            }
        }

        if (order.VoucherSystemId != null)
        {
            var voucherSystem = await _voucherRepository.GetVoucherSystemByIdAsync(order.VoucherSystemId);
            if (voucherSystem.Type == VoucherValueType.Percentage)
            {
                decimal discountAmount = order.OrderTotal * (voucherSystem.DiscountValue ?? 0) > (voucherSystem.MaxdiscountAmount ?? 0) ? (voucherSystem.MaxdiscountAmount ?? 0) : order.OrderTotal * (voucherSystem.DiscountValue ?? 0);
                order.OrderTotal = order.OrderTotal - discountAmount;
            }
            else if (voucherSystem.Type == VoucherValueType.FixedAmount)
            {
                order.OrderTotal = order.OrderTotal - voucherSystem.DiscountValue ?? 0;
            }
        }
        order.AmountTotal = order.OrderTotal + order.FeeShippingTotal - order.VoucherSystemValue;
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
        Console.WriteLine($"[DEBUG] Starting CreateOrderCODAsync for OrderId: {orderId}, UserId: {userId}");

        var orders = await _redisUtil.GetAsync<OrderVirtualDto>($"calculated_order_{userId}_{orderId}");
        if (orders == null)
        {
            Console.WriteLine($"[DEBUG] Order not found in Redis for OrderId: {orderId}");
            return new StatusResponse { Status = false, Message = "Đơn hàng không tồn tại" };
        }

        Console.WriteLine($"[DEBUG] Order found in Redis, starting transaction");

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
                        DiscountPrice = orders.VoucherSystemValue,
                        ShippingFee = itemShop.FeeShipping,
                        DeliveryAddress = orders.UserFullNewAddress,
                        DeliveryStatus = DeliveryStatus.NotDelivered,
                        StatusOrder = OrderStatus.Pending,
                        ShopId = itemShop.ShopId,
                        UserId = userId,
                        OriginalPrice = itemShop.PriceOriginal,
                        FinalPrice = itemShop.PriceAfterVoucher + itemShop.FeeShipping ?? 0,
                        PaymentMethod = PaymentMethod.COD,
                        StatusPayment = PaymentStatus.Unpaid,

                    };
                    Console.WriteLine($"[DEBUG] Creating Order for ShopId: {itemShop.ShopId}");
                    var orderDB = await _orderRepository.CreateOrderAsync(order);
                    if (orderDB == null)
                    {
                        Console.WriteLine($"[ERROR] Failed to create Order for ShopId: {itemShop.ShopId}");
                        throw new Exception("Lỗi khi tạo đơn hàng");
                    }
                    Console.WriteLine($"[DEBUG] Order created successfully with ID: {orderDB.Id}");

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
                        Console.WriteLine($"[DEBUG] Creating UserVoucherShop for UserId: {userId}, VoucherShopId: {itemShop.VoucherId}");
                        await _voucherRepository.CreateUserVoucherShopAsync(userVoucherShop);
                        Console.WriteLine($"[DEBUG] UserVoucherShop created successfully");


                        var orderVoucher = new OrderVoucher
                        {
                            OrderId = orderDB.Id,
                            VoucherId = itemShop.VoucherId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = userId,
                            VoucherPrice = itemShop.VoucherValue,
                            VoucherName = itemShop.VoucherName,
                            Type = OrderVoucherType.Shop,
                        };
                        Console.WriteLine($"[DEBUG] Creating OrderVoucher for OrderId: {orderDB.Id}, VoucherId: {itemShop.VoucherId}");
                        var orderVoucherDB = await _orderRepository.CreateOrderVoucherAsync(orderVoucher);
                        if (orderVoucherDB == null)
                        {
                            Console.WriteLine($"[ERROR] Failed to create OrderVoucher for OrderId: {orderDB.Id}");
                            throw new Exception("Lỗi khi tạo đơn hàng");
                        }
                        Console.WriteLine($"[DEBUG] OrderVoucher created successfully with ID: {orderVoucherDB.Id}");

                        voucherShop.Quantity = voucherShop.Quantity - 1;
                        await _voucherRepository.UpdateVoucherShopAsync(voucherShop);
                    }

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
                        Console.WriteLine($"[DEBUG] Creating OrderDetail for ProductId: {product.ProductId}");
                        var orderDetailDB = await _orderDetailRepository.CreateOrderDetailAsync(orderDetail);
                        if (orderDetailDB == null)
                        {
                            Console.WriteLine($"[ERROR] Failed to create OrderDetail for ProductId: {product.ProductId}");
                            throw new Exception("Lỗi khi tạo đơn hàng");
                        }
                        Console.WriteLine($"[DEBUG] OrderDetail created successfully with ID: {orderDetailDB.Id}");
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
                        var variantDb = await _variantRepository.GetVariantByIdAsync(product.VariantId.ToString());
                        if (variantDb == null)
                        {
                            throw new Exception("Biến thể không tồn tại");
                        }
                        variantDb.Quantity = variantDb.Quantity - product.Quantity;
                        await _variantRepository.UpdateVariantAsync(variantDb);
                    }

                    // Tạo OrderVoucher cho shop voucher (chỉ một lần cho mỗi shop)
                    if (itemShop.VoucherId != null)
                    {

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
                        GrossAmount = itemShop.PriceAfterVoucher + itemShop.FeeShipping,
                        NetAmount = (itemShop.PriceAfterVoucher + itemShop.FeeShipping) - itemShop.FeeShipping - platformFeeTotal,
                        PlatformFee = platformFeeTotal,
                        ShippingFee = itemShop.FeeShipping,
                        Status = PayoutStatus.Pending,
                    };
                    Console.WriteLine($"[DEBUG] Creating Payout for OrderId: {orderDB.Id}, ShopId: {itemShop.ShopId}");
                    // Sử dụng Add trực tiếp thay vì CreatePayoutAsync để tránh SaveChangesAsync trong transaction
                    await _payoutRepository.AddPayoutAsync(payout);
                    Console.WriteLine($"[DEBUG] Payout created successfully");



                }
                Console.WriteLine($"[DEBUG] Created {createdOrderIds.Count} Orders");
                // Tạo OrderVoucherSystem cho TẤT CẢ các Orders (vì VoucherSystem áp dụng cho toàn bộ đơn hàng)
                if (orders.VoucherSystemId != null)
                {
                    foreach (var orderId in createdOrderIds)
                    {
                        Console.WriteLine($"[DEBUG] Creating OrderVoucherSystem for OrderId: {orderId}, VoucherSystemId: {orders.VoucherSystemId}");
                        var orderVoucherSystem = new OrderVoucher
                        {
                            OrderId = orderId,
                            VoucherId = orders.VoucherSystemId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = userId,
                            VoucherPrice = orders.VoucherSystemValue,
                            VoucherName = orders.VoucherSystemName,
                            Type = OrderVoucherType.System,
                        };
                        var orderVoucherSystemDB = await _orderRepository.CreateOrderVoucherAsync(orderVoucherSystem);
                        if (orderVoucherSystemDB == null)
                        {
                            throw new Exception("Lỗi khi tạo OrderVoucherSystem");
                        }
                        Console.WriteLine($"[DEBUG] OrderVoucherSystem created successfully with ID: {orderVoucherSystemDB.Id}");
                    }
                }

                // Xử lý UserVoucherSystem sau khi tạo tất cả orders
                if (orders.VoucherSystemId != null)
                {
                    var userVoucherSystem = await _voucherRepository.GetUserVoucherSystemByVoucherSystemIdAsync(userId, orders.VoucherSystemId);
                    if (userVoucherSystem != null)
                    {
                        if (userVoucherSystem.UsedCount >= userVoucherSystem.ClaimedCount)
                        {
                            throw new Exception("bạn đã dùng hết voucher system này");
                        }
                        userVoucherSystem.UsedCount = userVoucherSystem.UsedCount + 1;
                        await _voucherRepository.UpdateUserVoucherSystemAsync(userVoucherSystem);
                        Console.WriteLine($"[DEBUG] Updated UserVoucherSystem UsedCount to: {userVoucherSystem.UsedCount}");
                    }
                    var voucherSystem = await _voucherRepository.GetVoucherSystemByIdAsync(orders.VoucherSystemId);
                    if (voucherSystem == null)
                    {
                        throw new Exception("Voucher system không tồn tại");
                    }
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
                    Console.WriteLine($"[DEBUG] Creating UserVoucherSystem for UserId: {userId}, VoucherSystemId: {orders.VoucherSystemId}");
                    await _voucherRepository.CreateUserVoucherSystemAsync(newUserVoucherSystem);
                    Console.WriteLine($"[DEBUG] UserVoucherSystem created successfully");

                    voucherSystem.Quantity = voucherSystem.Quantity - 1;
                    await _voucherRepository.UpdateVoucherSystemAsync(voucherSystem);

                }

                Console.WriteLine($"[DEBUG] All operations completed successfully");
                return new StatusResponse { Status = true, Message = "Đơn hàng đã được tạo thành công" };
            });

            Console.WriteLine($"[DEBUG] Transaction completed successfully");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception occurred in CreateOrderCODAsync: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            Console.WriteLine($"[DEBUG] Transaction was automatically rolled back");

            return new StatusResponse { Status = false, Message = $"Lỗi khi tạo đơn hàng: {ex.Message}" };
        }
    }
}