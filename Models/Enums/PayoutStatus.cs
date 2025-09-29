namespace PeShop.Models.Enums
{
    /// <summary>
    /// Trạng thái thanh toán cho shop
    /// </summary>
    public enum PayoutStatus
    {
        /// <summary>
        /// Chờ xử lý
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Hoàn tất
        /// </summary>
        Completed = 1,
        
        /// <summary>
        /// Thất bại
        /// </summary>
        Failed = 2,
        
        /// <summary>
        /// Đang xử lý
        /// </summary>
        Processing = 3,
        
        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled = 4
    }
}
