namespace PeShop.Models.Enums
{
    /// <summary>
    /// Loại voucher
    /// </summary>
    public enum VoucherType
    {
        /// <summary>
        /// Giảm phần trăm
        /// </summary>
        Percentage = 1,
        
        /// <summary>
        /// Giảm tiền
        /// </summary>
        FixedAmount = 2,
        
        /// <summary>
        /// Miễn phí vận chuyển
        /// </summary>
        FreeShipping = 3,
    
    }
}
