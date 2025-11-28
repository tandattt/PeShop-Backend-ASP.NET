# Flash Sale API Integration Guide

## 📋 Tổng quan

Flash Sale là tính năng giảm giá đặc biệt trong khoảng thời gian giới hạn. Các sản phẩm có flash sale sẽ được đánh dấu và hiển thị giá giảm.

### Điều kiện Flash Sale Active:
- ✅ `status = 1` (Active)
- ✅ Thời gian hiện tại nằm trong khoảng `start_time` và `end_time`
- ✅ Sản phẩm có `status = Active`

---

## 🔌 API Endpoints

### 1. Lấy danh sách sản phẩm (có Flash Sale)

#### **GET** `/Product/get-products`

Lấy danh sách sản phẩm với phân trang, bao gồm thông tin flash sale.

#### **Request**

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `CategoryId` | string | No | null | Lọc theo category ID |
| `CategoryChildId` | string | No | null | Lọc theo category con ID |
| `MinPrice` | decimal | No | 0 | Giá tối thiểu |
| `MaxPrice` | decimal | No | null | Giá tối đa |
| `ReviewPoint` | float | No | null | Điểm đánh giá tối thiểu |
| `Page` | int | No | 1 | Trang hiện tại |
| `PageSize` | int | No | 20 | Số lượng sản phẩm mỗi trang |

#### **Request Example:**

```http
GET /Product/get-products?Page=1&PageSize=20&MinPrice=0&MaxPrice=500000
```

```javascript
// JavaScript/TypeScript
const response = await fetch('/Product/get-products?Page=1&PageSize=20', {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json'
  }
});

const data = await response.json();
```

#### **Response - Success (200 OK):**

```json
{
  "error": null,
  "data": {
    "data": [
      {
        "id": "fdc94fee-e28c-4eb8-9088-48c2193bc798",
        "name": "Combo 3 Áo Thun Nam Ba Lỗ Viền Móng Cotton FREEMAN ASF297",
        "image": "https://salt.tikicdn.com/cache/w1200/ts/product/99/61/72/7788d34s103e50e9d022651ad3a712e8.jpg",
        "reviewCount": 0,
        "reviewPoint": 0,
        "price": 178000,
        "boughtCount": 0,
        "addressShop": "550000",
        "slug": "combo-3-ao-thun-nam-ba-lo-vien-mong-cotton-freeman-asf207-9760454",
        "shopId": "309018c0-b70b-11f0-b68d-22e843586b17",
        "shopName": "Shop Dụng Gentleman",
        "hasPromotion": false,
        "hasFlashSale": true,
        "flashSalePrice": 142400
      },
      {
        "id": "001eae8f-1009-41fe-9ed5-448f2eabb3ad2",
        "name": "Áo thun nam cổ tròn",
        "image": "https://example.com/product2.jpg",
        "reviewCount": 5,
        "reviewPoint": 4.5,
        "price": 250000,
        "boughtCount": 10,
        "addressShop": "700000",
        "slug": "ao-thun-nam-co-tron",
        "shopId": "shop-id-2",
        "shopName": "Shop ABC",
        "hasPromotion": false,
        "hasFlashSale": false,
        "flashSalePrice": null
      }
    ],
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "nextPage": 2,
    "previousPage": 1
  }
}
```

#### **Response Fields:**

##### **Product Object:**

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | ID sản phẩm |
| `name` | string | Tên sản phẩm |
| `image` | string | URL hình ảnh chính |
| `reviewCount` | number | Số lượng đánh giá |
| `reviewPoint` | number | Điểm đánh giá trung bình |
| `price` | number | **Giá gốc** của sản phẩm |
| `boughtCount` | number | Số lượng đã bán |
| `addressShop` | string | Địa chỉ shop |
| `slug` | string | URL-friendly slug |
| `shopId` | string | ID của shop |
| `shopName` | string | Tên shop |
| `hasPromotion` | boolean | Có promotion không |
| `hasFlashSale` | boolean | **Có flash sale không** |
| `flashSalePrice` | number\|null | **Giá sau khi giảm flash sale** (null nếu không có flash sale) |

##### **Pagination Object:**

| Field | Type | Description |
|-------|------|-------------|
| `data` | array | Mảng các sản phẩm |
| `currentPage` | number | Trang hiện tại |
| `pageSize` | number | Số sản phẩm mỗi trang |
| `totalCount` | number | Tổng số sản phẩm |
| `totalPages` | number | Tổng số trang |
| `hasNextPage` | boolean | Có trang tiếp theo không |
| `hasPreviousPage` | boolean | Có trang trước không |
| `nextPage` | number | Số trang tiếp theo |
| `previousPage` | number | Số trang trước |

---

### 2. Lấy danh sách sản phẩm theo Shop

#### **GET** `/Product/get-products-by-shop`

Lấy danh sách sản phẩm của một shop cụ thể.

