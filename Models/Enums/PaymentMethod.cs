namespace PeShop.Models.Enums
{
    /// <summary>
    /// Phương thức thanh toán
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Thanh toán khi nhận hàng (COD)
        /// </summary>
        COD = 1,
        
        /// <summary>
        /// Thẻ tín dụng
        /// </summary>
        CreditCard = 2,
        
        /// <summary>
        /// Ví điện tử MoMo
        /// </summary>
        MoMo = 3,
        
        /// <summary>
        /// Chuyển khoản ngân hàng
        /// </summary>
        BankTransfer = 4,
        
        /// <summary>
        /// Ví điện tử ZaloPay
        /// </summary>
        ZaloPay = 5,
        
        /// <summary>
        /// Ví điện tử VNPay
        /// </summary>
        VNPay = 6
    }
}
