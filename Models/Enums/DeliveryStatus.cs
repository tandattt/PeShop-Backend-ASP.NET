namespace PeShop.Models.Enums
{
    /// <summary>
    /// Trạng thái giao hàng
    /// </summary>
    public enum DeliveryStatus
    {
        /// <summary>
        /// Chưa giao hàng
        /// </summary>
        NotDelivered = 0,
        
        /// <summary>
        /// Đang giao hàng
        /// </summary>
        Delivering = 1,
        
        /// <summary>
        /// Đã nhận hàng
        /// </summary>
        Delivered = 2,
        
        /// <summary>
        /// Giao hàng thất bại
        /// </summary>
        DeliveryFailed = 3,
        
        /// <summary>
        /// Đã chuẩn bị hàng
        /// </summary>
        Prepared = 4
    }
}
