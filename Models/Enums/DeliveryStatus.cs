namespace PeShop.Models.Enums
{
    /// <summary>
    /// Trạng thái giao hàng GHN
    /// </summary>
    public enum DeliveryStatus
    {
        /// <summary>
        /// Sẵn sàng để lấy hàng
        /// </summary>
        Ready_To_Pick = 0,
        
        /// <summary>
        /// Đang đi lấy hàng
        /// </summary>
        Picking = 1,
        
        /// <summary>
        /// Đơn bị hủy
        /// </summary>
        Cancel = 2,
        
        /// <summary>
        /// Đang lấy hàng và thu COD
        /// </summary>
        Money_Collect_Picking = 3,
        
        /// <summary>
        /// Đã lấy hàng xong
        /// </summary>
        Picked = 4,
        
        /// <summary>
        /// Đang lưu kho
        /// </summary>
        Storing = 5,
        
        /// <summary>
        /// Đang vận chuyển
        /// </summary>
        Transporting = 6,
        
        /// <summary>
        /// Đang phân loại
        /// </summary>
        Sorting = 7,
        
        /// <summary>
        /// Đang giao hàng
        /// </summary>
        Delivering = 8,
        
        /// <summary>
        /// Đang giao hàng và thu COD
        /// </summary>
        Money_Collect_Delivering = 9,
        
        /// <summary>
        /// Giao hàng thành công
        /// </summary>
        Delivered = 10,
        
        /// <summary>
        /// Giao hàng thất bại
        /// </summary>
        Delivery_Fail = 11,
        
        /// <summary>
        /// Chờ hoàn hàng
        /// </summary>
        Waiting_To_Return = 12,
        
        /// <summary>
        /// Bắt đầu quy trình hoàn hàng
        /// </summary>
        Return = 13,
        
        /// <summary>
        /// Đang vận chuyển hàng hoàn
        /// </summary>
        Return_Transporting = 14,
        
        /// <summary>
        /// Đang phân loại hàng hoàn
        /// </summary>
        Return_Sorting = 15,
        
        /// <summary>
        /// Đang trả hàng về người gửi
        /// </summary>
        Returning = 16,
        
        /// <summary>
        /// Hoàn hàng thất bại
        /// </summary>
        Return_Fail = 17,
        
        /// <summary>
        /// Đã hoàn hàng thành công
        /// </summary>
        Returned = 18
    }
}
