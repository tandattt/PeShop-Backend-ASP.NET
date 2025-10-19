// Ví dụ về cách OrderVirtualDto sẽ được tạo với thông tin shop:

// Input: OrderVirtualRequest với các sản phẩm từ nhiều shop khác nhau
{
  "Items": [
    {
      "ProductId": "product1",
      "VariantId": null,
      "Quantity": 2,
      "ShopId": "shop1"
    },
    {
      "ProductId": "product2", 
      "VariantId": "variant1",
      "Quantity": 1,
      "ShopId": "shop1"
    },
    {
      "ProductId": "product3",
      "VariantId": null,
      "Quantity": 3,
      "ShopId": "shop2"
    }
  ]
}

// Output: OrderVirtualDto với các sản phẩm được nhóm theo shop và có thông tin shop
{
  "OrderId": "guid-123",
  "UserId": "user123",
  "CreatedAt": "2024-01-01T10:00:00Z",
  "ItemShops": [
    {
      "ShopId": "shop1",
      "ShopName": "Shop ABC",
      "ShopLogoUrl": "https://example.com/logo1.jpg",
      "Products": [
        {
          "ProductId": "product1",
          "VariantId": null,
          "Quantity": 2,
          "ShopId": "shop1"
        },
        {
          "ProductId": "product2",
          "VariantId": "variant1", 
          "Quantity": 1,
          "ShopId": "shop1"
        }
      ],
      "Total": 150.00
    },
    {
      "ShopId": "shop2",
      "ShopName": "Shop XYZ",
      "ShopLogoUrl": "https://example.com/logo2.jpg",
      "Products": [
        {
          "ProductId": "product3",
          "VariantId": null,
          "Quantity": 3,
          "ShopId": "shop2"
        }
      ],
      "Total": 75.00
    }
  ]
}
