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
                DeliveryStatus.NotDelivered => "Chưa giao hàng",
                DeliveryStatus.Delivering => "Đang giao hàng",
                DeliveryStatus.Delivered => "Đã nhận hàng",
                DeliveryStatus.DeliveryFailed => "Giao hàng thất bại",
                DeliveryStatus.Prepared => "Đã chuẩn bị hàng",
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
                VoucherValueType.FreeShipping => "Miễn phí vận chuyển",
                _ => null
            };
        }

    }
}
