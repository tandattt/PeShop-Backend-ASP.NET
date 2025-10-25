
using System;
using System.Collections.Generic;
using PeShop.Models.Enums;

namespace PeShop.Models.Entities;

public partial class OrderVoucher
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public OrderVoucherType? Type { get; set; }

    public decimal VoucherPrice { get; set; }

    public string? VoucherName { get; set; }

    public string? OrderId { get; set; }

    public string? VoucherId { get; set; }

    public virtual Order? Order { get; set; }
}
