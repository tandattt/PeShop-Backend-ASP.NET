namespace PeShop.Models.Enums
{
    /// <summary>
    /// Trạng thái đơn hàng
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Chờ xử lý
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Đã xác nhận
        /// </summary>
        Confirmed = 1,
        
        /// <summary>
        /// Từ chối
        /// </summary>
        Rejected = 2,
        
        /// <summary>
        /// Đã lấy hàng
        /// </summary>
        PickedUp = 3,
        
        /// <summary>
        /// Đang giao hàng
        /// </summary>
        Shipping = 4,
        
        /// <summary>
        /// Đã giao hàng
        /// </summary>
        Delivered = 5,


        /// đã hủy
        Cancelled  = 6
    }
}