#### **Request**

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `ShopId` | string | **Yes** | ID của shop |
| `Page` | int | No | Trang hiện tại (default: 1) |
| `PageSize` | int | No | Số sản phẩm mỗi trang (default: 20) |

#### **Request Example:**

```http
GET /Product/get-products-by-shop?ShopId=309018c0-b70b-11f0-b68d-22e843586b17&Page=1&PageSize=20
```

#### **Response:** Giống như endpoint `get-products`

---

### 3. Lấy chi tiết sản phẩm

#### **GET** `/Product/get-product-detail`

Lấy thông tin chi tiết của một sản phẩm (chưa có flash sale info trong response này).

#### **Request**

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `productId` | string | No* | ID của sản phẩm |
| `slug` | string | No* | Slug của sản phẩm |

*Một trong hai parameter phải được cung cấp.

#### **Request Example:**

```http
GET /Product/get-product-detail?productId=fdc94fee-e28c-4eb8-9088-48c2193bc798
```

hoặc

```http
GET /Product/get-product-detail?slug=combo-3-ao-thun-nam-ba-lo-vien-mong-cotton-freeman-asf207-9760454
```

---

## 💻 Frontend Implementation

### React/Next.js Example

```typescript
// types/product.ts
export interface Product {
  id: string;
  name: string;
  image: string;
  reviewCount: number;
  reviewPoint: number;
  price: number;
  boughtCount: number;
  addressShop: string;
  slug: string;
  shopId: string;
  shopName: string;
  hasPromotion: boolean;
  hasFlashSale: boolean;
  flashSalePrice: number | null;
}

export interface PaginationResponse<T> {
  data: T[];
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  nextPage: number;
  previousPage: number;
}

// services/productService.ts
export const getProducts = async (params: {
  page?: number;
  pageSize?: number;
  minPrice?: number;
  maxPrice?: number;
  categoryId?: string;
  categoryChildId?: string;
  reviewPoint?: number;
}): Promise<PaginationResponse<Product>> => {
  const queryParams = new URLSearchParams();
  
  if (params.page) queryParams.append('Page', params.page.toString());
  if (params.pageSize) queryParams.append('PageSize', params.pageSize.toString());
  if (params.minPrice !== undefined) queryParams.append('MinPrice', params.minPrice.toString());
  if (params.maxPrice !== undefined) queryParams.append('MaxPrice', params.maxPrice.toString());
  if (params.categoryId) queryParams.append('CategoryId', params.categoryId);
  if (params.categoryChildId) queryParams.append('CategoryChildId', params.categoryChildId);
  if (params.reviewPoint) queryParams.append('ReviewPoint', params.reviewPoint.toString());

  const response = await fetch(`/Product/get-products?${queryParams}`, {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
    },
  });

  const result = await response.json();
  return result.data;
};

// components/ProductCard.tsx
import React from 'react';
import { Product } from '../types/product';

interface ProductCardProps {
  product: Product;
}

export const ProductCard: React.FC<ProductCardProps> = ({ product }) => {
  // Tính phần trăm giảm giá
  const discountPercent = product.hasFlashSale && product.flashSalePrice
    ? Math.round(((product.price - product.flashSalePrice) / product.price) * 100)
    : 0;

  return (
    <div className="product-card">
      {/* Flash Sale Badge */}
      {product.hasFlashSale && (
        <div className="flash-sale-badge">
          <span>⚡ Flash Sale</span>
          <span>-{discountPercent}%</span>
        </div>
      )}

      {/* Product Image */}
      <img src={product.image} alt={product.name} />

      {/* Product Info */}
      <h3>{product.name}</h3>

      {/* Price Display */}
      <div className="price-container">
        {product.hasFlashSale && product.flashSalePrice ? (
          <>
            <span className="flash-sale-price">
              {product.flashSalePrice.toLocaleString('vi-VN')}₫
            </span>
            <span className="original-price strikethrough">
              {product.price.toLocaleString('vi-VN')}₫
            </span>
          </>
        ) : (
          <span className="normal-price">
            {product.price.toLocaleString('vi-VN')}₫
          </span>
        )}
      </div>

      {/* Shop Info */}
      <div className="shop-info">
        <span>{product.shopName}</span>
      </div>

      {/* Stats */}
      <div className="product-stats">
        <span>⭐ {product.reviewPoint} ({product.reviewCount})</span>
        <span>Đã bán: {product.boughtCount}</span>
      </div>
    </div>
  );
};

// pages/products.tsx
import React, { useEffect, useState } from 'react';
import { getProducts } from '../services/productService';
import { ProductCard } from '../components/ProductCard';
import { Product, PaginationResponse } from '../types/product';

export const ProductsPage: React.FC = () => {
  const [products, setProducts] = useState<PaginationResponse<Product> | null>(null);
  const [loading, setLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);

  useEffect(() => {
    const fetchProducts = async () => {
      setLoading(true);
      try {
        const data = await getProducts({ page: currentPage, pageSize: 20 });
        setProducts(data);
      } catch (error) {
        console.error('Error fetching products:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchProducts();
  }, [currentPage]);

  if (loading) return <div>Loading...</div>;
  if (!products) return <div>No products found</div>;

  return (
    <div className="products-page">
      <h1>Sản phẩm</h1>
      
      {/* Filter Flash Sale */}
      <div className="filters">
        <button onClick={() => {
          // Filter products with flash sale
          const flashSaleProducts = products.data.filter(p => p.hasFlashSale);
          console.log('Flash sale products:', flashSaleProducts);
        }}>
          ⚡ Chỉ hiển thị Flash Sale
        </button>
      </div>

      {/* Product Grid */}
      <div className="product-grid">
        {products.data.map((product) => (
          <ProductCard key={product.id} product={product} />
        ))}
      </div>

      {/* Pagination */}
      <div className="pagination">
        <button
          disabled={!products.hasPreviousPage}
          onClick={() => setCurrentPage(products.previousPage)}
        >
          Trang trước
        </button>
        <span>
          Trang {products.currentPage} / {products.totalPages}
        </span>
        <button
          disabled={!products.hasNextPage}
          onClick={() => setCurrentPage(products.nextPage)}
        >
          Trang sau
        </button>
      </div>
    </div>
  );
};
```

