// Ví dụ về cách VoucherService xử lý nhiều shop và voucher:

// Input: OrderVirtualDto với nhiều shop
{
  "OrderId": "order-123",
  "ItemShops": [
    {
      "ShopId": "shop1",
      "ShopName": "Shop ABC",
      "Total": 150.00,
      "Products": [...]
    },
    {
      "ShopId": "shop2", 
      "ShopName": "Shop XYZ",
      "Total": 75.00,
      "Products": [...]
    }
  ]
}

// Process:
// 1. Lấy system vouchers (áp dụng cho toàn bộ đơn hàng)
// 2. Lấy shop vouchers cho từng shop:
//    - Shop1 vouchers: voucher1, voucher2
//    - Shop2 vouchers: voucher3, voucher4

// Output: CheckVoucherEligibilityResponse
{
  "VoucherTypes": [
    {
      "VoucherType": "System",
      "Vouchers": [
        {
          "IsAllowed": true,
          "Reason": "",
          "Voucher": {
            "Id": "sys-voucher-1",
            "Name": "Giảm 10% toàn đơn",
            "DiscountValue": 10,
            "MiniumOrderValue": 200
          }
        }
      ]
    },
    {
      "VoucherType": "Shop", 
      "Vouchers": [
        {
          "IsAllowed": true,
          "Reason": "",
          "Voucher": {
            "Id": "shop1-voucher-1",
            "Name": "Giảm 20% shop ABC",
            "DiscountValue": 20,
            "MiniumOrderValue": 100
          }
        },
        {
          "IsAllowed": false,
          "Reason": "Đơn hàng từ shop này phải có giá trị tối thiểu 200.00",
          "Voucher": {
            "Id": "shop2-voucher-1", 
            "Name": "Giảm 15% shop XYZ",
            "DiscountValue": 15,
            "MiniumOrderValue": 200
          }
        }
      ]
    }
  ]
}

// Logic:
// - System voucher: Kiểm tra tổng đơn hàng (150 + 75 = 225)
// - Shop1 voucher: Kiểm tra shop1 total (150) - Đủ điều kiện
// - Shop2 voucher: Kiểm tra shop2 total (75) - Không đủ điều kiện
