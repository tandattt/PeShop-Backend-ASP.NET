using PeShop.Models.Enums;

namespace PeShop.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Chuyển đổi OrderStatus sang text tiếng Việt
        /// </summary>
        public static string ToVietnameseString(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Chờ xử lý",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Rejected => "Từ chối",
                OrderStatus.PickedUp => "Đã lấy hàng",
                OrderStatus.Shipping => "Đang giao hàng",
                OrderStatus.Delivered => "Đã giao hàng",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Chuyển đổi PaymentMethod sang text tiếng Việt
        /// </summary>
        public static string ToVietnameseString(this PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.COD => "Thanh toán khi nhận hàng",
                PaymentMethod.CreditCard => "Thẻ tín dụng",
                PaymentMethod.MoMo => "Ví MoMo",
                PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
                PaymentMethod.ZaloPay => "Ví ZaloPay",
                PaymentMethod.VNPay => "Ví VNPay",
                _ => method.ToString()
            };
        }

        /// <summary>
        /// Chuyển đổi PaymentStatus sang text tiếng Việt
        /// </summary>
        public static string ToVietnameseString(this PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Unpaid => "Chưa thanh toán",
                PaymentStatus.Paid => "Đã thanh toán",
                PaymentStatus.Refunded => "Hoàn tiền",
                PaymentStatus.Failed => "Thanh toán thất bại",
                PaymentStatus.Processing => "Đang xử lý",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Chuyển đổi DeliveryStatus sang text tiếng Việt
        /// </summary>
        public static string ToVietnameseString(this DeliveryStatus status)
        {
            return status switch
            {
                DeliveryStatus.Ready_To_Pick => "Sẵn sàng để lấy hàng",
                DeliveryStatus.Picking => "Đang đi lấy hàng",
                DeliveryStatus.Cancel => "Đơn bị hủy",
                DeliveryStatus.Money_Collect_Picking => "Đang lấy hàng và thu COD",
                DeliveryStatus.Picked => "Đã lấy hàng xong",
                DeliveryStatus.Storing => "Đang lưu kho",
                DeliveryStatus.Transporting => "Đang vận chuyển",
                DeliveryStatus.Sorting => "Đang phân loại",
                DeliveryStatus.Delivering => "Đang giao hàng",
                DeliveryStatus.Money_Collect_Delivering => "Đang giao hàng và thu COD",
                DeliveryStatus.Delivered => "Giao hàng thành công",
                DeliveryStatus.Delivery_Fail => "Giao hàng thất bại",
                DeliveryStatus.Waiting_To_Return => "Chờ hoàn hàng",
                DeliveryStatus.Return => "Bắt đầu quy trình hoàn hàng",
                DeliveryStatus.Return_Transporting => "Đang vận chuyển hàng hoàn",
                DeliveryStatus.Return_Sorting => "Đang phân loại hàng hoàn",
                DeliveryStatus.Returning => "Đang trả hàng về người gửi",
                DeliveryStatus.Return_Fail => "Hoàn hàng thất bại",
                DeliveryStatus.Returned => "Đã hoàn hàng thành công",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Chuyển đổi PayoutStatus sang text tiếng Việt
        /// </summary>
        public static string ToVietnameseString(this PayoutStatus status)
        {
            return status switch
            {
                PayoutStatus.Pending => "Chờ xử lý",
                PayoutStatus.Completed => "Hoàn tất",
                PayoutStatus.Failed => "Thất bại",
                PayoutStatus.Processing => "Đang xử lý",
                PayoutStatus.Cancelled => "Đã hủy",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Chuyển đổi VoucherType sang text tiếng Việt
        /// </summary>
        public static string ToVietnameseString(this VoucherValueType? type)
        {
            
            return type switch
            {
                VoucherValueType.Percentage => "Giảm phần trăm",
                VoucherValueType.FixedAmount => "Giảm tiền",
                // VoucherValueType.FreeShipping => "Miễn phí vận chuyển",
                _ => type?.ToString() ?? string.Empty
            };
        }
        public static string ToVietnameseString(this RankLevel level)
        {
            return level switch
            {
                RankLevel.Bronze => "Đồng",
                RankLevel.Silver => "Bạc",
                RankLevel.Gold => "Vàng",
                RankLevel.Platinum => "Bạch kim",
                RankLevel.Diamond => "Kim cương",
                _ => level.ToString() ?? string.Empty
            };
        }

        public static string ToVietnameseString(this SenderType type)
        {
            return type switch
            {
                SenderType.User => "user",
                SenderType.Shop => "shop",
                _ => type.ToString()
            };
        }
    }
}