### CSS Example

```css
/* Flash Sale Styling */
.flash-sale-badge {
  position: absolute;
  top: 10px;
  left: 10px;
  background: linear-gradient(135deg, #ff6b6b 0%, #ee5a52 100%);
  color: white;
  padding: 5px 10px;
  border-radius: 5px;
  display: flex;
  gap: 5px;
  font-weight: bold;
  font-size: 12px;
  z-index: 10;
  box-shadow: 0 2px 8px rgba(255, 107, 107, 0.3);
}

.flash-sale-badge span:first-child {
  display: flex;
  align-items: center;
  gap: 3px;
}

.price-container {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 10px 0;
}

.flash-sale-price {
  font-size: 20px;
  font-weight: bold;
  color: #ff6b6b;
}

.original-price {
  font-size: 14px;
  color: #999;
}

.strikethrough {
  text-decoration: line-through;
}

.normal-price {
  font-size: 18px;
  font-weight: bold;
  color: #333;
}

.product-card {
  position: relative;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 15px;
  transition: transform 0.2s, box-shadow 0.2s;
}

.product-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);
}
```

---

## 📝 Lưu ý quan trọng

### 1. **Hiển thị giá**
- Nếu `hasFlashSale === true`: Hiển thị `flashSalePrice` là giá chính, `price` gạch ngang
- Nếu `hasFlashSale === false`: Hiển thị `price` bình thường

### 2. **Tính phần trăm giảm giá**
```javascript
const discountPercent = (originalPrice - flashSalePrice) / originalPrice * 100;
```

### 3. **Kiểm tra Flash Sale**
```javascript
const isFlashSale = product.hasFlashSale && product.flashSalePrice !== null;
```

### 4. **Filter Flash Sale Products**
```javascript
const flashSaleProducts = products.filter(p => p.hasFlashSale === true);
```

### 5. **Sort theo giá Flash Sale**
```javascript
const sortedProducts = products.sort((a, b) => {
  const priceA = a.hasFlashSale && a.flashSalePrice ? a.flashSalePrice : a.price;
  const priceB = b.hasFlashSale && b.flashSalePrice ? b.flashSalePrice : b.price;
  return priceA - priceB;
});
```

---

## 🎨 UI/UX Best Practices

1. **Badge nổi bật**: Dùng màu đỏ/cam, icon ⚡ để thu hút attention
2. **Countdown timer**: Thêm đếm ngược thời gian kết thúc flash sale
3. **Progress bar**: Hiển thị % sản phẩm đã bán
4. **Animation**: Thêm hiệu ứng nhấp nháy nhẹ cho flash sale badge
5. **Mobile responsive**: Đảm bảo hiển thị tốt trên mobile

---

## 🔧 Testing

### Test Cases

1. ✅ Sản phẩm có flash sale hiển thị đúng badge
2. ✅ Giá flash sale được hiển thị, giá gốc bị gạch ngang
3. ✅ Sản phẩm không có flash sale hiển thị giá bình thường
4. ✅ Filter chỉ hiển thị sản phẩm flash sale hoạt động
5. ✅ Pagination hoạt động đúng
6. ✅ Sort theo giá flash sale hoạt động
7. ✅ Responsive trên mobile/tablet

---

## 📞 Support

Nếu có vấn đề trong quá trình tích hợp, vui lòng liên hệ Backend team hoặc kiểm tra:
- Console log debug trong `FlashSaleRepository`
- Database: table `flash_sale` và `flash_sale_product`
- Đảm bảo `status = 1` và thời gian hợp lệ

---

**Version:** 1.0  
**Last Updated:** November 2025  
**Author:** PeShop Backend Team



