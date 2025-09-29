namespace PeShop.Constants
{
    public static class ProductConstants
    {
        public static class Status
        {
            /// <summary>
            /// Không hoạt động
            /// </summary>
            public const string Inactive = "inactive";
            
            /// <summary>
            /// Đang hoạt động
            /// </summary>
            public const string Active = "active";
            
            /// <summary>
            /// Tạm ngưng
            /// </summary>
            public const string Suspended = "suspended";
            
            /// <summary>
            /// Hết hàng
            /// </summary>
            public const string OutOfStock = "out_of_stock";
            
            /// <summary>
            /// Đã xóa
            /// </summary>
            public const string Deleted = "deleted";
        }

        /// <summary>
        /// Chuyển đổi status text sang text tiếng Việt
        /// </summary>
        /// <param name="status">Status text từ database</param>
        /// <returns>Text tiếng Việt</returns>
        public static string GetStatusText(string status)
        {
            return status switch
            {
                Status.Inactive => "Không hoạt động",
                Status.Active => "Đang hoạt động",
                Status.Suspended => "Tạm ngưng",
                Status.OutOfStock => "Hết hàng",
                Status.Deleted => "Đã xóa",
                _ => "Không xác định"
            };
        }

        /// <summary>
        /// Kiểm tra status có hợp lệ không
        /// </summary>
        /// <param name="status">Status text</param>
        /// <returns>True nếu hợp lệ</returns>
        public static bool IsValidStatus(string status)
        {
            return status == Status.Inactive ||
                   status == Status.Active ||
                   status == Status.Suspended ||
                   status == Status.OutOfStock ||
                   status == Status.Deleted;
        }

        /// <summary>
        /// Lấy tất cả status values
        /// </summary>
        /// <returns>Array các status values</returns>
        public static string[] GetAllStatuses()
        {
            return new[]
            {
                Status.Inactive,
                Status.Active,
                Status.Suspended,
                Status.OutOfStock,
                Status.Deleted
            };
        }
    }
}
