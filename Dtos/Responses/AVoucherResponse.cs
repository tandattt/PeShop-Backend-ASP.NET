using PeShop.Models.Enums;

namespace PeShop.Dtos.Responses;

public class AVoucherResponse
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Mã voucher
    public string Name { get; set; } = string.Empty; // Tên voucher
    public VoucherValueType Type { get; set; } // Loại (Percentage hoặc FixedAmount)
    public string TypeName { get; set; } = string.Empty; // Tên loại (ví dụ: "Phần trăm", "Tiền")
    public uint MiniumOrderValue { get; set; } // Đơn tối thiểu
    public uint QuantityUsed { get; set; } // Đã dùng
    public uint Quantity { get; set; } // Tổng số lượng
    public DateTime? EndTime { get; set; } // Hết hạn
    public VoucherStatus? Status { get; set; } // Trạng thái
    public string StatusName { get; set; } = string.Empty; // Tên trạng thái
    public uint DiscountValue { get; set; } // Giá trị giảm
    public uint? MaxdiscountAmount { get; set; } // Số tiền giảm tối đa
    public DateTime? StartTime { get; set; } // Thời gian bắt đầu
    public DateTime? CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByName { get; set; }
}

