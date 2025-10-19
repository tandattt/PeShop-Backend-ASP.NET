using System;
using System.Collections.Generic;

namespace PeShop.Models.Entities;

public partial class OrderVoucher
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public decimal? PriceVoucherShop { get; set; }

    public decimal? PriceVoucherSystem { get; set; }

    public string? VoucherShopName { get; set; }

    public string? VoucherSystemName { get; set; }

    public string? OrderDetailId { get; set; }

    public virtual OrderDetail? OrderDetail { get; set; }
}
