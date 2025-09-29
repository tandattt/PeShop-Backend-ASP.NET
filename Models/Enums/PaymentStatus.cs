namespace PeShop.Models.Enums
{
    /// <summary>
    /// Trạng thái thanh toán
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Chưa thanh toán
        /// </summary>
        Unpaid = 0,
        
        /// <summary>
        /// Đã thanh toán
        /// </summary>
        Paid = 1,
        
        /// <summary>
        /// Hoàn tiền
        /// </summary>
        Refunded = 2,
        
        /// <summary>
        /// Thanh toán thất bại
        /// </summary>
        Failed = 3,
        
        /// <summary>
        /// Đang xử lý thanh toán
        /// </summary>
        Processing = 4
    }
}
